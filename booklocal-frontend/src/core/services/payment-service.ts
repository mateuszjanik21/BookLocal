import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface PaymentDto {
  paymentId: number;
  reservationId: number;
  businessId: number;
  method: string;
  amount: number;
  currency: string;
  transactionDate: string;
  status: string;
}

export interface CreatePaymentDto {
  reservationId: number;
  method: string;
  amount: number;
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

  getBusinessPayments(businessId: number, page: number = 1, pageSize: number = 10): Observable<PagedResult<PaymentDto>> {
    return this.http.get<PagedResult<PaymentDto>>(`${this.apiUrl}/payments/business/${businessId}?page=${page}&pageSize=${pageSize}`);
  }

  getReservationPayments(reservationId: number): Observable<PaymentDto[]> {
    return this.http.get<PaymentDto[]>(`${this.apiUrl}/payments/reservation/${reservationId}`);
  }
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
