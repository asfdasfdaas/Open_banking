import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { BankApiService } from '../../services/bank-api';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class DashboardComponent implements OnInit {
  // This array will hold the accounts from .NET
  accounts: any[] = [];
  isLoading: boolean = true;

  // Inject the service
  constructor(private bankApi: BankApiService, private cdr: ChangeDetectorRef) { }

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

  syncVakifbank() {
    this.bankApi.syncVakifbankAccounts().subscribe({
      next: (response) => {
        alert('Sync Successful!');
        this.loadAccounts(); // Refresh the list after syncing
      },
      error: (err) => console.error('Sync failed', err)
    });
  }
}
