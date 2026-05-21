import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResponse } from './category.service';

export interface StockGroup {
  id: string;
  groupCode: string;
  groupNameAr: string;
  groupNameEn: string;
  parentGroupId?: string;
  isDetail: boolean;
  inventoryAccountId?: string;
  salesAccountId?: string;
  costOfGoodsSoldAccountId?: string;
}

@Injectable({
  providedIn: 'root'
})
export class StockGroupService {
  private apiUrl = `${environment.apiUrl}/StockGroups`;

  constructor(private http: HttpClient) {}

  getAll(pageNumber = 1, pageSize = 10, searchTerm = ''): Observable<PagedResponse<StockGroup>> {
    let params = new HttpParams()
      .set('PageNumber', pageNumber.toString())
      .set('PageSize', pageSize.toString());
    
    if (searchTerm) {
      params = params.set('SearchTerm', searchTerm);
    }

    return this.http.get<PagedResponse<StockGroup>>(this.apiUrl, { params });
  }

  getList(): Observable<StockGroup[]> {
    return this.http.get<StockGroup[]>(`${this.apiUrl}/list`);
  }

  getById(id: string): Observable<StockGroup> {
    return this.http.get<StockGroup>(`${this.apiUrl}/${id}`);
  }

  create(group: Partial<StockGroup>): Observable<string> {
    return this.http.post<string>(this.apiUrl, group);
  }

  update(id: string, group: Partial<StockGroup>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, group);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
