import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router'; // To read the URL and navigate back
import { CommonModule } from '@angular/common'; // Required to format dates in HTML
import { BankApiService } from '../../services/bank-api';

@Component({
  selector: 'app-account-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './account-detail.html',
  styleUrl: './account-detail.scss'
})
export class AccountDetailComponent implements OnInit {
  accountNumber: string = '';
  details: any = null;
  transactions: any[] = [];
  isLoading: boolean = true;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private bankApi: BankApiService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit() {
    // 1. Grab the "12345" out of "localhost:4200/account/12345"
    this.accountNumber = this.route.snapshot.paramMap.get('accountNumber') || '';

    if (this.accountNumber) {
      this.loadDetails();
    }
  }

  loadDetails() {
    this.isLoading = true;
    this.bankApi.getAccountDetail(this.accountNumber).subscribe({
      next: (data) => {
        this.details = data;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load details', err);
        this.isLoading = false;
      }
    });
  }

  syncTransactions() {
    this.isLoading = true;

    // Automatically generate dates for the last 300 days
    const endDate = new Date();
    const startDate = new Date();
    startDate.setDate(endDate.getDate() - 300);

    // Convert them to the ISO format that .NET expects
    const startStr = startDate.toISOString();
    const endStr = endDate.toISOString();

    this.bankApi.syncTransactions(this.accountNumber, startStr, endStr).subscribe({
      next: (data) => {
        this.transactions = data.$values ? data.$values : data; // Handle .NET JSON wrapping safely
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to sync transactions', err);
        this.isLoading = false;
      }
    });
  }

  downloadReceipt(transactionId: string) {
    this.bankApi.downloadReceipt(this.accountNumber, transactionId).subscribe({
      next: (blob: Blob) => {
        // We take the raw bytes, create a hidden browser link, and simulate a click to download it.
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Receipt_${transactionId}.pdf`; // Name the file
        document.body.appendChild(a);
        a.click();

        // Clean up memory
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
      },
      error: (err) => {
        console.error('Download failed', err);
        alert('Failed to download receipt.');
      }
    });
  }

  goBack() {
    this.router.navigate(['/dashboard']);
  }
}
