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

@Injectable({
  providedIn: 'root'
})
export class FavoriteService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/favorites`;

  getFavorites(): Observable<FavoriteServiceDto[]> {
    return this.http.get<FavoriteServiceDto[]>(this.apiUrl);
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
