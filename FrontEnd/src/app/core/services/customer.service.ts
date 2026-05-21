import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PagedResponse } from './category.service';
import { environment } from '../../../environments/environment';

export interface Customer {
  id: string;
  customerCode: string;
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
export class CustomerService {
  private apiUrl = `${environment.apiUrl}/Customers`;

  constructor(private http: HttpClient) {}

  getAll(pageNumber = 1, pageSize = 10, searchTerm = ''): Observable<PagedResponse<Customer>> {
    let params = new HttpParams()
      .set('PageNumber', pageNumber.toString())
      .set('PageSize', pageSize.toString());
    
    if (searchTerm) {
      params = params.set('SearchTerm', searchTerm);
    }
    
    return this.http.get<PagedResponse<Customer>>(this.apiUrl, { params });
  }

  getAllList(): Observable<Customer[]> {
    return this.http.get<Customer[]>(`${this.apiUrl}/all`);
  }

  getById(id: string): Observable<Customer> {
    return this.http.get<Customer>(`${this.apiUrl}/${id}`);
  }

  create(customer: Partial<Customer>): Observable<string> {
    return this.http.post<string>(this.apiUrl, customer);
  }

  update(id: string, customer: Partial<Customer>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, customer);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
