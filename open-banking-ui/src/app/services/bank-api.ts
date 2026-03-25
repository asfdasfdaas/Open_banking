import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class BankApiService {
  // Pointing exactly to secure .NET port
  private baseUrl = 'https://localhost:7277/api';

  constructor(private http: HttpClient) { }

  // Fetch the accounts saved in internal database
  getAccounts(): Observable<any> {
    return this.http.get(`${this.baseUrl}/AccountList/get-accounts-list`);
  }

  // Trigger the sync to pull fresh data from Vakifbank
  syncVakifbankAccounts(): Observable<any> {
    return this.http.post(`${this.baseUrl}/Vakifbank/vakif-accounts`, {});
  }

  getAccountDetail(accountNumber: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/Vakifbank/account-detail/${accountNumber}`);
  }

  syncTransactions(accountNumber: string, startDate: string, endDate: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/Vakifbank/account-transactions/${accountNumber}?startDate=${startDate}&endDate=${endDate}`, {});
  }

  getTransactions(accountNumber: string, startDate: string, endDate: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/AccountList/${accountNumber}/transactions?startDate=${startDate}&endDate=${endDate}`);
  }

  downloadReceipt(accountNumber: string, transactionId: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/Vakifbank/receipt/${accountNumber}/${transactionId}`, {
      responseType: 'blob'
    });
  }

  transferInternal(payload: { SenderAccountNumber: string, ReceiverAccountNumber: string, Amount: number, Description: string }): Observable<any> {
    return this.http.post(`${this.baseUrl}/AccountList/transfer`, payload);
  }
    
  calculateCurrency(sourceCurrency: string, amount: number, targetCurrency: string): Observable<any> {
    const url = `${this.baseUrl}/Vakifbank/currency-calculator?sourceCurrency=${sourceCurrency}&amount=${amount}&targetCurrency=${targetCurrency}`;

    return this.http.post(url, {});
  }

  getDepositProducts(): Observable<any> {
    return this.http.post(`${this.baseUrl}/Vakifbank/deposit-products`,"{}");
  }

  calculateDeposit(depositType: string, campaignId: string, amount: number, days: number): Observable<any> {
    const payload = {
      Amount: amount,
      CurrencyCode: "TL", // Hardcoded as you requested
      DepositType: Number(depositType), // Ensuring they are numbers if the backend expects integers
      CampaignId: Number(campaignId),
      TermDays: days
    };
    return this.http.post(`${this.baseUrl}/Vakifbank/deposit-calculator`, payload);
  }

  getCities(): Observable<any> {
    return this.http.post(`${this.baseUrl}/Vakifbank/cities`, {});
  }

  getDistricts(cityCode: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/Vakifbank/districts?cityCode=${cityCode}`, {});
  }

  getBranches(cityCode: string, districtCode: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/Vakifbank/branches?cityCode=${cityCode}&districtCode=${districtCode}`, {});
  }
}

