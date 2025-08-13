import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { MainCategory, ServiceCategory, ServiceCategoryFeed } from '../../types/business.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getCategoryFeed(): Observable<ServiceCategoryFeed[]> {
    return this.http.get<ServiceCategoryFeed[]>(`${this.apiUrl}/categories/feed`);
  }

  getCategories(businessId: number, includeArchived: boolean): Observable<ServiceCategory[]> {
    return this.http.get<ServiceCategory[]>(`${this.apiUrl}/businesses/${businessId}/categories`, {
      params: { includeArchived }
    });
  }

  addCategory(businessId: number, payload: { name: string }): Observable<ServiceCategory> {
  return this.http.post<ServiceCategory>(`${this.apiUrl}/businesses/${businessId}/categories`, payload);
}

  updateCategory(businessId: number, categoryId: number, payload: { name: string }) {
    return this.http.put(`${this.apiUrl}/businesses/${businessId}/categories/${categoryId}`, payload);
  }

  deleteCategory(businessId: number, categoryId: number) {
    return this.http.delete(`${this.apiUrl}/businesses/${businessId}/categories/${categoryId}`);
  }

  getMainCategories(): Observable<MainCategory[]> {
    return this.http.get<MainCategory[]>(`${this.apiUrl}/maincategories`);
  }
}