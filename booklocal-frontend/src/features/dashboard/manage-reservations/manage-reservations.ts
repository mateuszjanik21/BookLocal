import { Component, OnInit, ViewEncapsulation, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { CalendarEvent, CalendarModule, CalendarView } from 'angular-calendar';

import { ReservationService } from '../../../core/services/reservation';
import { Reservation } from '../../../types/reservation.model';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-manage-reservations',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    DatePipe,
    CalendarModule,
  ],
  templateUrl: './manage-reservations.html',
  styleUrl: './manage-reservations.css',
  encapsulation: ViewEncapsulation.None
})
export class ManageReservationsComponent implements OnInit {
  private reservationService = inject(ReservationService);
  private router = inject(Router);
  private toastr = inject(ToastrService);

  isLoading = true;
  
  view: CalendarView = CalendarView.Week;
  CalendarView = CalendarView;
  viewDate: Date = new Date();
  events: CalendarEvent[] = [];

  ngOnInit(): void {
    this.loadReservations();
  }

  loadReservations(): void {
    this.isLoading = true;
    this.reservationService.getMyReservations().pipe(
      finalize(() => this.isLoading = false)
    ).subscribe({
      next: (data) => {
        this.events = data.map(res => this.mapReservationToCalendarEvent(res));
      },
      error: (err) => {
        this.toastr.error("Błąd podczas pobierania rezerwacji.");
      }
    });
  }
  
  private mapReservationToCalendarEvent(reservation: Reservation): CalendarEvent<{ reservation: Reservation }> {
    const statusColors = {
      confirmed: { primary: '#3ABFF8', secondary: '#C5EEFF' },
      cancelled: { primary: '#F87272', secondary: '#FEE3E3' },
      completed: { primary: '#A9A9A9', secondary: '#E5E7EB' },
    };
    const color = statusColors[reservation.status.toLowerCase() as keyof typeof statusColors] 
                  || { primary: '#6c757d', secondary: '#E2E3E5' };

    return {
      id: reservation.reservationId,
      start: new Date(reservation.startTime),
      end: new Date(reservation.endTime),
      title: `${new Date(reservation.startTime).toLocaleTimeString('pl-PL', { hour: '2-digit', minute: '2-digit' })} - ${reservation.customerFullName} (${reservation.serviceName})`,
      color: { ...color },
      meta: {
        reservation
      }
    };
  }

  setView(view: CalendarView): void {
    this.view = view;
  }

  eventClicked({ event }: { event: CalendarEvent }): void {
    if (event.id) {
      this.router.navigate(['/dashboard/reservations', event.id]);
    }
  }

  dateChanged(): void {
  }
}