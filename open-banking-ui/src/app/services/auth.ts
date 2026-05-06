import { Router } from '@angular/router';
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, tap, BehaviorSubject, map, catchError, of } from 'rxjs';

export interface SessionStatus {
  isAuthenticated: boolean;
  remainingSeconds: number;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  // pointing to .NET AuthController
  private baseUrl = 'https://localhost:7277/api/Auth';


  private loggedInSubject = new BehaviorSubject<boolean>(false);


  public isLoggedIn$ = this.loggedInSubject.asObservable();

  private readonly refreshUrl = 'https://localhost:7277/api/Auth/refresh';
  private readonly checkSessionUrl = 'https://localhost:7277/api/Auth/check-session';
  private readonly promptBeforeExpiryMs = 60_000; // 1 minute before expiry prompt
  private promptTimeoutId: ReturnType<typeof setTimeout> | null = null;
  private expiryTimeoutId: ReturnType<typeof setTimeout> | null = null;

  constructor(private http: HttpClient, private router: Router) { }

  register(userData: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/register`, userData);
  }

  login(credentials: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/login`, credentials).pipe(
      tap(() => {
        // JWT is now in an HttpOnly cookie, so a successful login means authenticated.
        this.loggedInSubject.next(true);
        this.startSessionTimers(15 * 60);
      })
    );
  }

  isLoggedIn(): boolean {
    return this.loggedInSubject.value;
  }

  checkSession(): Observable<SessionStatus | null> {
    return this.http.get<SessionStatus>(this.checkSessionUrl).pipe(
      tap((session) => {
        this.loggedInSubject.next(session.isAuthenticated);
      }),
      catchError(() => {
        this.clearSessionTimers();
        this.loggedInSubject.next(false);
        return of(null);
      })
    );
  }

  logout(): void {
    this.clearSessionTimers();
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
      tap(() => {
        this.loggedInSubject.next(true);
        this.startSessionTimers(15 * 60);
      }),
      catchError(() => {
        this.clearSessionTimers();
        this.loggedInSubject.next(false);
        return of(false);
      })
    );
  }

  initializeSessionFlow(remainingSeconds: number): Observable<boolean> {
    if (remainingSeconds <= 0) {
      this.clearSessionTimers();
      this.loggedInSubject.next(false);
      return of(false);
    }

    if (remainingSeconds <= 5 * 60) {
      return this.refresh();
    }

    this.startSessionTimers(remainingSeconds);
    return of(true);
  }

  private startSessionTimers(remainingSeconds: number): void {
    this.clearSessionTimers();

    const remainingMs = Math.max(remainingSeconds * 1000, 0);
    const promptDelay = Math.max(remainingMs - this.promptBeforeExpiryMs, 0);

    this.promptTimeoutId = setTimeout(() => {
      this.handleSessionExtensionPrompt();
    }, promptDelay);

    this.expiryTimeoutId = setTimeout(() => {
      this.logout();
      this.router.navigate(['/']);
    }, remainingMs);
  }

  private clearSessionTimers(): void {
    if (this.promptTimeoutId) {
      clearTimeout(this.promptTimeoutId);
      this.promptTimeoutId = null;
    }

    if (this.expiryTimeoutId) {
      clearTimeout(this.expiryTimeoutId);
      this.expiryTimeoutId = null;
    }
  }

  private handleSessionExtensionPrompt(): void {
    const shouldContinue = window.confirm(
      'Your session is about to expire in under a minute. Continue session?'
    );

    if (shouldContinue) {
      this.refresh().subscribe((success) => {
        if (!success) {
          this.router.navigate(['/']);
        }
      });
      return;
    }

  }
}
