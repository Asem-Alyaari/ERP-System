import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResponse } from './category.service';

export enum JournalEntryStatus {
  Draft = 1,
  Posted = 2,
  Cancelled = 3
}

export interface JournalEntryLineDto {
  accountId: string;
  debit: number;
  credit: number;
  currencyId: string;
  exchangeRate: number;
  foreignDebit?: number;
  foreignCredit?: number;
  costCenterId?: string;
  memo?: string;
}

export interface CreateJournalEntryCommand {
  voucherNumber: string;
  transactionDate: string;
  description?: string;
  fiscalPeriodId: string;
  createdBy: string;
  lines: JournalEntryLineDto[];
}

export interface JournalEntryListItem {
  id: string;
  voucherNumber: string;
  transactionDate: string;
  description?: string;
  fiscalPeriodId: string;
  fiscalPeriodName: string;
  status: JournalEntryStatus;
  createdBy: string;
  createdAt: string;
  postedBy?: string;
  postedAt?: string;
  totalDebit: number;
  totalCredit: number;
}

export interface JournalEntryLineDetail {
  id: string;
  accountId: string;
  accountCode: string;
  accountNameAr: string;
  accountNameEn: string;
  debit: number;
  credit: number;
  foreignDebit?: number;
  foreignCredit?: number;
  currencyId: string;
  currencyCode: string;
  currencyName: string;
  exchangeRate: number;
  costCenterId?: string;
  costCenterCode?: string;
  costCenterNameAr?: string;
  costCenterNameEn?: string;
  memo?: string;
}

export interface JournalEntryDetail {
  id: string;
  voucherNumber: string;
  transactionDate: string;
  description?: string;
  fiscalPeriodId: string;
  fiscalPeriodName: string;
  status: JournalEntryStatus;
  createdBy: string;
  createdAt: string;
  postedBy?: string;
  postedAt?: string;
  lines: JournalEntryLineDetail[];
}

@Injectable({
  providedIn: 'root'
})
export class JournalEntryService {
  private apiUrl = `${environment.apiUrl}/JournalEntries`;

  constructor(private http: HttpClient) {}

  getAll(
    pageNumber = 1,
    pageSize = 10,
    searchTerm = '',
    status?: JournalEntryStatus | null,
    fiscalPeriodId?: string | null,
    startDate?: string | null,
    endDate?: string | null
  ): Observable<PagedResponse<JournalEntryListItem>> {
    let params = new HttpParams()
      .set('PageNumber', pageNumber.toString())
      .set('PageSize', pageSize.toString());

    if (searchTerm) params = params.set('SearchTerm', searchTerm);
    if (status !== undefined && status !== null) params = params.set('Status', status.toString());
    if (fiscalPeriodId) params = params.set('FiscalPeriodId', fiscalPeriodId);
    if (startDate) params = params.set('StartDate', startDate);
    if (endDate) params = params.set('EndDate', endDate);

    return this.http.get<PagedResponse<JournalEntryListItem>>(this.apiUrl, { params });
  }

  getById(id: string): Observable<JournalEntryDetail> {
    return this.http.get<JournalEntryDetail>(`${this.apiUrl}/${id}`);
  }

  create(command: CreateJournalEntryCommand): Observable<string> {
    return this.http.post<string>(this.apiUrl, command);
  }

  post(id: string, postedBy: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/post`, JSON.stringify(postedBy), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  unpost(id: string, unpostedBy: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/unpost`, JSON.stringify(unpostedBy), {
      headers: { 'Content-Type': 'application/json' }
    });
  }
}
