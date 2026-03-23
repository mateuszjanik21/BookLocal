import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface FavoriteServiceDto {
  serviceVariantId: number;
  serviceName: string;
  variantName: string;
  price: number;
  durationMinutes: number;
  businessId: number;
  businessName: string;
  businessCity?: string;
  businessPhotoUrl?: string;
  isActive: boolean;
  isServiceArchived: boolean;
}

export interface FavoritePagedResult {
  items: FavoriteServiceDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class FavoriteService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/favorites`;

  getFavorites(pageNumber: number = 1, pageSize: number = 12): Observable<FavoritePagedResult> {
    return this.http.get<FavoritePagedResult>(`${this.apiUrl}?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }

  addFavorite(serviceVariantId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${serviceVariantId}`, {});
  }

  removeFavorite(serviceVariantId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${serviceVariantId}`);
  }

  checkIsFavorite(serviceVariantId: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/check/${serviceVariantId}`);
  }
}
