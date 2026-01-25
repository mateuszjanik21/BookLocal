import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

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

export interface CreateSubscriptionPlan {
  name: string;
  priceMonthly: number;
  priceYearly: number;
  maxEmployees: number;
  maxServices: number;
  hasAdvancedReports: boolean;
  hasMarketingTools: boolean;
  commissionPercentage: number;
}

export interface AdminBusinessListDto {
  businessId: number;
  name: string;
  ownerEmail: string;
  createdAt: string;
  isVerified: boolean;
  verificationStatus: string;
  subscriptionPlanName: string;
}

export interface VerifyBusinessDto {
  isApproved: boolean;
  rejectionReason?: string;
}

export interface AdminStats {
  totalBusinesses: number;
  newBusinessesThisMonth: number;
  activeSubscriptions: number;
  totalRevenue: number;
  pendingVerifications: number;
}

@Injectable({
  providedIn: 'root',
})
export class AdminService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl + '/admin';

  getStats(): Observable<AdminStats> {
    return this.http.get<AdminStats>(`${this.apiUrl}/stats`);
  }

  getPlans(): Observable<SubscriptionPlan[]> {
    return this.http.get<SubscriptionPlan[]>(`${this.apiUrl}/plans`);
  }

  createPlan(plan: CreateSubscriptionPlan): Observable<SubscriptionPlan> {
    return this.http.post<SubscriptionPlan>(`${this.apiUrl}/plans`, plan);
  }

  updatePlan(id: number, plan: CreateSubscriptionPlan): Observable<any> {
    return this.http.put(`${this.apiUrl}/plans/${id}`, plan);
  }

  deletePlan(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/plans/${id}`);
  }

  getBusinesses(status?: string): Observable<AdminBusinessListDto[]> {
    let params = new HttpParams();
    if (status) {
      params = params.set('status', status);
    }
    return this.http.get<AdminBusinessListDto[]>(`${this.apiUrl}/businesses`, {
      params,
    });
  }

  verifyBusiness(businessId: number, dto: VerifyBusinessDto): Observable<any> {
    return this.http.patch(
      `${this.apiUrl}/businesses/${businessId}/verify`,
      dto
    );
  }
}
