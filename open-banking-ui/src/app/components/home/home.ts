import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [],
  templateUrl: './home.html',
  styleUrl: './home.scss'
})
export class HomeComponent implements OnInit {
  isUserLoggedIn: boolean = false;

  constructor(private authService: AuthService, private router: Router) { }

  ngOnInit() {
    // Check if they have a token the second the page loads
    this.isUserLoggedIn = this.authService.isLoggedIn();
  }

  navigate(path: string) {
    this.router.navigate([path]);
  }

  quit() {
    this.authService.logout();
    this.isUserLoggedIn = false; // Update the UI immediately
    alert('You have successfully logged out.');
  }
}
