import { Component, OnInit } from '@angular/core';
import { RouterOutlet, RouterModule, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './services/auth.service';
import { TranslateService, TranslateModule } from '@ngx-translate/core';
import { MsalService, MsalBroadcastService } from '@azure/msal-angular';
import { EventType, AuthenticationResult, InteractionStatus } from '@azure/msal-browser';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterModule, CommonModule, TranslateModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  constructor(
    public authService: AuthService,
    private router: Router,
    public translate: TranslateService,
    private msalService: MsalService,
    private msalBroadcastService: MsalBroadcastService
  ) {
    this.translate.addLangs(['en', 'pt']);
    this.translate.setDefaultLang('en');
    this.translate.use('en');
  }

  ngOnInit(): void {
    // Processa o redirect de volta da Microsoft após autenticação
    this.msalService.handleRedirectObservable({ navigateToLoginRequestUrl: false }).subscribe();

    // Quando o MSAL terminar de processar o login, carrega o utilizador da API
    this.msalBroadcastService.msalSubject$
      .pipe(filter(e => e.eventType === EventType.LOGIN_SUCCESS))
      .subscribe(() => {
        this.authService.loadCurrentUser().subscribe(() => {
          this.router.navigate(['/books']);
        });
      });

    // Se já há uma conta MSAL activa mas ainda não temos utilizador local, carrega-o
    this.msalBroadcastService.inProgress$
      .pipe(filter(status => status === InteractionStatus.None))
      .subscribe(() => {
        const accounts = this.msalService.instance.getAllAccounts();
        if (accounts.length > 0 && !this.authService.currentUser()) {
          this.authService.loadCurrentUser().subscribe();
        }
      });
  }

  get isLoginPage(): boolean {
    return this.router.url.includes('/login');
  }

  switchLanguage(lang: string) {
    this.translate.use(lang);
  }

  logout() {
    this.authService.logout();
  }
}
