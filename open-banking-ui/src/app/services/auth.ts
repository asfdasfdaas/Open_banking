import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  // Pointing to your .NET AuthController
  private baseUrl = 'https://localhost:7277/api/Auth';

  constructor(private http: HttpClient) { }

  register(userData: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/register`, userData);
  }

  login(credentials: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/login`, credentials).pipe(
      tap((response: any) => {
        if (response.token) {
          localStorage.setItem('jwt_token', response.token);
        }
      })
    );
  }

  // A helper function to grab the token later
  getToken(): string | null {
    return localStorage.getItem('jwt_token');
  }

  isLoggedIn(): boolean {
    return !!this.getToken(); // Returns true if token exists, false if it doesn't
  }

  logout(): void {
    localStorage.removeItem('jwt_token');
  }

  saveVakifbankConsent(consentId: string): Observable<any> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    // Note the `"${consentId}"` - .NET [FromBody] strings MUST be quoted!
    return this.http.post(`${this.baseUrl}/save-vakifbank-consent`, `"${consentId}"`, { headers });
  }
}
