import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface LoyaltyConfig {
  isActive: boolean;
  spendAmountForOnePoint: number;
}

export interface LoyaltyBalance {
  pointsBalance: number;
  totalPointsEarned: number;
}

export interface LoyaltyTransaction {
  transactionId: number;
  pointsAmount: number;
  type: string;
  description: string;
  createdAt: string;
}

export interface CustomerLoyaltyData {
  balance: LoyaltyBalance;
  transactions: LoyaltyTransaction[];
}

@Injectable({
  providedIn: 'root'
})
export class LoyaltyService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getConfig(businessId: number): Observable<LoyaltyConfig> {
    return this.http.get<LoyaltyConfig>(`${this.apiUrl}/businesses/${businessId}/loyalty/config`);
  }

  updateConfig(businessId: number, config: LoyaltyConfig): Observable<LoyaltyConfig> {
    return this.http.put<LoyaltyConfig>(`${this.apiUrl}/businesses/${businessId}/loyalty/config`, config);
  }

  getCustomerLoyalty(businessId: number, customerId: string): Observable<CustomerLoyaltyData> {
    return this.http.get<CustomerLoyaltyData>(`${this.apiUrl}/businesses/${businessId}/loyalty/customer/${customerId}`);
  }

  recalculatePoints(businessId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/businesses/${businessId}/loyalty/recalculate`, {});
  }
}
