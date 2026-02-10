import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { Client } from '../models/client.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;
  // Signal to hold current user state
  currentUser = signal<Client | null>(null);

  constructor(private http: HttpClient) {
    // Try to restore user from local storage on init
    const storedUser = localStorage.getItem('currentUser');
    if (storedUser) {
      this.currentUser.set(JSON.parse(storedUser));
    }
  }

  login(credentials: { email: string, password: string }): Observable<Client> {
    return this.http.post<Client>(`${this.apiUrl}/login`, credentials).pipe(
      tap(user => {
        this.currentUser.set(user);
        localStorage.setItem('currentUser', JSON.stringify(user));
      })
    );
  }

  register(data: { name: string, email: string, password: string }): Observable<Client> {
    return this.http.post<Client>(`${this.apiUrl}/register`, data);
  }

  googleLogin(idToken: string): Observable<Client> {
    console.log('AuthService: Sending Google ID Token to backend...', `${this.apiUrl}/google-login`);
    return this.http.post<Client>(`${this.apiUrl}/google-login`, { idToken }).pipe(
      tap(user => {
        console.log('AuthService: Google Login Response:', user);
        this.currentUser.set(user);
        localStorage.setItem('currentUser', JSON.stringify(user));
      })
    );
  }

  logout(): void {
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
