import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface InvoiceDto {
  invoiceId: number;
  invoiceNumber: string;
  issueDate: string;
  saleDate: string;
  customerName: string;
  customerNip?: string;
  totalNet: number;
  totalTax: number;
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
export class InvoiceService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  generateInvoice(businessId: number, reservationId: number): Observable<InvoiceDto> {
    return this.http.post<InvoiceDto>(`${this.apiUrl}/businesses/${businessId}/invoices/generate`, { reservationId });
  }

  getInvoices(businessId: number, page: number = 1, pageSize: number = 15, search?: string, month?: string): Observable<PagedResult<InvoiceDto>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (search) {
      params = params.set('search', search);
    }
    if (month) {
      params = params.set('month', month);
    }

    return this.http.get<PagedResult<InvoiceDto>>(`${this.apiUrl}/businesses/${businessId}/invoices`, { params });
  }
}
