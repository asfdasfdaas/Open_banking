import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  // Pointing to .NET AuthController
  private baseUrl = 'https://localhost:7277/api/Auth';

  constructor(private http: HttpClient) { }

  register(userData: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/register`, userData);
  }

  login(credentials: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/login`, credentials).pipe(
      tap((response: any) => {
        if (response.token) {
          sessionStorage.setItem('jwt_token', response.token);
        }
      })
    );
  }

  getToken(): string | null {
    return sessionStorage.getItem('jwt_token');
  }

  isLoggedIn(): boolean {
    return !!this.getToken(); // Returns true if token exists, false if it doesn't
  }

  logout(): void {
    sessionStorage.removeItem('jwt_token');
    localStorage.removeItem('jwt_token');
  }

  saveVakifbankConsent(consentId: string): Observable<any> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    return this.http.post(`${this.baseUrl}/save-vakifbank-consent`, `"${consentId}"`, { headers });
  }
}
