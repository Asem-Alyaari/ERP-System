import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResponse } from './category.service';

export interface CurrencyExchangeRate {
  id: string;
  currencyId: string;
  currencyCode: string;
  currencyName: string;
  rate: number;
  effectiveDate: string;
}

@Injectable({
  providedIn: 'root'
})
export class CurrencyExchangeRateService {
  private apiUrl = `${environment.apiUrl}/CurrencyExchangeRates`;

  constructor(private http: HttpClient) {}

  getAll(pageNumber = 1, pageSize = 10, currencyId?: string, searchTerm = ''): Observable<PagedResponse<CurrencyExchangeRate>> {
    let params = new HttpParams()
      .set('PageNumber', pageNumber.toString())
      .set('PageSize', pageSize.toString());

    if (currencyId) {
      params = params.set('CurrencyId', currencyId);
    }
    if (searchTerm) {
      params = params.set('SearchTerm', searchTerm);
    }

    return this.http.get<PagedResponse<CurrencyExchangeRate>>(this.apiUrl, { params });
  }

  getList(currencyId?: string): Observable<CurrencyExchangeRate[]> {
    let params = new HttpParams();
    if (currencyId) {
      params = params.set('currencyId', currencyId);
    }
    return this.http.get<CurrencyExchangeRate[]>(`${this.apiUrl}/list`, { params });
  }

  getById(id: string): Observable<CurrencyExchangeRate> {
    return this.http.get<CurrencyExchangeRate>(`${this.apiUrl}/${id}`);
  }

  create(rate: Partial<CurrencyExchangeRate>): Observable<string> {
    return this.http.post<string>(this.apiUrl, rate);
  }

  update(id: string, rate: Partial<CurrencyExchangeRate>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, rate);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
