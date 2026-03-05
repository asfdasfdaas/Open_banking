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
}
