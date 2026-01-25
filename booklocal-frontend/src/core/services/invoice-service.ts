import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface InvoiceDto {
  invoiceId: number;
  invoiceNumber: string;
  issueDate: string;
  saleDate: string;
  customerName: string;
  customerNip?: string;
  totalGross: number;
  paymentMethod: number;
  items: InvoiceItemDto[];
}

export interface InvoiceItemDto {
  name: string;
  quantity: number;
  unitPriceNet: number;
  vatRate: number;
  netValue: number;
  grossValue: number;
}

@Injectable({
  providedIn: 'root'
})
export class InvoiceService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  generateInvoice(businessId: number, reservationId: number): Observable<InvoiceDto> {
    return this.http.post<InvoiceDto>(`${this.apiUrl}/businesses/${businessId}/invoices/generate`, { reservationId });
  }

  getInvoices(businessId: number, page: number = 1, pageSize: number = 10): Observable<PagedResult<InvoiceDto>> {
    return this.http.get<PagedResult<InvoiceDto>>(`${this.apiUrl}/businesses/${businessId}/invoices?page=${page}&pageSize=${pageSize}`);
  }
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
