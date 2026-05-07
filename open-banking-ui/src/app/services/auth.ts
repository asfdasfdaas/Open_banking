import { Router } from '@angular/router';
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, tap, BehaviorSubject, map, catchError, of, switchMap } from 'rxjs';

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
  private countdownIntervalId: ReturnType<typeof setInterval> | null = null;
  private sessionPromptSubject = new BehaviorSubject<boolean>(false);
  private sessionCountdownSubject = new BehaviorSubject<number>(0);

  public sessionPrompt$ = this.sessionPromptSubject.asObservable();
  public sessionCountdown$ = this.sessionCountdownSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) { }

  register(userData: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/register`, userData);
  }

  login(credentials: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/login`, credentials).pipe(
      switchMap((response) => {
        // JWT is now in an HttpOnly cookie, so a successful login means authenticated.
        this.loggedInSubject.next(true);
        return this.checkSession().pipe(
          tap((session) => {
            if (session?.isAuthenticated) {
              this.startSessionTimers(session.remainingSeconds);
            }
          }),
          map(() => response)
        );
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
      switchMap(() => {
        this.loggedInSubject.next(true);
        return this.checkSession().pipe(
          tap((session) => {
            if (session?.isAuthenticated) {
              this.startSessionTimers(session.remainingSeconds);
            }
          }),
          map((session) => !!session?.isAuthenticated)
        );
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

  continueSession(): Observable<boolean> {
    this.closeSessionPrompt();
    return this.refresh();
  }

  declineSession(): void {
    this.closeSessionPrompt();
  }

  private startSessionTimers(remainingSeconds: number): void {
    this.clearSessionTimers();

    const remainingMs = Math.max(remainingSeconds * 1000, 0);
    const promptDelay = Math.max(remainingMs - this.promptBeforeExpiryMs, 0);

    this.promptTimeoutId = setTimeout(() => {
      this.openSessionPrompt(Math.ceil(this.promptBeforeExpiryMs / 1000));
    }, promptDelay);

    this.expiryTimeoutId = setTimeout(() => {
      this.closeSessionPrompt();
      this.logout();
      this.router.navigate(['/login']);
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

    this.closeSessionPrompt();
  }

  private openSessionPrompt(seconds: number): void {
    this.closeSessionPrompt();
    this.sessionPromptSubject.next(true);
    this.sessionCountdownSubject.next(seconds);

    this.countdownIntervalId = setInterval(() => {
      const current = this.sessionCountdownSubject.value;
      if (current <= 0) {
        if (this.countdownIntervalId) {
          clearInterval(this.countdownIntervalId);
        }
        this.sessionCountdownSubject.next(0);
        return;
      }

      this.sessionCountdownSubject.next(current - 1);
    }, 1000);
  }

  private closeSessionPrompt(): void {
    if (this.countdownIntervalId) {
      clearInterval(this.countdownIntervalId);
      this.countdownIntervalId = null;
    }

    this.sessionPromptSubject.next(false);
    this.sessionCountdownSubject.next(0);
  }
}
