import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { ServicePayload } from '../../types/business.model';

@Injectable({
  providedIn: 'root'
})
export class ServiceService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  addService(businessId: number, payload: ServicePayload & { serviceCategoryId: number }) {
    return this.http.post(`${this.apiUrl}/businesses/${businessId}/services`, payload);
  }

  updateService(businessId: number, serviceId: number, payload: ServicePayload) {
    return this.http.put(`${this.apiUrl}/businesses/${businessId}/services/${serviceId}`, payload);
  }

  deleteService(businessId: number, serviceId: number) {
    return this.http.delete(`${this.apiUrl}/businesses/${businessId}/services/${serviceId}`);
  }

  deleteServiceVariant(businessId: number, serviceId: number, variantId: number) {
    return this.http.delete(`${this.apiUrl}/businesses/${businessId}/services/${serviceId}/variants/${variantId}`);
  }

  restoreService(businessId: number, serviceId: number) {
    return this.http.patch(`${this.apiUrl}/businesses/${businessId}/services/${serviceId}/restore`, {});
  }

  restoreServiceVariant(businessId: number, serviceId: number, variantId: number) {
    return this.http.patch(`${this.apiUrl}/businesses/${businessId}/services/${serviceId}/variants/${variantId}/restore`, {});
  }

  restoreCategory(businessId: number, categoryId: number) {
    return this.http.patch(`${this.apiUrl}/businesses/${businessId}/categories/${categoryId}/restore`, {});
  }

  deleteCategory(businessId: number, categoryId: number) {
    return this.http.delete(`${this.apiUrl}/businesses/${businessId}/categories/${categoryId}`);
  }
}