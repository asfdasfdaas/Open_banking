import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth';
import { ToastService } from '../../services/toast';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class RegisterComponent {
  userData = { username: '', email: '', password: '' };
  errorMessage = '';

  constructor(
    private authService: AuthService,
    private router: Router,
    private toastService: ToastService
  ) { }

  onRegister() {
    this.authService.register(this.userData).subscribe({
      next: () => {
        this.router.navigate(['/login']);
        this.toastService.show('Account created, please log in', 'info');
      },
      error: (err) => {
        this.errorMessage = err.error || 'Failed to create account.';
        console.error('Registration failed:', err);
        this.toastService.show('Registration failed. Check details.', 'error');
      }
    });
  }

  cancel() {
    this.router.navigate(['/']);
  }
}
