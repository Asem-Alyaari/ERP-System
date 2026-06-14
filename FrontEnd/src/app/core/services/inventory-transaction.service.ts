import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResponse } from './category.service';

export enum TransactionType {
  StockIn = 0,
  StockOut = 1,
  Transfer = 2
}

export enum TransactionStatus {
  Draft = 0,
  Posted = 1
}

export interface TransactionLine {
  id?: string;
  itemId: string;
  itemNameAr?: string;
  itemNameEn?: string;
  quantity: number;
  unitCost: number;
  totalCost: number;
  batchNumber?: string;
  availableQty?: number;
}

export interface InventoryTransaction {
  id: string;
  documentNumber: string;
  transactionDate: string;
  transactionType: TransactionType;
  sourceWarehouseId: string;
  sourceWarehouseNameAr?: string;
  destinationWarehouseId?: string;
  destinationWarehouseNameAr?: string;
  notes?: string;
  status: TransactionStatus;
  lines: TransactionLine[];
  createdAt?: string;
  postedAt?: string;
}

export interface ItemStockInfo {
  itemId: string;
  warehouseId: string;
  availableQty: number;
  averageCost: number;
}

@Injectable({
  providedIn: 'root'
})
export class InventoryTransactionService {
  private apiUrl = `${environment.apiUrl}/InventoryTransactions`;

  constructor(private http: HttpClient) {}

  getAll(pageNumber = 1, pageSize = 10, searchTerm = ''): Observable<PagedResponse<InventoryTransaction>> {
    let params = new HttpParams()
      .set('PageNumber', pageNumber.toString())
      .set('PageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('SearchTerm', searchTerm);
    }

    return this.http.get<PagedResponse<InventoryTransaction>>(this.apiUrl, { params });
  }

  getById(id: string): Observable<InventoryTransaction> {
    return this.http.get<InventoryTransaction>(`${this.apiUrl}/${id}`);
  }

  create(transaction: Partial<InventoryTransaction>): Observable<string> {
    return this.http.post<string>(this.apiUrl, transaction);
  }

  update(id: string, transaction: Partial<InventoryTransaction>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, transaction);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  post(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/post`, {});
  }

  getItemStockInfo(itemId: string, warehouseId: string): Observable<ItemStockInfo> {
    return this.http.get<ItemStockInfo>(`${this.apiUrl}/item-stock`, {
      params: new HttpParams()
        .set('ItemId', itemId)
        .set('WarehouseId', warehouseId)
    });
  }
}
