import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms'; // Required to read HTML textboxes
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth';

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

  constructor(private authService: AuthService, private router: Router) { }

  onLogin() {
    this.authService.login(this.credentials).subscribe({
      next: () => {
        // Login successful! Teleport them to the dashboard.
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.errorMessage = 'Invalid username or password.';
        console.error('Login failed:', err);
      }
    });
  }
}
