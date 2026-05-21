import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResponse } from './category.service';

export interface ItemUnit {
  id?: string;
  unitId: string;
  unitNameAr?: string;
  unitNameEn?: string;
  conversionRate: number;
  isBaseUnit: boolean;
  price: number;
  parentUnitId?: string;
  multiplier?: number;
}

export interface Item {
  id: string;
  itemCode: string;
  itemNameAr: string;
  itemNameEn: string;
  stockGroupId: string;
  stockGroupNameAr?: string;
  stockGroupNameEn?: string;
  categoryId?: string;
  categoryNameAr?: string;
  categoryNameEn?: string;
  sku?: string;
  barcode?: string;
  defaultPurchasePrice: number;
  salesPrice: number;
  isActive: boolean;
  reorderLevel: number;
  minimumQuantity: number;
  maximumQuantity: number;
  safetyStockUnitId?: string;
  itemUnits?: ItemUnit[];
}

@Injectable({
  providedIn: 'root'
})
export class ItemService {
  private apiUrl = `${environment.apiUrl}/Items`;

  constructor(private http: HttpClient) {}

  getAll(pageNumber = 1, pageSize = 10, searchTerm = ''): Observable<PagedResponse<Item>> {
    let params = new HttpParams()
      .set('PageNumber', pageNumber.toString())
      .set('PageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('SearchTerm', searchTerm);
    }

    return this.http.get<PagedResponse<Item>>(this.apiUrl, { params });
  }

  getById(id: string): Observable<Item> {
    return this.http.get<Item>(`${this.apiUrl}/${id}`);
  }

  create(item: Partial<Item>): Observable<string> {
    return this.http.post<string>(this.apiUrl, item);
  }

  update(id: string, item: Partial<Item>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, item);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
