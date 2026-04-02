import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class BankApiService {
  // Pointing to .NET port
  private baseUrl = 'https://localhost:7277/api';
  private defaultProvider = 'vakifbank';

  constructor(private http: HttpClient) { }

  private normalizeProvider(provider?: string): string {
    const p = (provider ?? this.defaultProvider).trim();
    return p.length ? p : this.defaultProvider;
  }

  private banks(provider?: string): string {
    return `${this.baseUrl}/banks/${encodeURIComponent(this.normalizeProvider(provider))}`;
  }

  // Fetch the accounts saved in internal database
  getAccounts(): Observable<any> {
    return this.http.get(`${this.baseUrl}/AccountList/get-accounts-list`);
  }

  // Trigger the sync to pull fresh data from the selected bank provider
  syncAccounts(provider?: string): Observable<any> {
    return this.http.post(`${this.banks(provider)}/accounts/sync`, {});
  }

  // Loads account detail. Backend resolves internal vs external via stored ProviderName.
  getAccountDetail(accountNumber: string, provider?: string): Observable<any> {
    return this.http.get(`${this.banks(provider)}/accounts/${encodeURIComponent(accountNumber)}`);
  }

  syncTransactions(accountNumber: string, startDate: string, endDate: string, provider?: string): Observable<any> {
    return this.http.post(
      `${this.banks(provider)}/accounts/${encodeURIComponent(accountNumber)}/transactions/sync?startDate=${startDate}&endDate=${endDate}`,
      {}
    );
  }

  getTransactions(accountNumber: string, startDate: string, endDate: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/AccountList/${accountNumber}/transactions?startDate=${startDate}&endDate=${endDate}`);
  }

  downloadReceipt(accountNumber: string, transactionId: string, provider?: string): Observable<Blob> {
    return this.http.get(`${this.banks(provider)}/accounts/${encodeURIComponent(accountNumber)}/receipt/${encodeURIComponent(transactionId)}`, {
      responseType: 'blob'
    });
  }

  transferInternal(payload: { SenderAccountNumber: string, ReceiverAccountNumber: string, Amount: number, Description: string }): Observable<any> {
    return this.http.post(`${this.baseUrl}/AccountList/transfer`, payload);
  }
    
  calculateCurrency(sourceCurrency: string, amount: number, targetCurrency: string): Observable<any> {
    const url = `${this.banks()}/currency-calculator?sourceCurrency=${sourceCurrency}&amount=${amount}&targetCurrency=${targetCurrency}`;

    return this.http.post(url, {});
  }

  getDepositProducts(): Observable<any> {
    return this.http.post(`${this.banks()}/deposit-products`, "{}");
  }

  calculateDeposit(depositType: string, campaignId: string, amount: number, days: number): Observable<any> {
    const payload = {
      Amount: amount,
      CurrencyCode: "TL", // Hardcoded as you requested
      DepositType: Number(depositType), // Ensuring they are numbers if the backend expects integers
      CampaignId: Number(campaignId),
      TermDays: days
    };
    return this.http.post(`${this.banks()}/deposit-calculator`, payload);
  }

  getCities(): Observable<any> {
    return this.http.post(`${this.banks()}/cities`, {});
  }

  getDistricts(cityCode: string): Observable<any> {
    return this.http.post(`${this.banks()}/districts?cityCode=${cityCode}`, {});
  }

  getBranches(cityCode: string, districtCode: string): Observable<any> {
    return this.http.post(`${this.banks()}/branches?cityCode=${cityCode}&districtCode=${districtCode}`, {});
  }

  sendChatMessage(prompt: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/Ai/chat`, { Prompt: prompt });
  }

  getAccountInsights(accountNumber: string, startDate: string, endDate: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/Ai/analyze-spending/${accountNumber}?startDate=${startDate}&endDate=${endDate}`, {});
  }
}
