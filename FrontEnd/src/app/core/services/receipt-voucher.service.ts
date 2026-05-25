import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export enum ReceiptPaymentMethod {
  Cash = 1,
  BankTransfer = 2,
  Cheque = 3
}

export enum ReceiptTargetType {
  Customer = 1,
  Vendor = 2,
  Account = 3
}

export enum ReceiptVoucherStatus {
  Draft = 1,
  Posted = 2,
  Cancelled = 3
}

export interface CreateReceiptVoucherCommand {
  voucherNumber: string;
  voucherDate: string;
  paymentMethod: ReceiptPaymentMethod;
  destinationAccountId: string;
  sourceType: ReceiptTargetType;
  amount: number;
  createdBy: string;
  notes?: string;
  vendorId?: string;
  customerId?: string;
  sourceAccountId?: string;
  costCenterId?: string;
}

export interface ReceiptVoucherListItem {
  id: string;
  voucherNumber: string;
  voucherDate: string;
  paymentMethod: ReceiptPaymentMethod;
  destinationAccountId: string;
  destinationAccountCode: string;
  destinationAccountNameAr: string;
  sourceType: ReceiptTargetType;
  vendorId?: string;
  vendorName?: string;
  customerId?: string;
  customerName?: string;
  sourceAccountId?: string;
  sourceAccountCode?: string;
  sourceAccountNameAr?: string;
  amount: number;
  notes?: string;
  status: ReceiptVoucherStatus;
  createdBy: string;
  createdAt: string;
  postedBy?: string;
  postedAt?: string;
  costCenterId?: string;
  costCenterCode?: string;
  costCenterNameAr?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ReceiptVoucherService {
  private apiUrl = `${environment.apiUrl}/Vouchers`;

  constructor(private http: HttpClient) {}

  create(command: CreateReceiptVoucherCommand): Observable<string> {
    return this.http.post<string>(`${this.apiUrl}/receipt`, command);
  }

  getAll(): Observable<ReceiptVoucherListItem[]> {
    return this.http.get<ReceiptVoucherListItem[]>(`${this.apiUrl}/receipt`);
  }

  getById(id: string): Observable<ReceiptVoucherListItem> {
    return this.http.get<ReceiptVoucherListItem>(`${this.apiUrl}/receipt/${id}`);
  }

  post(id: string, userId: string): Observable<boolean> {
    return this.http.post<boolean>(`${this.apiUrl}/receipt/${id}/post`, { userId });
  }
}
