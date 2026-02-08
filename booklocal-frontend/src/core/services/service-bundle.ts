import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { ServiceBundle, CreateServiceBundlePayload } from '../../types/service-bundle.model';

@Injectable({
  providedIn: 'root'
})
export class ServiceBundleService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/businesses`;

  getBundles(businessId: number): Observable<ServiceBundle[]> {
    return this.http.get<ServiceBundle[]>(`${this.apiUrl}/${businessId}/bundles`);
  }

  getBundle(businessId: number, bundleId: number): Observable<ServiceBundle> {
    return this.http.get<ServiceBundle>(`${this.apiUrl}/${businessId}/bundles/${bundleId}`);
  }

  createBundle(businessId: number, payload: CreateServiceBundlePayload): Observable<ServiceBundle> {
    return this.http.post<ServiceBundle>(`${this.apiUrl}/${businessId}/bundles`, payload);
  }

  deleteBundle(businessId: number, bundleId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${businessId}/bundles/${bundleId}`);
  }

  updateBundle(businessId: number, bundleId: number, payload: CreateServiceBundlePayload): Observable<ServiceBundle> {
    return this.http.put<ServiceBundle>(`${this.apiUrl}/${businessId}/bundles/${bundleId}`, payload);
  }
}
