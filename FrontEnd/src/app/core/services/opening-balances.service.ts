import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface OpeningBalance {
  accountId: string;
  debit: number;
  credit: number;
}

export interface SetOpeningBalancesCommand {
  fiscalPeriodId: string;
  balances: OpeningBalance[];
  createdBy: string;
}

@Injectable({
  providedIn: 'root'
})
export class OpeningBalancesService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  setOpeningBalances(fiscalPeriodId: string, command: SetOpeningBalancesCommand): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/FiscalPeriods/${fiscalPeriodId}/opening-balances`, command);
  }
}
