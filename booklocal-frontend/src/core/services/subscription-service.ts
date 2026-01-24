import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface SubscriptionPlan {
  planId: number;
  name: string;
  priceMonthly: number;
  priceYearly: number;
  maxEmployees: number;
  maxServices: number;
  hasAdvancedReports: boolean;
  hasMarketingTools: boolean;
  commissionPercentage: number;
  isActive: boolean;
}

export interface CurrentSubscription {
  planName: string;
  planId?: number;
  startDate?: string;
  endDate?: string;
  price?: number;
  isActive: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class SubscriptionService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/subscription`;

  getPublicPlans(): Observable<SubscriptionPlan[]> {
    return this.http.get<SubscriptionPlan[]>(`${this.apiUrl}/plans`);
  }

  subscribe(planId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/subscribe`, planId);
  }

  getCurrentSubscription(): Observable<CurrentSubscription> {
    return this.http.get<CurrentSubscription>(`${this.apiUrl}/current`);
  }
}
