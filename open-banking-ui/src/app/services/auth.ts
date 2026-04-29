import { Router } from '@angular/router';
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, tap, BehaviorSubject, map, catchError, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  // pointing to .NET AuthController
  private baseUrl = 'https://localhost:7277/api/Auth';


  private loggedInSubject = new BehaviorSubject<boolean>(false);


  public isLoggedIn$ = this.loggedInSubject.asObservable();

  private readonly refreshUrl = 'https://localhost:7277/api/Auth/refresh';

  constructor(private http: HttpClient, private router: Router) { }

  register(userData: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/register`, userData);
  }

  login(credentials: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/login`, credentials).pipe(
      tap(() => {
        // JWT is now in an HttpOnly cookie, so a successful login means authenticated.
        this.loggedInSubject.next(true);
      })
    );
  }

  isLoggedIn(): boolean {
    return this.loggedInSubject.value;
  }

  checkSession(): Observable<boolean> {
    return this.http.get(`${this.baseUrl}/check-session`).pipe(
      map(() => true),
      tap(() => this.loggedInSubject.next(true)),
      // If the JWT expired, try to refresh silently before giving up
      catchError(() => this.refresh())
    );
  }

  logout(): void {
    this.loggedInSubject.next(false);
    this.http.post(`${this.baseUrl}/logout`, {}).subscribe({
      next: () => {
        console.log("Token securely invalidated on the server.");
      },
      error: (err) => {
        console.error("Logout failed on server.", err);
      }
    });
  }

  saveVakifbankConsent(consentId: string): Observable<any> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    return this.http.post(`${this.baseUrl}/save-vakifbank-consent`, `"${consentId}"`, { headers });
  }

  refresh(): Observable<boolean> {
    return this.http.post(this.refreshUrl, {}).pipe(
      map(() => true),
      tap(() => this.loggedInSubject.next(true)),
      catchError(() => {
        this.loggedInSubject.next(false);
        return of(false);
      })
    );
  }
}
