import { Component, signal } from '@angular/core';
import { RouterOutlet, RouterModule, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './services/auth.service';
import { TranslateService, TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterModule, CommonModule, TranslateModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  constructor(
    public authService: AuthService,
    private router: Router,
    public translate: TranslateService
  ) {
    this.translate.addLangs(['en', 'pt']);
    this.translate.setDefaultLang('en');

    // Use 'en' by default
    this.translate.use('en');
  }

  get isLoginPage(): boolean {
    return this.router.url.includes('/login') || this.router.url.includes('/register');
  }

  switchLanguage(lang: string) {
    this.translate.use(lang);
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
