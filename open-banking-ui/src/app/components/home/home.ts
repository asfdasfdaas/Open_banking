import { Component, OnInit, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth';
import { CommonModule } from '@angular/common';
import { BankApiService } from '../../services/bank-api';
import { forkJoin, of, interval, Subscription } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './home.html',
  styleUrl: './home.scss'
})
export class HomeComponent implements OnInit, OnDestroy {
  isUserLoggedIn: boolean = false;

  popularRates: { code: string, rate: number, icon: string }[] = [];
  isLoadingRates: boolean = true;
  ratesError: boolean = false;
  private pollingSubscription?: Subscription;

  cities: any[] = [];
  districts: any[] = [];
  branches: any[] = [];

  selectedCityCode: string = '';
  selectedDistrictCode: string = '';

  isLoadingLocations: boolean = false;
  isLoadingBranches: boolean = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private bankApi: BankApiService
  ) { }

  ngOnInit() {
    // Check if they have a token
    this.isUserLoggedIn = this.authService.isLoggedIn();
    this.loadPopularCurrencies();

    this.loadCities();

    this.pollingSubscription = interval(60000).subscribe(() => {
      console.log("Timer triggered: Refreshing live rates...");
      this.loadPopularCurrencies();
    });
  }
  ngOnDestroy() {
    if (this.pollingSubscription) {
      this.pollingSubscription.unsubscribe();
      console.log("Home page destroyed: Polling timer successfully killed.");
    }
  }

  navigate(path: string) {
    this.router.navigate([path]);
  }

  quit() {
    this.authService.logout();
    this.isUserLoggedIn = false;
    window.location.reload();
  }
  loadPopularCurrencies() {
    console.log("Starting to fetch currencies...");
    this.isLoadingRates = true;

    const currenciesToFetch = [
      { code: 'USD', icon: 'Dolar' },
      { code: 'EUR', icon: 'Euro' },
      { code: 'GBP', icon: 'Sterlin' },
      { code: 'JPY', icon: 'Yen' },
      { code: 'XAU', icon: 'Altın' },
      { code: 'XAG', icon: 'Gümüş' }
    ];

    const requests = currenciesToFetch.map(curr =>
      this.bankApi.calculateCurrency(curr.code, 1, 'TL').pipe(
        map(res => {
          console.log(`Raw Network Response for ${curr.code}:`, res);

          return {
            code: curr.code,
            // If res is an object, use res.convertedAmount. If res is just a number, use res
            rate: res.convertedAmount !== undefined ? res.convertedAmount : res,
            icon: curr.icon
          };
        }),
        catchError(err => {
          console.error(`API Failed for ${curr.code}`, err);
          return of(null);
        })
      )
    );

    // forkJoin executes all requests simultaneously and waits for all of them to finish
    forkJoin(requests).subscribe(results => {
      console.log("All requests finished!", results);

      // Filter out any currencies that failed
      this.popularRates = results.filter(r => r !== null) as any[];

      // Sort to ensure consistent visual order
      this.popularRates.sort((a, b) => b.code.localeCompare(a.code));

      if (this.popularRates.length === 0) {
        this.ratesError = true;
      }

      this.isLoadingRates = false;
      this.cdr.detectChanges();
    });
  }

  loadCities() {
    this.isLoadingLocations = true;
    this.bankApi.getCities().subscribe({
      next: (res) => {
        this.cities = res;
        this.isLoadingLocations = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error("Failed to load cities", err);
        this.isLoadingLocations = false;
      }
    });
  }
  onCityChange() {
    // Reset the downstream data
    this.selectedDistrictCode = '';
    this.districts = [];
    this.branches = [];

    if (!this.selectedCityCode) return;

    this.isLoadingLocations = true;
    const formattedCode = String(this.selectedCityCode).padStart(2, '0');
    this.bankApi.getDistricts(formattedCode).subscribe({
      next: (res) => {
        this.districts = res;
        this.isLoadingLocations = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error("Failed to load districts", err);
        this.isLoadingLocations = false;
      }
    });
  }

  onDistrictChange() {
    this.branches = [];
    if (!this.selectedDistrictCode) return;

    this.isLoadingBranches = true;
    this.bankApi.getBranches(this.selectedCityCode, this.selectedDistrictCode).subscribe({
      next: (res) => {
        this.branches = res;
        this.isLoadingBranches = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error("Failed to load branches", err);
        this.isLoadingBranches = false;
      }
    });
  }
}

