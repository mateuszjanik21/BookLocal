import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { WorkSchedule } from '../../types/schedule.model';

@Injectable({
  providedIn: 'root'
})
export class ScheduleService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getSchedule(employeeId: number): Observable<WorkSchedule[]> {
    return this.http.get<WorkSchedule[]>(`${this.apiUrl}/schedules/${employeeId}`);
  }

  updateSchedule(employeeId: number, schedule: WorkSchedule[]): Observable<any> {
    return this.http.put(`${this.apiUrl}/schedules/${employeeId}`, schedule);
  }
}