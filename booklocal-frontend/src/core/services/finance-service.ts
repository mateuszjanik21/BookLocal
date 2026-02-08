import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { DailyEmployeePerformance } from '../../types/report.model';

export interface DailyFinancialReport {
  reportId: number;
  businessId: number;
  reportDate: string;
  totalRevenue: number;
  tipsAmount: number;
  averageTicketValue: number;
  cashRevenue: number;
  cardRevenue: number;
  onlineRevenue: number;
  totalAppointments: number;
  completedAppointments: number;
  cancelledAppointments: number;
  noShowCount: number;
  newCustomersCount: number;
  returningCustomersCount: number;
  occupancyRate: number;
  topSellingServiceName: string;
  totalCommission: number;
}

@Injectable({
  providedIn: 'root'
})
export class FinanceService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  generateDailyReport(businessId: number, date: string): Observable<DailyFinancialReport> {
    const params = new HttpParams().set('date', date);
    return this.http.post<DailyFinancialReport>(`${this.apiUrl}/businesses/${businessId}/finance/generate-daily-report`, {}, { params });
  }

  generateReportRange(businessId: number, startDate: string, endDate: string): Observable<any> {
    const params = new HttpParams().set('startDate', startDate).set('endDate', endDate);
    return this.http.post(`${this.apiUrl}/businesses/${businessId}/finance/generate-range`, {}, { params });
  }

  deleteReport(businessId: number, date: string): Observable<any> {
    const params = new HttpParams().set('date', date);
    return this.http.delete(`${this.apiUrl}/businesses/${businessId}/finance/report`, { params });
  }

  getReports(businessId: number, month: number, year: number): Observable<DailyFinancialReport[]> {
    const params = new HttpParams()
      .set('month', month.toString())
      .set('year', year.toString());
    return this.http.get<DailyFinancialReport[]>(`${this.apiUrl}/businesses/${businessId}/finance/reports`, { params });
  }

  getLiveReports(businessId: number, startDate: string, endDate: string): Observable<DailyFinancialReport[]> {
    const params = new HttpParams()
      .set('startDate', startDate)
      .set('endDate', endDate);
    return this.http.get<DailyFinancialReport[]>(`${this.apiUrl}/businesses/${businessId}/finance/reports-live`, { params });
  }

  getEmployeePerformance(businessId: number, date?: string, startDate?: string, endDate?: string): Observable<DailyEmployeePerformance[]> {
      let params = new HttpParams();
      if (date) params = params.set('date', date);
      if (startDate && endDate) {
          params = params.set('startDate', startDate).set('endDate', endDate);
      }
    return this.http.get<DailyEmployeePerformance[]>(`${this.apiUrl}/businesses/${businessId}/finance/employee-performance`, { params });
  }
}
