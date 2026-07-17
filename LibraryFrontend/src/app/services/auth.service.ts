import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { MsalService } from '@azure/msal-angular';
import { Client } from '../models/client.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;
  currentUser = signal<Client | null>(null);

  constructor(
    private http: HttpClient,
    private msalService: MsalService
  ) {
    const storedUser = localStorage.getItem('currentUser');
    if (storedUser) {
      this.currentUser.set(JSON.parse(storedUser));
    }
  }

  login(): void {
    this.msalService.loginRedirect();
  }

  loadCurrentUser(): Observable<Client> {
    return this.http.get<Client>(`${this.apiUrl}/me`).pipe(
      tap(user => {
        this.currentUser.set(user);
        localStorage.setItem('currentUser', JSON.stringify(user));
      })
    );
  }

  logout(): void {
    this.clearCurrentUser();
    this.msalService.logoutRedirect();
  }

  clearCurrentUser(): void {
    this.currentUser.set(null);
    localStorage.removeItem('currentUser');
  }

  get isAdmin(): boolean {
    return this.currentUser()?.role === 'Admin';
  }

  get isLoggedIn(): boolean {
    return !!this.currentUser();
  }
}
