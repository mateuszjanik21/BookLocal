import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { EmploymentContract, EmploymentContractUpsert, EmployeePayroll, GeneratePayrollRequest, HrMonthlySummary } from '../../types/hr.models';
import { Employee } from '../../types/business.model';

@Injectable({
  providedIn: 'root'
})
export class HRService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getContracts(businessId: number): Observable<EmploymentContract[]> {
    return this.http.get<EmploymentContract[]>(`${this.apiUrl}/businesses/${businessId}/hr/contracts`);
  }

  getEmployeesForHr(businessId: number): Observable<Employee[]> {
    return this.http.get<Employee[]>(`${this.apiUrl}/businesses/${businessId}/hr/employees`);
  }

  createContract(businessId: number, payload: EmploymentContractUpsert): Observable<EmploymentContract> {
    return this.http.post<EmploymentContract>(`${this.apiUrl}/businesses/${businessId}/hr/contracts`, payload);
  }

  updateContract(businessId: number, contractId: number, payload: EmploymentContractUpsert): Observable<EmploymentContract> {
    return this.http.put<EmploymentContract>(`${this.apiUrl}/businesses/${businessId}/hr/contracts/${contractId}`, payload);
  }

  getPayrolls(businessId: number, month?: number, year?: number): Observable<EmployeePayroll[]> {
    let params = new HttpParams();
    if (month) params = params.set('month', month);
    if (year) params = params.set('year', year);
    return this.http.get<EmployeePayroll[]>(`${this.apiUrl}/businesses/${businessId}/hr/payrolls`, { params });
  }

  getMonthlySummary(businessId: number, endMonth: number, endYear: number, count: number = 6): Observable<HrMonthlySummary[]> {
    let params = new HttpParams()
      .set('endMonth', endMonth)
      .set('endYear', endYear)
      .set('count', count);
    return this.http.get<HrMonthlySummary[]>(`${this.apiUrl}/businesses/${businessId}/hr/monthly-summary`, { params });
  }

  generatePayroll(businessId: number, payload: GeneratePayrollRequest): Observable<EmployeePayroll> {
    return this.http.post<EmployeePayroll>(`${this.apiUrl}/businesses/${businessId}/hr/payrolls/generate`, payload);
  }

  generatePayrollForAll(businessId: number, employeeIds: number[], month: number, year: number, day?: number): Observable<any[]> {
    const requests = employeeIds.map(id =>
      this.generatePayroll(businessId, { employeeId: id, month, year, day }).pipe(
        catchError((err) => {
          if (err.status === 400 && err.error) {
              return of({ isError: true, message: err.error });
          }
          return of(null);
        })
      )
    );
    return forkJoin(requests);
  }
  archiveContract(businessId: number, contractId: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/businesses/${businessId}/hr/contracts/${contractId}/archive`, {});
  }

  deletePayroll(businessId: number, payrollId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/businesses/${businessId}/hr/payrolls/${payrollId}`);
  }
}
