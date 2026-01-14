import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { OwnerCreateReservationPayload, Reservation } from '../../types/reservation.model'; 
import { Observable } from 'rxjs';
import { PagedResult } from '../../types/business.model';

@Injectable({
  providedIn: 'root'
})
export class ReservationService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getAvailableSlots(employeeId: number, serviceId: number, date: string): Observable<string[]> {
    const params = new HttpParams()
      .set('serviceVariantId', serviceId.toString())
      .set('date', date);
    return this.http.get<string[]>(`${this.apiUrl}/employees/${employeeId}/availability`, { params });
  }

  createReservation(payload: { serviceVariantId: number; employeeId: number; startTime: string; discountCode?: string; paymentMethod: string }) {
    return this.http.post(`${this.apiUrl}/reservations`, payload);
  }

  getMyReservations(scope: string, pageNumber: number, pageSize: number): Observable<PagedResult<Reservation>> {
    const params = new HttpParams()
      .set('scope', scope)
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<PagedResult<Reservation>>(`${this.apiUrl}/reservations/my-reservations`, { params });
  }

  getCalendarEvents(): Observable<Reservation[]> {
    return this.http.get<Reservation[]>(`${this.apiUrl}/reservations/calendar`);
  }

  getReservationById(id: number): Observable<Reservation> {
    return this.http.get<Reservation>(`${this.apiUrl}/reservations/${id}`);
  }

  updateReservationStatus(id: number, status: string): Observable<any> {
    return this.http.patch(`${this.apiUrl}/reservations/${id}/status`, { status });
  }

  cancelReservation(id: number): Observable<any> {
    return this.http.patch(`${this.apiUrl}/reservations/my-reservations/${id}/cancel`, {});
  }

  createReservationAsOwner(payload: OwnerCreateReservationPayload): Observable<any> {
    return this.http.post(`${this.apiUrl}/reservations/dashboard/reservations`, payload);
  }
}