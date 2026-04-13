import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { BankApiService } from '../../services/bank-api';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { IbanPipe } from '../../pipes/iban-pipe';
import { ToastService } from '../../services/toast';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartOptions } from 'chart.js';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [FormsModule, CommonModule, IbanPipe, BaseChartDirective],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class DashboardComponent implements OnInit {
  accounts: any[] = [];
  isLoading: boolean = true;
  newConsentId: string = '';
  showConsentInput: boolean = false;
  depositProducts: any[] = [];
  selectedProduct: any = null;
  depositAmount: number | null = null;
  depositDays: number | null = null;
  calculationResult: any = null;
  isCalculating: boolean = false;

  // Overview / Summary section properties
  summaryData: any = null;
  isSummaryLoading: boolean = false;
  startDate: string = '';
  endDate: string = '';

  public doughnutChartLabels: string[] = ['Income', 'Expenses'];
  public doughnutChartDatasets: ChartConfiguration<'doughnut'>['data']['datasets'] = [
    { data: [0, 0], backgroundColor: ['#16a34a', '#dc2626'], hoverOffset: 4 }
  ];
  public doughnutChartOptions: ChartOptions<'doughnut'> = { responsive: true, maintainAspectRatio: false };

  public lineChartData: ChartConfiguration<'line'>['data'] = {
    labels: [],
    datasets: [
      {
        data: [],
        label: 'Balance',
        fill: true,
        tension: 0.1,
        borderColor: '#2563eb',
        backgroundColor: 'rgba(37, 99, 235, 0.1)'
      }
    ]
  };
  public lineChartOptions: ChartOptions<'line'> = { responsive: true, maintainAspectRatio: false };
  constructor(
    private bankApi: BankApiService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef,
    private router: Router,
    private toastService: ToastService
  ) { }

  ngOnInit() {
    const end = new Date();
    const start = new Date();
    start.setDate(end.getDate() - 30);

    this.endDate = end.toISOString().split('T')[0];
    this.startDate = start.toISOString().split('T')[0];

    this.loadAccounts();
    this.loadDepositProducts();
    this.loadSummary();
  }

  loadSummary() {
    this.isSummaryLoading = true;
    const startIso = new Date(this.startDate).toISOString();
    const endObj = new Date(this.endDate);
    endObj.setHours(23, 59, 59, 999);
    const endIso = endObj.toISOString();

    this.bankApi.getDashboardSummary('all', startIso, endIso).subscribe({
      next: (data) => {
        this.summaryData = data;
        
        // Inject data into charts
        this.doughnutChartDatasets[0].data = [data.totalIncome, data.totalExpense];
        
        // Map chart points
        this.lineChartData.labels = data.chartData.map((c: any) => c.dateLabel);
        this.lineChartData.datasets[0].data = data.chartData.map((c: any) => c.balance);

        this.isSummaryLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load summary', err);
        this.isSummaryLoading = false;
        this.toastService.show('Failed to load financial overview', 'error');
        this.cdr.detectChanges();
      }
    });
  }

  toggleConsentInput() {
    this.showConsentInput = !this.showConsentInput;
  }

  navigate(path: string) {
    this.router.navigate([path]);
  }

  copyToClipboard(text: string) {
    if (!text) return;
    navigator.clipboard.writeText(text).then(() => {
      this.toastService.show('IBAN copied to clipboard!', 'success');
    }).catch(err => {
      console.error('Failed to copy text: ', err);
    });
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
      error: (err) => {
        console.error('Failed to load accounts', err);
        this.toastService.show('Faild to load accounts. Please check if you are logged in', 'error')
      }

    });
  }

  viewDetails(accountNumber: string) {
    // Navigates to exactly: localhost:4200/account/12345
    this.router.navigate(['/account', accountNumber]);
  }

  syncVakifbank() {
    this.isLoading = true;
    this.bankApi.syncAccounts('vakifbank').subscribe({
      next: (response) => {
        this.loadAccounts();
      },
      error: (err) => {
        console.error('Sync failed', err);
        if (err.status === 400) {
          this.toastService.show('Please connect your vakıfbank account first', 'error');
        }
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });


  }
  connectVakifbank() {
    if (!this.newConsentId) {
      this.toastService.show('Please enter a consetn Id', 'error');
      return;
    }

    this.authService.saveVakifbankConsent(this.newConsentId).subscribe({
      next: (res) => {
        this.toastService.show('Vakıfbank connected successfuly', 'success');
        this.newConsentId = ''; // Clear the text box
        this.showConsentInput = false;
        this.syncVakifbank();
      },
      error: (err) => {
        console.error('Failed to connect bank', err);
        this.toastService.show('Filed to connect', 'error');
      }
    });
  }

  loadDepositProducts() {
    this.bankApi.getDepositProducts().subscribe({
      next: (response) => {
        this.depositProducts = response.$values ? response.$values : response;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load deposit products', err);
        //this.toastService.show('Failed to load deposit products', 'error');
      }
    });
  }

  calculateDepositReturn() {
    // Check if the object itself is selected
    if (!this.selectedProduct || !this.depositAmount || !this.depositDays) {
      this.toastService.show('Please fill in all fields to calculate', 'error');
      return;
    }

    this.isCalculating = true;
    this.calculationResult = null;

    // Extract the IDs from the selected object
    const depositType = this.selectedProduct.productCode;
    const campaignId = this.selectedProduct.campaignId;

    this.bankApi.calculateDeposit(depositType, campaignId, this.depositAmount, this.depositDays).subscribe({
      next: (result) => {
        this.calculationResult = result;
        this.isCalculating = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Calculation failed', err);
        this.toastService.show('Failed to calculate deposit. Check inputs.', 'error');
        this.isCalculating = false;
        this.cdr.detectChanges();
      }
    });
  }
}
