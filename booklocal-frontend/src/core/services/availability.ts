import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AvailabilityService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getAvailableSlots(employeeId: number, date: string, serviceVariantId: number): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/employees/${employeeId}/availability`, {
      params: { date, serviceVariantId }
    });
  }

  getBundleAvailableSlots(employeeId: number, date: string, bundleId: number): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/employees/${employeeId}/availability/bundle`, {
      params: { date, bundleId }
    });
  }
}
