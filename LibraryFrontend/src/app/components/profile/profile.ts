import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { ClientService } from '../../services/client.service';
import { LoanService } from '../../services/loan.service';
import { Client } from '../../models/client.model';
import { Loan } from '../../models/loan.model';
import { TranslateModule } from '@ngx-translate/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, MatDialogModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css'
})
export class Profile implements OnInit {
  profileForm: FormGroup;
  message: string = '';
  currentUser: Client | null = null;
  myLoans: Loan[] = [];
  activeTab: 'profile' | 'loans' = 'profile';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private clientService: ClientService,
    private loanService: LoanService,
    private dialog: MatDialog
  ) {
    this.profileForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]]
    });
  }

  ngOnInit(): void {
    const user = this.authService.currentUser();
    if (user) {
      this.currentUser = user;
      this.profileForm.patchValue({
        name: user.name,
        email: user.email
      });

      this.loadMyLoans(user.id);
    }
  }

  loadMyLoans(userId: number): void {
    this.loanService.getLoans(userId).subscribe(data => {
      this.myLoans = data;
    });
  }

  returnLoan(id: number): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '350px',
      data: {
        title: 'Return Book',
        message: 'Return this book?'
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result && this.currentUser) {
        this.loanService.returnLoan(id).subscribe({
          next: () => this.loadMyLoans(this.currentUser!.id),
          error: (err) => console.error(err)
        });
      }
    });
  }

  onSubmit(): void {
    if (this.profileForm.invalid || !this.currentUser) return;

    const updatedData = { ...this.currentUser, ...this.profileForm.value };
    updatedData.id = this.currentUser.id;
    updatedData.role = this.currentUser.role;

    this.clientService.updateClient(this.currentUser.id, updatedData).subscribe({
      next: () => {
        this.authService.currentUser.set(updatedData);
        localStorage.setItem('currentUser', JSON.stringify(updatedData));

        this.message = 'Profile updated successfully!';
        setTimeout(() => this.message = '', 3000);
      },
      error: () => {
        this.message = 'Error updating profile.';
      }
    });
  }
}
