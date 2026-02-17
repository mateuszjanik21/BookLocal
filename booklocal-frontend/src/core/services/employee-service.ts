import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { EmployeeDetail, EmployeePayload, EmployeeCertificateDto, ScheduleExceptionDto } from '../../types/employee.models';
import { Employee } from '../../types/business.model';

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getEmployees(businessId: number): Observable<Employee[]> {
    return this.http.get<Employee[]>(`${this.apiUrl}/businesses/${businessId}/employees`);
  }

  getAssignedServiceIds(businessId: number, employeeId: number): Observable<number[]> {
    return this.http.get<number[]>(`${this.apiUrl}/businesses/${businessId}/employees/${employeeId}/services`);
  }

  assignServices(businessId: number, employeeId: number, serviceIds: number[]) {
    return this.http.post(`${this.apiUrl}/businesses/${businessId}/employees/${employeeId}/services`, { serviceIds });
  }

  addEmployee(businessId: number, payload: EmployeePayload) {
    return this.http.post<Employee>(`${this.apiUrl}/businesses/${businessId}/employees`, payload);
  }

  updateEmployee(businessId: number, employeeId: number, payload: EmployeePayload) {
    return this.http.put(`${this.apiUrl}/businesses/${businessId}/employees/${employeeId}`, payload);
  }

  deleteEmployee(businessId: number, employeeId: number) {
    return this.http.delete(`${this.apiUrl}/businesses/${businessId}/employees/${employeeId}`);
  }

  getEmployeeDetails(businessId: number, employeeId: number): Observable<EmployeeDetail> {
    return this.http.get<EmployeeDetail>(`${this.apiUrl}/businesses/${businessId}/employees/${employeeId}/details`);
  }

  addCertificate(businessId: number, employeeId: number, payload: { name: string; institution?: string; dateObtained: string; isVisibleToClient: boolean }): Observable<EmployeeCertificateDto> {
    return this.http.post<EmployeeCertificateDto>(`${this.apiUrl}/businesses/${businessId}/employees/${employeeId}/certificates`, payload);
  }

  deleteCertificate(businessId: number, employeeId: number, certId: number) {
    return this.http.delete(`${this.apiUrl}/businesses/${businessId}/employees/${employeeId}/certificates/${certId}`);
  }

  addAbsence(businessId: number, employeeId: number, payload: { dateFrom: string; dateTo: string; type: string; reason?: string; blocksCalendar: boolean }): Observable<ScheduleExceptionDto> {
    return this.http.post<ScheduleExceptionDto>(`${this.apiUrl}/businesses/${businessId}/employees/${employeeId}/absences`, payload);
  }

  toggleAbsenceApproval(businessId: number, employeeId: number, absenceId: number) {
    return this.http.patch(`${this.apiUrl}/businesses/${businessId}/employees/${employeeId}/absences/${absenceId}/approve`, {});
  }

  deleteAbsence(businessId: number, employeeId: number, absenceId: number) {
    return this.http.delete(`${this.apiUrl}/businesses/${businessId}/employees/${employeeId}/absences/${absenceId}`);
  }

  updateFinanceSettings(businessId: number, employeeId: number, payload: any) {
    return this.http.put(`${this.apiUrl}/businesses/${businessId}/employees/${employeeId}/finance-settings`, payload);
  }
}