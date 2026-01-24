import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
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
  hasAdvancedReports?: boolean;
  hasMarketingTools?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class SubscriptionService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/subscription`;

  private _currentSubscription = new BehaviorSubject<CurrentSubscription | null>(null);
  public currentSubscription$ = this._currentSubscription.asObservable();

  constructor() {
  }

  getPublicPlans(): Observable<SubscriptionPlan[]> {
    return this.http.get<SubscriptionPlan[]>(`${this.apiUrl}/plans`);
  }

  subscribe(planId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/subscribe`, planId).pipe(
        tap(() => this.refreshSubscription())
    );
  }

  getCurrentSubscription(): Observable<CurrentSubscription> {
    return this.http.get<CurrentSubscription>(`${this.apiUrl}/current`).pipe(
        tap(sub => this._currentSubscription.next(sub))
    );
  }
  
  refreshSubscription() {
      this.getCurrentSubscription().subscribe({
          error: (err) => console.error('Failed to refresh subscription', err)
      });
  }

  createPlan(plan: Partial<SubscriptionPlan>): Observable<SubscriptionPlan> {
    return this.http.post<SubscriptionPlan>(this.apiUrl, plan);
  }

  updatePlan(id: number, plan: Partial<SubscriptionPlan>): Observable<SubscriptionPlan> {
    return this.http.put<SubscriptionPlan>(`${this.apiUrl}/${id}`, plan);
  }

  deletePlan(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
