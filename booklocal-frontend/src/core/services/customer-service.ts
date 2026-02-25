import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { CustomerDetail, CustomerListItem, UpdateCustomerNotePayload, UpdateCustomerStatusPayload, CustomerStatusFilter, CustomerHistoryFilter, CustomerSpentFilter } from '../../types/customer.models';

export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
}

@Injectable({
  providedIn: 'root'
})
export class CustomerService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getCustomers(businessId: number, search?: string, statusFilter: CustomerStatusFilter = CustomerStatusFilter.All, historyFilter: CustomerHistoryFilter = CustomerHistoryFilter.All, spentFilter: CustomerSpentFilter = CustomerSpentFilter.All, page: number = 1, pageSize: number = 20): Observable<PagedResult<CustomerListItem>> {
    let params = new HttpParams()
        .set('page', page)
        .set('pageSize', pageSize);
        
    if (search) params = params.set('search', search);
    if (statusFilter !== CustomerStatusFilter.All) params = params.set('status', statusFilter.toString());
    if (historyFilter !== CustomerHistoryFilter.All) params = params.set('history', historyFilter.toString());
    if (spentFilter !== CustomerSpentFilter.All) params = params.set('spent', spentFilter.toString());

    return this.http.get<PagedResult<CustomerListItem>>(`${this.apiUrl}/businesses/${businessId}/customers`, { params });
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
