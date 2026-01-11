import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { EmploymentContract, EmploymentContractUpsert, EmployeePayroll, GeneratePayrollRequest } from '../../types/hr.models';

@Injectable({
  providedIn: 'root'
})
export class HRService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getContracts(businessId: number): Observable<EmploymentContract[]> {
    return this.http.get<EmploymentContract[]>(`${this.apiUrl}/businesses/${businessId}/hr/contracts`);
  }

  createContract(businessId: number, payload: EmploymentContractUpsert): Observable<EmploymentContract> {
    return this.http.post<EmploymentContract>(`${this.apiUrl}/businesses/${businessId}/hr/contracts`, payload);
  }

  getPayrolls(businessId: number, month?: number, year?: number): Observable<EmployeePayroll[]> {
    let params = new HttpParams();
    if (month) params = params.set('month', month);
    if (year) params = params.set('year', year);

    return this.http.get<EmployeePayroll[]>(`${this.apiUrl}/businesses/${businessId}/hr/payrolls`, { params });
  }

  generatePayroll(businessId: number, payload: GeneratePayrollRequest): Observable<EmployeePayroll> {
    return this.http.post<EmployeePayroll>(`${this.apiUrl}/businesses/${businessId}/hr/payrolls/generate`, payload);
  }
}
