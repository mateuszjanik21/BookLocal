import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { ServiceCategory } from '../../types/business.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getCategories(businessId: number) {
    return this.http.get<ServiceCategory[]>(`${this.apiUrl}/businesses/${businessId}/categories`);
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
}