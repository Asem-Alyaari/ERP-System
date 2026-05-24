import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface LedgerTransaction {
  id: string;
  date: string;
  voucherNumber: string;
  reference: string;
  description: string;
  debit: number;
  credit: number;
  runningBalance: number;
  costCenterName?: string;
}

export interface LedgerSummary {
  openingBalance: number;
  totalDebit: number;
  totalCredit: number;
  closingBalance: number;
}

export interface LedgerReport {
  accountId: string;
  accountCode: string;
  accountNameAr: string;
  accountNameEn: string;
  costCenterId?: string;
  costCenterName?: string;
  fromDate: string;
  toDate: string;
  summary: LedgerSummary;
  transactions: LedgerTransaction[];
}

@Injectable({
  providedIn: 'root'
})
export class LedgerService {
  private apiUrl = `${environment.apiUrl}/FinancialReports`;

  constructor(private http: HttpClient) {}

  getReport(
    accountId: string,
    costCenterId?: string,
    fromDate?: string,
    toDate?: string
  ): Observable<LedgerReport> {
    let params = new HttpParams().set('accountId', accountId);

    if (costCenterId) {
      params = params.set('costCenterId', costCenterId);
    }
    if (fromDate) {
      params = params.set('fromDate', fromDate);
    }
    if (toDate) {
      params = params.set('toDate', toDate);
    }

    return this.http.get<LedgerReport>(`${this.apiUrl}/ledger`, { params });
  }

  exportReport(
    accountId: string,
    costCenterId?: string,
    fromDate?: string,
    toDate?: string
  ): Observable<Blob> {
    let params = new HttpParams().set('accountId', accountId);

    if (costCenterId) {
      params = params.set('costCenterId', costCenterId);
    }
    if (fromDate) {
      params = params.set('fromDate', fromDate);
    }
    if (toDate) {
      params = params.set('toDate', toDate);
    }

    return this.http.get(`${this.apiUrl}/ledger/export`, {
      params,
      responseType: 'blob'
    });
  }
}
