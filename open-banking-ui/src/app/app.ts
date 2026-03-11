import { Component, signal } from '@angular/core';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { AuthService } from './services/auth';
import { ToastComponent } from './components/toast/toast';
import { ToastService } from './services/toast';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ToastComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  title = signal('open-banking-ui');
  isLoggedIn = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private toastService: ToastService
  ) {
    // Listen to the router. Every time the page changes, re-check the login status!
    this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        this.isLoggedIn = this.authService.isLoggedIn();
      }
    });
  }

  // Helper method for the HTML to use
  navigate(path: string) {
    this.router.navigate([path]);
  }

  logout() {
    this.authService.logout();
    this.isLoggedIn = false;
    if (window.location.pathname == '/') {
      window.location.reload();
    }
    this.router.navigate(['/']); // Kick them back to the home page
    this.toastService.show('Logged out', 'success');
  }
}
