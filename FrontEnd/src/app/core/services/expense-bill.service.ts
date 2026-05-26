import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export enum ExpenseBillPaymentMethod {
  Cash = 1,
  Bank = 2,
  Credit = 3
}

export enum ExpenseBillStatus {
  Draft = 1,
  Posted = 2,
  Cancelled = 3
}

export interface CreateExpenseBillLineCommand {
  accountId: string;
  amount: number;
  costCenterId: string;
  notes?: string;
}

export interface CreateExpenseBillCommand {
  billNumber: string;
  transactionDate: string;
  paymentMethod: ExpenseBillPaymentMethod;
  totalAmount: number;
  taxAmount: number;
  netAmount: number;
  createdBy: string;
  notes?: string;
  vendorId?: string;
  supplierName?: string;
  paymentAccountId?: string;
  lines: CreateExpenseBillLineCommand[];
}

export interface ExpenseBillLineDto {
  id: string;
  accountId: string;
  accountCode: string;
  accountNameAr: string;
  amount: number;
  costCenterId: string;
  costCenterCode: string;
  costCenterNameAr: string;
  notes?: string;
}

export interface ExpenseBillListItem {
  id: string;
  billNumber: string;
  transactionDate: string;
  paymentMethod: ExpenseBillPaymentMethod;
  vendorId?: string;
  vendorName?: string;
  supplierName?: string;
  paymentAccountId?: string;
  paymentAccountCode?: string;
  paymentAccountNameAr?: string;
  totalAmount: number;
  taxAmount: number;
  netAmount: number;
  notes?: string;
  status: ExpenseBillStatus;
  createdBy: string;
  createdAt: string;
  postedBy?: string;
  postedAt?: string;
  lines?: ExpenseBillLineDto[];
}

@Injectable({
  providedIn: 'root'
})
export class ExpenseBillService {
  private apiUrl = `${environment.apiUrl}/ExpenseBills`;

  constructor(private http: HttpClient) {}

  create(command: CreateExpenseBillCommand): Observable<string> {
    return this.http.post<string>(this.apiUrl, command);
  }

  getAll(): Observable<ExpenseBillListItem[]> {
    return this.http.get<ExpenseBillListItem[]>(this.apiUrl);
  }

  getById(id: string): Observable<ExpenseBillListItem> {
    return this.http.get<ExpenseBillListItem>(`${this.apiUrl}/${id}`);
  }

  post(id: string, userId: string): Observable<boolean> {
    return this.http.post<boolean>(`${this.apiUrl}/${id}/post`, { userId });
  }
}
