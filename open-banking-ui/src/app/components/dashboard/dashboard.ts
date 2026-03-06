import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { BankApiService } from '../../services/bank-api';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth';
import { Router } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class DashboardComponent implements OnInit {
  // This array will hold the accounts from .NET
  accounts: any[] = [];
  isLoading: boolean = true;

  newConsentId: string = '';

  // Inject the service
  constructor(
    private bankApi: BankApiService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) { }

  // This runs automatically when the page loads
  ngOnInit() {
    this.loadAccounts();
  }

  loadAccounts() {
    this.isLoading = true;

    this.bankApi.getAccounts().subscribe({
      next: (data) => {
        this.accounts = data.$values ? data.$values : data;

        this.isLoading = false;

        console.log('Accounts loaded:', data);

        this.cdr.detectChanges();
      },
      error: (err) => console.error('Failed to load accounts', err)
    });
  }

  viewDetails(accountNumber: string) {
    // Navigates to exactly: localhost:4200/account/12345
    this.router.navigate(['/account', accountNumber]);
  }

  syncVakifbank() {
    this.isLoading = true;
    this.bankApi.syncVakifbankAccounts().subscribe({
      next: (response) => {
        alert('Sync Successful!');
        this.loadAccounts();
      },
      error: (err) => {
        console.error('Sync failed', err);
        // If your .NET API returns a 400 because they haven't connected yet:
        if (err.status === 400) {
          alert(err.error.message || 'Please connect your Vakifbank account first.');
        }
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });


  }
  connectVakifbank() {
    if (!this.newConsentId) {
      alert('Please enter a Consent ID');
      return;
    }

    this.authService.saveVakifbankConsent(this.newConsentId).subscribe({
      next: (res) => {
        alert('Vakifbank Connected Successfully!');
        this.newConsentId = ''; // Clear the text box
        this.syncVakifbank(); // Automatically trigger a sync now that we are connected!
      },
      error: (err) => {
        console.error('Failed to connect bank', err);
        alert('Failed to connect. Check console.');
      }
    });
  }
}
