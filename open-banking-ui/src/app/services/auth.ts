import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, tap, BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  // Pointing to .NET AuthController
  private baseUrl = 'https://localhost:7277/api/Auth';

  // 1. The BehaviorSubject holds the live state
  private loggedInSubject = new BehaviorSubject<boolean>(this.hasToken());

  // 2. Expose it as a stream for components to listen to
  public isLoggedIn$ = this.loggedInSubject.asObservable();

  constructor(private http: HttpClient) { }

  private hasToken(): boolean {
    return !!sessionStorage.getItem('jwt_token');
  }

  register(userData: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/register`, userData);
  }

  login(credentials: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/login`, credentials).pipe(
      tap((response: any) => {
        if (response.token) {
          sessionStorage.setItem('jwt_token', response.token);
          // 🚀 Broadcast to the app: We are logged in!
          this.loggedInSubject.next(true);
        }
      })
    );
  }

  getToken(): string | null {
    return sessionStorage.getItem('jwt_token');
  }

  isLoggedIn(): boolean {
    return !!this.getToken(); // Keep this for any quick static checks
  }

  logout(): void {
    this.http.post(`${this.baseUrl}/logout`, {}).subscribe({
      next: () => {
        console.log("Token securely invalidated on the server.");
        this.clearSession();
      },
      error: (err) => {
        console.error("Logout failed on server, but clearing local cache anyway.", err);
        this.clearSession();
      }
    });
  }

  // 🚀 Helper to ensure the token is wiped and the app is notified
  private clearSession() {
    sessionStorage.removeItem('jwt_token');
    this.loggedInSubject.next(false); // Broadcast to the app: We are logged out!
  }

  saveVakifbankConsent(consentId: string): Observable<any> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    return this.http.post(`${this.baseUrl}/save-vakifbank-consent`, `"${consentId}"`, { headers });
  }
}
