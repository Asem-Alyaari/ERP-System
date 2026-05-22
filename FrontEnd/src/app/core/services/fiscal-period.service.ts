import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface FiscalPeriod {
  id: string;
  yearName: string;
  startDate: string;
  endDate: string;
  isClosed: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class FiscalPeriodService {
  private apiUrl = `${environment.apiUrl}/FiscalPeriods`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<FiscalPeriod[]> {
    return this.http.get<FiscalPeriod[]>(this.apiUrl);
  }

  create(period: Partial<FiscalPeriod>): Observable<string> {
    return this.http.post<string>(this.apiUrl, period);
  }

  close(id: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/close`, {});
  }

  open(id: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/open`, {});
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
