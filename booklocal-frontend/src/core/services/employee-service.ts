import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { EmployeePayload } from '../../types/employee.models';
import { Employee } from '../../types/business.model';

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

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
}