import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ReservationService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  createReservation(payload: { serviceId: number; employeeId: number; startTime: string; }) {
    return this.http.post(`${this.apiUrl}/reservations`, payload);
  }
}