import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { TranslateModule } from '@ngx-translate/core';
import { GoogleSigninButtonModule, SocialAuthService, SocialUser } from '@abacritt/angularx-social-login';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, TranslateModule, GoogleSigninButtonModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login implements OnInit {
  loginForm: FormGroup;
  error: string = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private socialAuthService: SocialAuthService
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  ngOnInit() {
    this.socialAuthService.authState.subscribe((user: SocialUser) => {
      console.log('Google Auth State Changed:', user);
      if (user && user.idToken) {
        console.log('Google User ID Token found, initiating backend login...');
        this.authService.googleLogin(user.idToken).subscribe({
          next: (res) => {
            console.log('Backend Google Login Success:', res);
            this.router.navigate(['/books']);
          },
          error: (err) => {
            console.error('Backend Google Login Failed:', err);
            this.error = 'Google Login failed';
          }
        });
      } else {
        console.log('No Google User or ID Token present.');
      }
    });
  }

  onSubmit(): void {
    if (this.loginForm.invalid) return;

    this.authService.login(this.loginForm.value).subscribe({
      next: () => {
        this.router.navigate(['/books']);
      },
      error: (err) => {
        this.error = 'Invalid email or password';
      }
    });
  }
}
