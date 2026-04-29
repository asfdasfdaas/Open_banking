import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { BankApiService, DailyLimitDto } from '../../services/bank-api';
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
  idempotencyKey: string = '';

  limitData: DailyLimitDto | null = null;
  usagePercentage: number = 0;

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
        label: 'Balance',
        fill: true,
        tension: 0, // This makes the line curved instead of jagged
        borderColor: '#2563eb',
        backgroundColor: 'rgba(37, 99, 235, 0.1)' // faint blue under the line
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
    // Calculate the dates
    const end = new Date();
    const start = new Date();
    start.setDate(end.getDate() - 30);

    // Format them as YYYY-MM-DD so the HTML Date Picker can read them
    this.endDate = end.toISOString().split('T')[0];
    this.startDate = start.toISOString().split('T')[0];

    this.accountNumber = this.route.snapshot.paramMap.get('accountNumber') || '';

    if (this.accountNumber) {
      this.fetchLimits();
      this.loadDetails();

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

    const startIso = new Date(this.startDate).toISOString();
    const endObj = new Date(this.endDate);
    endObj.setHours(23, 59, 59, 999);
    const endIso = endObj.toISOString();

    this.bankApi.getTransactions(this.accountNumber, startIso, endIso).subscribe({
      next: (data) => {
        this.transactions = data.$values ? data.$values : data;
        this.loadAccountSummary(this.accountNumber);
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

  loadAccountSummary(accountNumber: string) {// pie and chart graph

    const startIso = new Date(this.startDate).toISOString();
    const endObj = new Date(this.endDate);
    endObj.setHours(23, 59, 59, 999);
    const endIso = endObj.toISOString();

    this.bankApi.getDashboardSummary(accountNumber, startIso, endIso).subscribe({
      next: (data) => {
        this.totalIncome = data.totalIncome;
        this.totalExpense = data.totalExpense;
        this.netTotal = data.netTotal;

        this.doughnutChartDatasets = [
          {
            ...this.doughnutChartDatasets[0], 
            data: [data.totalIncome, data.totalExpense] 
          }
        ];


        this.lineChartData = {
          ...this.lineChartData, 
          labels: data.chartData.map((c: any) => c.dateLabel),
          datasets: [
            {
              ...this.lineChartData.datasets[0],
              data: data.chartData.map((c: any) => c.balance)
            }
          ]
        };

        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load account summary', err);
        this.cdr.detectChanges();
      }
    });
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
    this.idempotencyKey = crypto.randomUUID();
  }

  closeTransferModal() {
    this.isTransferModalOpen = false;
  }

  submitTransfer() {
    if (this.transferData.amount <= 0) {
      this.toastService.show('Amount must be greater than zero.', 'error');
      return;
    }
    if (!this.transferData.receiverAccountNumber) {
      this.toastService.show('Please enter a receiver account number.', 'error');
      return;
    }

    this.isTransferring = true;

    // Build the DTO the .NET backend expects
    const payload = {
      SenderAccountNumber: this.details.accountNumber,
      ReceiverAccountNumber: String(this.transferData.receiverAccountNumber),
      Amount: this.transferData.amount,
      Description: this.transferData.description,
    };

    this.bankApi.transferInternal(payload, this.idempotencyKey).subscribe({
      next: (res) => {
        this.isTransferring = false;
        this.closeTransferModal();
        this.toastService.show('Transfer completed successfully!', 'success');

        // instantly reload the account details and ledger so the user sees their new balance
        this.loadDetails();
        this.loadLedger();
      },
      error: (err) => {
        this.isTransferring = false;
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
    this.aiInsights = ''; // clear previous insights

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
  fetchLimits() {
    this.bankApi.getAccountLimits(this.accountNumber).subscribe({
      next: (data) => {
        this.limitData = data;

        this.usagePercentage = Math.min((data.used / data.limit) * 100, 100);

        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load daily limits', err);
        this.isLoading = false;
      }
    });
  }
}
