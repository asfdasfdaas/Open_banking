import { Component, signal, OnInit, OnDestroy } from '@angular/core';
import { RouterOutlet, Router } from '@angular/router';
import { AuthService } from './services/auth';
import { ToastComponent } from './components/toast/toast';
import { ToastService } from './services/toast';
import { ChatComponent } from './components/chat/chat';
import { Subscription } from 'rxjs'; // 🚀 Import Subscription

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ToastComponent, ChatComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit, OnDestroy {
  title = signal('open-banking-ui');
  isLoggedIn = false;
  private authSub!: Subscription;

  constructor(
    private authService: AuthService,
    private router: Router,
    private toastService: ToastService
  ) { }

  ngOnInit() {
    this.authService.checkSession().subscribe();

    this.authSub = this.authService.isLoggedIn$.subscribe(status => {
      this.isLoggedIn = status;
    });
  }

  ngOnDestroy() {
    // Prevent memory leaks if this component is ever destroyed
    if (this.authSub) {
      this.authSub.unsubscribe();
    }
  }

  navigate(path: string) {
    this.router.navigate([path]);
  }

  logout() {
    this.authService.logout();

    this.router.navigate(['/']);

    this.toastService.show('Logged out', 'success');
  }
}
