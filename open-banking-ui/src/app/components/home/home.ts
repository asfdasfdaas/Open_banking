import { Component, OnInit, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth';
import { CommonModule } from '@angular/common';
import { BankApiService } from '../../services/bank-api';
import { forkJoin, of, interval, Subscription } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home.html',
  styleUrl: './home.scss'
})
export class HomeComponent implements OnInit, OnDestroy {
  isUserLoggedIn: boolean = false;

  popularRates: { code: string, rate: number, icon: string }[] = [];
  isLoadingRates: boolean = true;
  ratesError: boolean = false;
  private pollingSubscription?: Subscription;

  constructor(
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private bankApi: BankApiService
  ) { }

  ngOnInit() {
    // Check if they have a token the second the page loads
    this.isUserLoggedIn = this.authService.isLoggedIn();
    this.loadPopularCurrencies();
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

    // 1. Prepare all our API requests
    const requests = currenciesToFetch.map(curr =>
      this.bankApi.calculateCurrency(curr.code, 1, 'TL').pipe(
        map(res => {
          // 🚀 This logs EXACTLY what your .NET server is handing to Angular
          console.log(`Raw Network Response for ${curr.code}:`, res);

          return {
            code: curr.code,
            // Safety net: If res is an object, use res.convertedAmount. If res is just a raw number, use res!
            rate: res.convertedAmount !== undefined ? res.convertedAmount : res,
            icon: curr.icon
          };
        }),
        catchError(err => {
          console.error(`API Failed for ${curr.code}`, err);
          return of(null); // If one fails, gracefully return null so the others don't crash!
        })
      )
    );

    // 2. forkJoin executes all requests simultaneously and waits for ALL of them to finish
    forkJoin(requests).subscribe(results => {
      console.log("All requests finished!", results);

      // Filter out any currencies that failed (the ones that returned null)
      this.popularRates = results.filter(r => r !== null) as any[];

      // Sort to ensure consistent visual order
      this.popularRates.sort((a, b) => b.code.localeCompare(a.code));

      if (this.popularRates.length === 0) {
        this.ratesError = true;
      }

      // Hide the spinner guaranteed!
      this.isLoadingRates = false;
      this.cdr.detectChanges();
    });
  }
}
