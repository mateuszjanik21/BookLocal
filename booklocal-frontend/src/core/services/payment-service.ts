import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface PaymentDto {
  paymentId: number;
  reservationId: number;
  businessId: number;
  method: string;
  amount: number;
  commissionAmount: number;
  currency: string;
  transactionDate: string;
  status: string;
  customerName?: string;
  serviceName?: string;
  reservationAmount: number;
}

export interface CreatePaymentDto {
  reservationId: number;
  method: string;
  amount: number;
}

export interface UpdatePaymentDto {
  amount: number;
  method: number;
  status: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  createPayment(payment: CreatePaymentDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/payments`, payment);
  }

  getBusinessPayments(
    businessId: number,
    page: number = 1,
    pageSize: number = 15,
    sort?: string,
    sortDir?: string,
    methodFilter?: string,
    statusFilter?: string
  ): Observable<PagedResult<PaymentDto>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    if (sort) params = params.set('sort', sort);
    if (sortDir) params = params.set('sortDir', sortDir);
    if (methodFilter) params = params.set('methodFilter', methodFilter);
    if (statusFilter) params = params.set('statusFilter', statusFilter);
    return this.http.get<PagedResult<PaymentDto>>(`${this.apiUrl}/payments/business/${businessId}`, { params });
  }

  getReservationPayments(reservationId: number): Observable<PaymentDto[]> {
    return this.http.get<PaymentDto[]>(`${this.apiUrl}/payments/reservation/${reservationId}`);
  }

  updatePayment(paymentId: number, dto: UpdatePaymentDto): Observable<any> {
    return this.http.put(`${this.apiUrl}/payments/${paymentId}`, dto);
  }

  deletePayment(paymentId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/payments/${paymentId}`);
  }
}
