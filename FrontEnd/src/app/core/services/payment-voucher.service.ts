import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export enum PaymentMethod {
  Cash = 1,
  BankTransfer = 2,
  Cheque = 3
}

export enum VoucherPartnerType {
  Customer = 1,
  Vendor = 2,
  Account = 3
}

export enum VoucherStatus {
  Draft = 1,
  Posted = 2,
  Cancelled = 3
}

export interface CreatePaymentVoucherCommand {
  voucherNumber: string;
  voucherDate: string;
  paymentMethod: PaymentMethod;
  sourceAccountId: string;
  destinationType: VoucherPartnerType;
  amount: number;
  createdBy: string;
  notes?: string;
  vendorId?: string;
  customerId?: string;
  destinationAccountId?: string;
  costCenterId?: string;
}

export interface PaymentVoucherListItem {
  id: string;
  voucherNumber: string;
  voucherDate: string;
  paymentMethod: PaymentMethod;
  sourceAccountId: string;
  sourceAccountCode: string;
  sourceAccountNameAr: string;
  destinationType: VoucherPartnerType;
  vendorId?: string;
  vendorName?: string;
  customerId?: string;
  customerName?: string;
  destinationAccountId?: string;
  destinationAccountCode?: string;
  destinationAccountNameAr?: string;
  amount: number;
  notes?: string;
  status: VoucherStatus;
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
export class PaymentVoucherService {
  private apiUrl = `${environment.apiUrl}/Vouchers`;

  constructor(private http: HttpClient) {}

  create(command: CreatePaymentVoucherCommand): Observable<string> {
    return this.http.post<string>(`${this.apiUrl}/payment`, command);
  }

  createReceipt(command: CreatePaymentVoucherCommand): Observable<string> {
    return this.http.post<string>(`${this.apiUrl}/receipt`, command);
  }

  getAll(): Observable<PaymentVoucherListItem[]> {
    return this.http.get<PaymentVoucherListItem[]>(`${this.apiUrl}/payment`);
  }

  getById(id: string): Observable<PaymentVoucherListItem> {
    return this.http.get<PaymentVoucherListItem>(`${this.apiUrl}/payment/${id}`);
  }

  post(id: string, userId: string): Observable<boolean> {
    return this.http.post<boolean>(`${this.apiUrl}/payment/${id}/post`, { userId });
  }
}
