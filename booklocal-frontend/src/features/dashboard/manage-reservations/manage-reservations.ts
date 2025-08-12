import { Component, OnInit, ViewEncapsulation, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { CalendarEvent, CalendarModule, CalendarView } from 'angular-calendar';
import { finalize } from 'rxjs';

import { ReservationService } from '../../../core/services/reservation';
import { Reservation } from '../../../types/reservation.model';
import { Employee } from '../../../types/business.model';
import { AddReservationOwnerModalComponent } from '../../../shared/components/add-reservation-owner-modal/add-reservation-owner-modal';

@Component({
  selector: 'app-manage-reservations',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    DatePipe,
    CalendarModule,
    AddReservationOwnerModalComponent
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

  isAddModalVisible = false;
  newReservationDetails: { date: Date, employee?: Employee } | null = null;

  private statusColors: Record<string, { primary: string; secondary: string; }> = {
    Confirmed: {
      primary: '#3b82f6',
      secondary: '#dbeafe',
    },
    Completed: {
      primary: '#6b7280',
      secondary: '#e5e7eb',
    },
    Cancelled: {
      primary: '#ef4444',
      secondary: '#fee2e2',
    }
  };

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
    const customerName = reservation.customerFullName || reservation.guestName || 'Gość';
    const eventColor = this.statusColors[reservation.status] || { primary: '#a855f7', secondary: '#f3e8ff' };

    return {
      id: reservation.reservationId,
      start: new Date(reservation.startTime),
      end: new Date(reservation.endTime),
      title: `${new Date(reservation.startTime).toLocaleTimeString('pl-PL', { hour: '2-digit', minute: '2-digit' })} - ${customerName} (${reservation.serviceName})`,
      color: { ...eventColor },
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

  slotClicked(date: Date): void {
    this.newReservationDetails = { date };
    this.isAddModalVisible = true;
  }

  openAddReservationModal(): void {
    this.newReservationDetails = { date: new Date() };
    this.isAddModalVisible = true;
  }
  
  closeAddModal(refetch: boolean): void {
    this.isAddModalVisible = false;
    if (refetch) {
      this.loadReservations();
    }
  }

  dateChanged(): void {
  }
}