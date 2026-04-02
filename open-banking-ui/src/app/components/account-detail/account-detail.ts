import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { BankApiService } from '../../services/bank-api';
import { FormsModule } from '@angular/forms';
import { IbanPipe } from '../../pipes/iban-pipe';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartOptions } from 'chart.js';
import { ToastService } from '../../services/toast';

@Component({
  selector: 'app-account-detail',
  standalone: true,
  imports: [CommonModule, IbanPipe, FormsModule, BaseChartDirective],
  templateUrl: './account-detail.html',
  styleUrl: './account-detail.scss'
})
export class AccountDetailComponent implements OnInit {
  accountNumber: string = '';
  details: any = null;
  providerName: string = 'vakifbank';
  transactions: any[] = [];
  isLoading: boolean = true;
  startDate: string = '';
  endDate: string = '';
  totalIncome: number = 0;
  totalExpense: number = 0;
  netTotal: number = 0;
  isTransferModalOpen: boolean = false;
  isTransferring: boolean = false;
  aiInsights: string = '';
  isAnalyzing: boolean = false;
  transferData = {
    receiverAccountNumber: '',
    amount: 0,
    description: ''
  };

  public doughnutChartLabels: string[] = ['Income', 'Expenses'];
  public doughnutChartDatasets: ChartConfiguration<'doughnut'>['data']['datasets'] = [
    
    { data: [0, 0], backgroundColor: ['#16a34a', '#dc2626'], hoverOffset: 4 }
  ];
  public doughnutChartOptions: ChartOptions<'doughnut'> = { responsive: true, maintainAspectRatio: false };

  public lineChartData: ChartConfiguration<'line'>['data'] = {
    labels: [], // This will hold the dates
    datasets: [
      {
        data: [], // This will hold the balances
        label: 'Daily Balance',
        fill: true,
        tension: 0.1, // This makes the line curved instead of jagged
        borderColor: '#2563eb', // Blue-600
        backgroundColor: 'rgba(37, 99, 235, 0.1)' // Very faint blue underneath the line
      }
    ]
  };
  public lineChartOptions: ChartOptions<'line'> = { responsive: true, maintainAspectRatio: false };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private bankApi: BankApiService,
    private cdr: ChangeDetectorRef,
    private toastService: ToastService
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
      this.loadLedger();
    }
  }

  copyToClipboard(text: string) {
    if (!text) return;


    navigator.clipboard.writeText(text).then(() => {
      this.toastService.show('IBAN copied to clipboard!', 'success');
    }).catch(err => {
      console.error('Failed to copy text: ', err);
    });
  }

  loadDetails() {
    this.isLoading = true;
    this.bankApi.getAccountDetail(this.accountNumber, this.providerName).subscribe({
      next: (data) => {
        this.details = data;
        this.providerName = (data?.providerName || data?.ProviderName || this.providerName);
        this.toastService.show('Account Details loaded successfuly', 'info');
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load details', err);
        this.isLoading = false;
      }
    });
  }

  loadLedger() {
    this.isLoading = true;

    // Read the dates from the HTML inputs
    const startIso = new Date(this.startDate).toISOString();
    const endObj = new Date(this.endDate);
    endObj.setHours(23, 59, 59, 999);
    const endIso = endObj.toISOString();

    this.bankApi.getTransactions(this.accountNumber, startIso, endIso).subscribe({
      next: (data) => {
        this.transactions = data.$values ? data.$values : data;
        this.calculateTotals();
        this.isLoading = false;
        this.cdr.detectChanges();

      },
      error: (err) => {
        console.error('Failed to load transactions', err);
        this.isLoading = false;
        this.cdr.detectChanges();
        this.toastService.show('Failed to load transactions. Please try again later.', 'error');
      }
    });
  }

  calculateTotals() {
    this.totalIncome = 0;
    this.totalExpense = 0;


    this.transactions.sort((a, b) => new Date(b.transactionDate).getTime() - new Date(a.transactionDate).getTime());


    const chartTx = [...this.transactions].reverse();

    const balanceData: number[] = [];
    const dateLabels: string[] = [];


    for (const tx of chartTx) {
      if (tx.amount > 0) {
        this.totalIncome += tx.amount;
      } else if (tx.amount < 0) {
        this.totalExpense += Math.abs(tx.amount);
      }

      // Add data points for the line chart
      balanceData.push(tx.balance);

      // Format the date label to just show "MM/DD"
      const dateObj = new Date(tx.transactionDate);
      dateLabels.push(`${dateObj.getMonth() + 1}/${dateObj.getDate()}`);
    }

    this.netTotal = this.totalIncome - this.totalExpense;

    // Inject the calculated data into the chart objects
    this.doughnutChartDatasets[0].data = [this.totalIncome, this.totalExpense];
    this.lineChartData.labels = dateLabels;
    this.lineChartData.datasets[0].data = balanceData;
  }

  downloadReceipt(transactionId: string) {
    this.bankApi.downloadReceipt(this.accountNumber, transactionId, this.providerName).subscribe({
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

  openTransferModal() {
    this.isTransferModalOpen = true;
    // Reset the form every time they open it
    this.transferData = { receiverAccountNumber: '', amount: 0, description: '' };
  }

  closeTransferModal() {
    this.isTransferModalOpen = false;
  }

  submitTransfer() {
    // Basic frontend validation
    if (this.transferData.amount <= 0) {
      this.toastService.show('Amount must be greater than zero.', 'error');
      return;
    }
    if (!this.transferData.receiverAccountNumber) {
      this.toastService.show('Please enter a receiver account number.', 'error');
      return;
    }

    this.isTransferring = true;

    // Build the exact DTO the .NET backend expects
    const payload = {
      SenderAccountNumber: this.details.accountNumber,
      ReceiverAccountNumber: this.transferData.receiverAccountNumber,
      Amount: this.transferData.amount,
      Description: this.transferData.description
    };

    this.bankApi.transferInternal(payload).subscribe({
      next: (res) => {
        this.isTransferring = false;
        this.closeTransferModal();
        this.toastService.show('Transfer completed successfully!', 'success');

        // Instantly reload the account details and ledger so the user sees their new balance!
        this.loadDetails();
        this.loadLedger();
      },
      error: (err) => {
        this.isTransferring = false;
        // Display the specific error message generated by our C# Repository!
        this.toastService.show(err.error?.message || 'Transfer failed.', 'error');
      }
    });
  }

  goBack() {
    this.router.navigate(['/dashboard']);
  }

  analyzeSpending() {
    if (!this.accountNumber || !this.startDate || !this.endDate) return;

    this.isAnalyzing = true;
    this.aiInsights = ''; // Clear previous insights

    this.bankApi.getAccountInsights(this.accountNumber, this.startDate, this.endDate).subscribe({
      next: (res) => {
        this.aiInsights = res.advice;
        this.isAnalyzing = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error("AI Analysis failed", err);
        this.aiInsights = "Unable to generate insights at this time. Please try again later.";
        this.isAnalyzing = false;
        this.cdr.detectChanges();
      }
    });
  }
}
