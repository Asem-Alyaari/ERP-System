import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PagedResponse } from './category.service';
import { environment } from '../../../environments/environment';

export interface Vendor {
  id: string;
  vendorCode: string;
  nameAr: string;
  nameEn: string;
  taxNumber?: string;
  phone?: string;
  email?: string;
  accountId: string;
  accountNameAr?: string;
  accountNameEn?: string;
}

@Injectable({
  providedIn: 'root'
})
export class VendorService {
  private apiUrl = `${environment.apiUrl}/Vendors`;

  constructor(private http: HttpClient) {}

  getAll(pageNumber = 1, pageSize = 10, searchTerm = ''): Observable<PagedResponse<Vendor>> {
    let params = new HttpParams()
      .set('PageNumber', pageNumber.toString())
      .set('PageSize', pageSize.toString());
    
    if (searchTerm) {
      params = params.set('SearchTerm', searchTerm);
    }
    
    return this.http.get<PagedResponse<Vendor>>(this.apiUrl, { params });
  }

  getAllList(): Observable<Vendor[]> {
    return this.http.get<Vendor[]>(`${this.apiUrl}/all`);
  }

  getById(id: string): Observable<Vendor> {
    return this.http.get<Vendor>(`${this.apiUrl}/${id}`);
  }

  create(vendor: Partial<Vendor>): Observable<string> {
    return this.http.post<string>(this.apiUrl, vendor);
  }

  update(id: string, vendor: Partial<Vendor>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, vendor);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
