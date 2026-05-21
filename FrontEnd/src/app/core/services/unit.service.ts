import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PagedResponse } from './category.service';
import { environment } from '../../../environments/environment';

export interface Unit {
  id: string;
  nameAr: string;
  nameEn: string;
  shortName?: string;
}

@Injectable({
  providedIn: 'root'
})
export class UnitService {
  private apiUrl = `${environment.apiUrl}/Units`;

  constructor(private http: HttpClient) {}

  getAll(pageNumber = 1, pageSize = 10): Observable<PagedResponse<Unit>> {
    let params = new HttpParams()
      .set('PageNumber', pageNumber.toString())
      .set('PageSize', pageSize.toString());
    return this.http.get<PagedResponse<Unit>>(this.apiUrl, { params });
  }

  getById(id: string): Observable<Unit> {
    return this.http.get<Unit>(`${this.apiUrl}/${id}`);
  }

  create(unit: Partial<Unit>): Observable<string> {
    return this.http.post<string>(this.apiUrl, unit);
  }

  update(id: string, unit: Partial<Unit>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, unit);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
