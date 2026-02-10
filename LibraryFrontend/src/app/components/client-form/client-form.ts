import { Component, OnInit, Inject, Optional } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ClientService } from '../../services/client.service';
import { Client } from '../../models/client.model';
import { TranslateModule } from '@ngx-translate/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-client-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, TranslateModule, MatDialogModule, MatButtonModule],
  templateUrl: './client-form.html',
  styleUrl: './client-form.css'
})
export class ClientForm implements OnInit {
  clientForm: FormGroup;
  isEditMode = false;
  clientId: number | null = null;
  submitted = false;

  get isModal(): boolean {
    return !!this.dialogRef;
  }

  constructor(
    private fb: FormBuilder,
    private clientService: ClientService,
    private route: ActivatedRoute,
    private router: Router,
    @Optional() public dialogRef: MatDialogRef<ClientForm>,
    @Optional() @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.clientForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: [''],
      role: ['User', Validators.required]
    });
  }

  ngOnInit(): void {
    if (this.data && this.data.id) {
      this.isEditMode = true;
      this.clientId = this.data.id;
      if (this.clientId) {
        this.loadClient(this.clientId);
      }
    } else {
      const id = this.route.snapshot.paramMap.get('id');
      if (id) {
        this.isEditMode = true;
        this.clientId = +id;
        this.loadClient(this.clientId);
      }
    }
  }

  loadClient(id: number) {
    this.clientService.getClient(id).subscribe(client => {
      this.clientForm.patchValue(client);
    });
  }

  onSubmit(): void {
    this.submitted = true;
    if (this.clientForm.invalid) {
      return;
    }

    const clientData = this.clientForm.value;

    if (this.isEditMode && this.clientId) {
      clientData.id = this.clientId;
      this.clientService.updateClient(this.clientId, clientData).subscribe(() => {
        this.handleSuccess();
      });
    } else {
      this.clientService.createClient(clientData).subscribe(() => {
        this.handleSuccess();
      });
    }
  }

  handleSuccess() {
    if (this.dialogRef) {
      this.dialogRef.close(true);
    } else {
      this.router.navigate(['/clients']);
    }
  }

  onCancel() {
    if (this.dialogRef) {
      this.dialogRef.close();
    } else {
      this.router.navigate(['/clients']);
    }
  }
}
