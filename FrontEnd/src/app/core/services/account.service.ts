import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AccountLookup {
  id: string;
  accountCode: string;
  accountNameAr: string;
  accountNameEn: string;
  costCenterStatus?: 'Required' | 'Optional' | 'Disabled';
}

export enum AccountType {
  Asset = 1,
  Liability = 2,
  Equity = 3,
  Revenue = 4,
  Expense = 5
}

export interface AccountDto {
  id: string;
  accountCode: string;
  accountNameAr: string;
  accountNameEn: string;
  parentAccountId?: string;
  accountType: AccountType;
  accountTypeName: string;
  isDetail: boolean;
  currencyId: string;
  currencyCode: string;
  currencySymbol: string;
}

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private apiUrl = `${environment.apiUrl}/Accounts`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<AccountDto[]> {
    return this.http.get<AccountDto[]>(this.apiUrl);
  }

  getDetailAccounts(): Observable<AccountLookup[]> {
    return this.http.get<AccountLookup[]>(`${this.apiUrl}/detail`);
  }

  create(account: Partial<AccountDto>): Observable<string> {
    return this.http.post<string>(this.apiUrl, account);
  }

  update(id: string, account: Partial<AccountDto>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, account);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
 