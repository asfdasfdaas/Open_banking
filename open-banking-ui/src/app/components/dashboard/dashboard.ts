import { Component, OnInit } from '@angular/core';
import { BankApiService } from '../../services/bank-api';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  // This array will hold the accounts from .NET
  accounts: any[] = [];

  // Inject the service
  constructor(private bankApi: BankApiService) { }

  // This runs automatically when the page loads
  ngOnInit() {
    this.loadAccounts();
  }

  loadAccounts() {
    this.bankApi.getAccounts().subscribe({
      next: (data) => {
        this.accounts = data;
        console.log('Accounts loaded:', data);
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
