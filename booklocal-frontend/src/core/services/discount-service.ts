import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Discount {
  discountId: number;
  code: string;
  type: 'Percentage' | 'FixedAmount';
  value: number;
  maxUses?: number;
  usedCount: number;
  validFrom?: string;
  validTo?: string;
  isActive: boolean;
  serviceId?: number;
}

export interface CreateDiscountDto {
  code: string;
  type: number;
  value: number;
  maxUses?: number;
  validFrom?: string;
  validTo?: string;
  serviceId?: number;
}

export interface VerifyDiscountRequest {
    code: string;
    serviceId?: number;
    originalPrice: number;
}

export interface VerifyDiscountResult {
    isValid: boolean;
    message?: string;
    discountId?: number;
    discountAmount: number;
    finalPrice: number;
}

@Injectable({
  providedIn: 'root'
})
export class DiscountService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getDiscounts(businessId: number): Observable<Discount[]> {
    return this.http.get<Discount[]>(`${this.apiUrl}/businesses/${businessId}/discounts`);
  }

  createDiscount(businessId: number, dto: CreateDiscountDto): Observable<Discount> {
    return this.http.post<Discount>(`${this.apiUrl}/businesses/${businessId}/discounts`, dto);
  }

  toggleDiscount(businessId: number, discountId: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/businesses/${businessId}/discounts/${discountId}/toggle`, {});
  }

  verifyDiscount(businessId: number, req: VerifyDiscountRequest): Observable<VerifyDiscountResult> {
      return this.http.post<VerifyDiscountResult>(`${this.apiUrl}/businesses/${businessId}/discounts/verify`, req);
  }
}
