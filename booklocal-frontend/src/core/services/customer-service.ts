import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { CustomerDetail, CustomerListItem, UpdateCustomerNotePayload, UpdateCustomerStatusPayload } from '../../types/customer.models';


@Injectable({
  providedIn: 'root'
})
export class CustomerService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getCustomers(businessId: number, search?: string): Observable<CustomerListItem[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);

    return this.http.get<CustomerListItem[]>(`${this.apiUrl}/businesses/${businessId}/customers`, { params });
  }

  getCustomerDetails(businessId: number, customerId: string): Observable<CustomerDetail> {
    return this.http.get<CustomerDetail>(`${this.apiUrl}/businesses/${businessId}/customers/${customerId}`);
  }

  updateNotes(businessId: number, customerId: string, payload: UpdateCustomerNotePayload) {
    return this.http.put(`${this.apiUrl}/businesses/${businessId}/customers/${customerId}/notes`, payload);
  }

  updateStatus(businessId: number, customerId: string, payload: UpdateCustomerStatusPayload) {
    return this.http.put(`${this.apiUrl}/businesses/${businessId}/customers/${customerId}/status`, payload);
  }
}
