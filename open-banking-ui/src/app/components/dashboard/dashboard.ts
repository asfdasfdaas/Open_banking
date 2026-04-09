import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { BankApiService } from '../../services/bank-api';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { IbanPipe } from '../../pipes/iban-pipe';
import { ToastService } from '../../services/toast';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [FormsModule, CommonModule, IbanPipe],
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


  constructor(
    private bankApi: BankApiService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef,
    private router: Router,
    private toastService: ToastService
  ) { }

  // This runs automatically when the page loads
  ngOnInit() {
    this.loadAccounts();
    this.loadDepositProducts();
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
        this.toastService.show('Failed to load deposit products', 'error');
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
