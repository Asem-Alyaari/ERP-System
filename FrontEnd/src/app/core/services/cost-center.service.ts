import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CostCenterLookup {
  id: string;
  costCenterCode: string;
  costCenterNameAr: string;
  costCenterNameEn: string;
  isDetail: boolean;
  parentCostCenterId?: string;
}

export interface CostCenterDto {
  id: string;
  costCenterCode: string;
  costCenterNameAr: string;
  costCenterNameEn: string;
  isDetail: boolean;
  parentCostCenterId?: string;
}

@Injectable({
  providedIn: 'root'
})
export class CostCenterService {
  private apiUrl = `${environment.apiUrl}/CostCenters`;

  constructor(private http: HttpClient) {}

  getAll(onlyDetail = true): Observable<CostCenterLookup[]> {
    return this.http.get<CostCenterLookup[]>(this.apiUrl, {
      params: { onlyDetail: onlyDetail.toString() }
    });
  }

  getDetailCostCenters(): Observable<CostCenterLookup[]> {
    return this.getAll(true);
  }

  getById(id: string): Observable<CostCenterDto> {
    return this.http.get<CostCenterDto>(`${this.apiUrl}/${id}`);
  }

  create(costCenter: Partial<CostCenterDto>): Observable<string> {
    return this.http.post<string>(this.apiUrl, costCenter);
  }

  update(id: string, costCenter: Partial<CostCenterDto>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, costCenter);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
