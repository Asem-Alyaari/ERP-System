import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AccountLookup {
  id: string;
  accountCode: string;
  accountNameAr: string;
  accountNameEn: string;
}

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private apiUrl = `${environment.apiUrl}/Accounts`;

  constructor(private http: HttpClient) {}

  getDetailAccounts(): Observable<AccountLookup[]> {
    return this.http.get<AccountLookup[]>(`${this.apiUrl}/detail`);
  }
}
