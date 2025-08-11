import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { Reservation } from '../../types/reservation.model'; 
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ReservationService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getAvailableSlots(employeeId: number, serviceId: number, date: string): Observable<string[]> {
    const params = new HttpParams()
      .set('serviceId', serviceId.toString())
      .set('date', date);
    return this.http.get<string[]>(`${this.apiUrl}/employees/${employeeId}/availability`, { params });
  }

  createReservation(payload: { serviceId: number; employeeId: number; startTime: string; }) {
    return this.http.post(`${this.apiUrl}/reservations`, payload);
  }

  getMyReservations(): Observable<Reservation[]> {
    return this.http.get<Reservation[]>(`${this.apiUrl}/reservations/my-reservations`);
  }

  getReservationById(id: number): Observable<Reservation> {
    return this.http.get<Reservation>(`${this.apiUrl}/reservations/${id}`);
  }
}