import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms'; // Required to read HTML textboxes
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth';
import { ToastService } from '../../services/toast';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class LoginComponent {
  credentials = { username: '', password: '' };
  errorMessage = '';

  constructor(
    private authService: AuthService,
    private router: Router,
    private toastService: ToastService
  ) { }

  onLogin() {
    this.authService.login(this.credentials).subscribe({
      next: () => {
        // Login successful! Teleport them to the dashboard.
        this.router.navigate(['/dashboard']);
        this.toastService.show('Logged in successfully', 'success');
      },
      error: (err) => {
        this.errorMessage = 'Invalid username or password.';
        console.error('Login failed:', err);
        this.toastService.show('Failed to log in', 'error');
      }
    });
  }
}
