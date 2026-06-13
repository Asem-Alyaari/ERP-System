import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResponse } from './category.service';

export interface Warehouse {
  id: string;
  code: string;
  nameAr: string;
  nameEn: string;
  location?: string;
  isActive: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class WarehouseService {
  private apiUrl = `${environment.apiUrl}/Warehouses`;

  constructor(private http: HttpClient) {}

  getAll(pageNumber = 1, pageSize = 10, searchTerm = ''): Observable<PagedResponse<Warehouse>> {
    let params = new HttpParams()
      .set('PageNumber', pageNumber.toString())
      .set('PageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('SearchTerm', searchTerm);
    }

    return this.http.get<PagedResponse<Warehouse>>(this.apiUrl, { params });
  }

  getList(): Observable<Warehouse[]> {
    return this.http.get<Warehouse[]>(`${this.apiUrl}/list`);
  }

  getById(id: string): Observable<Warehouse> {
    return this.http.get<Warehouse>(`${this.apiUrl}/${id}`);
  }

  create(warehouse: Partial<Warehouse>): Observable<string> {
    return this.http.post<string>(this.apiUrl, warehouse);
  }

  update(id: string, warehouse: Partial<Warehouse>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, warehouse);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
