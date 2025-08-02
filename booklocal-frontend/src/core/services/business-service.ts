import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { Business, BusinessDetail, Employee } from '../../types/business.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class BusinessService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getBusinesses(): Observable<Business[]> {
    return this.http.get<Business[]>(`${this.apiUrl}/businesses`);
  }

  getBusinessById(id: number): Observable<BusinessDetail> {
    return this.http.get<BusinessDetail>(`${this.apiUrl}/businesses/${id}`);
  }

  getEmployeesForService(businessId: number, serviceId: number): Observable<Employee[]> {
    return this.http.get<Employee[]>(`${this.apiUrl}/businesses/${businessId}/services/${serviceId}/employees`);
  }

  getMyBusiness(): Observable<BusinessDetail> {
    return this.http.get<BusinessDetail>(`${this.apiUrl}/businesses/my-business`);
  }
}