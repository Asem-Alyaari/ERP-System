import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResponse } from './category.service';

export interface Currency {
  id: string;
  code: string;
  name: string;
  symbol: string;
  isLocal: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class CurrencyService {
  private apiUrl = `${environment.apiUrl}/Currencies`;

  constructor(private http: HttpClient) {}

  getAll(pageNumber = 1, pageSize = 10, searchTerm = ''): Observable<PagedResponse<Currency>> {
    let params = new HttpParams()
      .set('PageNumber', pageNumber.toString())
      .set('PageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('SearchTerm', searchTerm);
    }

    return this.http.get<PagedResponse<Currency>>(this.apiUrl, { params });
  }

  getList(): Observable<Currency[]> {
    return this.http.get<Currency[]>(`${this.apiUrl}/list`);
  }

  getById(id: string): Observable<Currency> {
    return this.http.get<Currency>(`${this.apiUrl}/${id}`);
  }

  create(currency: Partial<Currency>): Observable<string> {
    return this.http.post<string>(this.apiUrl, currency);
  }

  update(id: string, currency: Partial<Currency>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, currency);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
