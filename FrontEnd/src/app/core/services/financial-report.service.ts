import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface IncomeStatementLine {
  accountId: string;
  accountCode: string;
  accountNameAr: string;
  accountNameEn: string;
  balance: number;
  level: number;
  parentAccountId: string | null;
}

export interface IncomeStatement {
  periodName: string;
  fromDate: string | null;
  toDate: string | null;
  revenues: IncomeStatementLine[];
  expenses: IncomeStatementLine[];
  totalRevenues: number;
  totalExpenses: number;
  netProfitLoss: number;
}

export interface BalanceSheetLine {
  accountId: string;
  accountCode: string;
  accountNameAr: string;
  accountNameEn: string;
  balance: number;
  level: number;
  parentAccountId: string | null;
}

export interface BalanceSheet {
  periodName: string;
  asOfDate: string;
  assets: BalanceSheetLine[];
  liabilities: BalanceSheetLine[];
  equity: BalanceSheetLine[];
  totalAssets: number;
  totalLiabilities: number;
  totalEquity: number;
  netProfitLoss: number;
  isBalanced: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class FinancialReportService {
  private apiUrl = `${environment.apiUrl}/FinancialReports`;

  constructor(private http: HttpClient) {}

  getIncomeStatement(params: {
    fiscalPeriodId: string;
    fromDate?: string;
    toDate?: string;
    costCenterId?: string;
  }): Observable<IncomeStatement> {
    return this.http.get<IncomeStatement>(`${this.apiUrl}/income-statement`, { params });
  }

  getBalanceSheet(params: {
    fiscalPeriodId: string;
    asOfDate?: string;
    costCenterId?: string;
  }): Observable<BalanceSheet> {
    return this.http.get<BalanceSheet>(`${this.apiUrl}/balance-sheet`, { params });
  }
}
