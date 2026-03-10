import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { BankApiService } from '../../services/bank-api';
import { FormsModule } from '@angular/forms';
import { IbanPipe } from '../../pipes/iban-pipe'; // Make sure this path is correct for your project!

@Component({
  selector: 'app-account-detail',
  standalone: true,
  imports: [CommonModule, IbanPipe, FormsModule],
  templateUrl: './account-detail.html',
  styleUrl: './account-detail.scss'
})
export class AccountDetailComponent implements OnInit {
  accountNumber: string = '';
  details: any = null;
  transactions: any[] = [];
  isLoading: boolean = true;

  startDate: string = '';
  endDate: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private bankApi: BankApiService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit() {
    // 1. Calculate the dates
    const end = new Date();
    const start = new Date();
    start.setDate(end.getDate() - 30);

    // 2. Format them as YYYY-MM-DD so the HTML Date Picker can read them!
    this.endDate = end.toISOString().split('T')[0];
    this.startDate = start.toISOString().split('T')[0];

    // 3. Grab the account number
    this.accountNumber = this.route.snapshot.paramMap.get('accountNumber') || '';

    if (this.accountNumber) {
      this.loadDetails();

      // 4. Auto-sync transactions immediately on page load
      this.syncTransactions();
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

    // Read the dates from the HTML inputs
    const startIso = new Date(this.startDate).toISOString();

    const endObj = new Date(this.endDate);
    endObj.setHours(23, 59, 59, 999);
    const endIso = endObj.toISOString();

    // Make the single, correct API call
    this.bankApi.syncTransactions(this.accountNumber, startIso, endIso).subscribe({
      next: (data) => {
        this.transactions = data.$values ? data.$values : data;
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
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Receipt_${transactionId}.pdf`;
        document.body.appendChild(a);
        a.click();

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
