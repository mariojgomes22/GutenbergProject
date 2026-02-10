import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { ClientService } from '../../services/client.service';
import { LoanService } from '../../services/loan.service';
import { Client } from '../../models/client.model';
import { Loan } from '../../models/loan.model';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
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
    private loanService: LoanService
  ) {
    this.profileForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.minLength(3)]]
    });
  }

  ngOnInit(): void {
    const user = this.authService.currentUser();
    if (user) {
      this.currentUser = user;
      this.profileForm.patchValue({
        name: user.name,
        email: user.email,
        password: user.password
      });

      this.loadMyLoans(user.id);
    }
  }

  loadMyLoans(userId: number): void {
    this.loanService.getLoans(userId).subscribe(data => {
      this.myLoans = data;
    });
  }

  onSubmit(): void {
    if (this.profileForm.invalid || !this.currentUser) return;

    const updatedData = { ...this.currentUser, ...this.profileForm.value };

    // Ensure ID is present for update
    updatedData.id = this.currentUser.id;
    // Keep role same as before
    updatedData.role = this.currentUser.role;

    this.clientService.updateClient(this.currentUser.id, updatedData).subscribe({
      next: () => {
        // Update local auth state
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
