import { Component, signal, OnInit, OnDestroy } from '@angular/core';
import { RouterOutlet, Router } from '@angular/router';
import { AuthService } from './services/auth';
import { ToastComponent } from './components/toast/toast';
import { ToastService } from './services/toast';
import { ChatComponent } from './components/chat/chat';
import { Subscription, switchMap } from 'rxjs';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ToastComponent, ChatComponent, AsyncPipe],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit{
  title = signal('open-banking-ui');

  constructor(
    public authService: AuthService,
    private router: Router,
    private toastService: ToastService
  ) { }

  ngOnInit() {
    this.authService.checkSession().pipe(
      switchMap((session) => {
        if (!session || !session.isAuthenticated) {
          return this.authService.initializeSessionFlow(0);
        }
        return this.authService.initializeSessionFlow(session.remainingSeconds);
      })
    ).subscribe();
  }

  navigate(path: string) {
    this.router.navigate([path]);
  }

  logout() {
    this.authService.logout();

    this.router.navigate(['/']);

    this.toastService.show('Logged out', 'success');
  }

  continueSession() {
    this.authService.continueSession().subscribe((success) => {
      if (success) {
        this.toastService.show('Session extended', 'success');
        return;
      }

      this.router.navigate(['/login']);
      this.toastService.show('Session expired. Please login again.', 'error');
    });
  }

  declineSession() {
    this.authService.declineSession();
    this.toastService.show('Session will end at token expiry.', 'success');
  }
}
