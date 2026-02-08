import { Component, OnInit, ViewEncapsulation, inject, HostListener } from '@angular/core';
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

  get viewTitle(): string {
    const datePipe = new DatePipe('pl-PL');
    
    if (this.view === CalendarView.Month) {
      return this.maximizeFirstLetter(datePipe.transform(this.viewDate, 'LLLL yyyy') || '');
    } else if (this.view === CalendarView.Week) {
      const startOfWeek = this.getStartOfWeek(this.viewDate);
      const endOfWeek = this.getEndOfWeek(this.viewDate);
      
      const startDay = datePipe.transform(startOfWeek, 'd');
      const endDay = datePipe.transform(endOfWeek, 'd');
      const month = this.maximizeFirstLetter(datePipe.transform(endOfWeek, 'MMMM') || '');
      
      if (startOfWeek.getMonth() === endOfWeek.getMonth()) {
         return `${startDay} - ${endDay} ${month}`;
      } else {
         const startMonth = this.maximizeFirstLetter(datePipe.transform(startOfWeek, 'MMM') || '');
         return `${startDay} ${startMonth} - ${endDay} ${month}`;
      }
    } else {
      return this.maximizeFirstLetter(datePipe.transform(this.viewDate, 'd MMMM yyyy') || '');
    }
  }

  private maximizeFirstLetter(string: string): string {
    return string.charAt(0).toUpperCase() + string.slice(1);
  }

  private getStartOfWeek(date: Date): Date {
    const day = date.getDay();
    const diff = date.getDate() - day + (day === 0 ? -6 : 1); 
    return new Date(date.setDate(diff));
  }
  
  private getEndOfWeek(date: Date): Date {
    const start = this.getStartOfWeek(new Date(date));
    return new Date(start.setDate(start.getDate() + 6));
  }

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
    this.checkScreenSize();
    this.loadReservations();
  }

  @HostListener('window:resize', ['$event'])
  onResize(event: any) {
    this.checkScreenSize();
  }

  private checkScreenSize() {
    if (window.innerWidth < 768) {
      this.view = CalendarView.Day;
    }
  }

  loadReservations(): void {
    this.isLoading = true;

    let start: Date;
    let end: Date;

    if (this.view === CalendarView.Month) {
      start = new Date(this.viewDate.getFullYear(), this.viewDate.getMonth(), 1);
      end = new Date(this.viewDate.getFullYear(), this.viewDate.getMonth() + 1, 0, 23, 59, 59);
       
       const startDay = start.getDay(); 
       const diffStart = start.getDate() - startDay + (startDay === 0 ? -6 : 1);
       start.setDate(diffStart); 

       const endDay = end.getDay();
       const diffEnd = 7 - (endDay === 0 ? 7 : endDay);
       end.setDate(end.getDate() + diffEnd);
    } else if (this.view === CalendarView.Week) {
      start = this.getStartOfWeek(new Date(this.viewDate));
      start.setHours(0, 0, 0, 0);
      end = this.getEndOfWeek(new Date(this.viewDate));
      end.setHours(23, 59, 59);
    } else {
      start = new Date(this.viewDate);
      start.setHours(0, 0, 0, 0);
      end = new Date(this.viewDate);
      end.setHours(23, 59, 59);
    }

    const toLocalISO = (date: Date): string => {
        const offset = date.getTimezoneOffset() * 60000;
        return new Date(date.getTime() - offset).toISOString().slice(0, 19);
    };

    this.reservationService.getCalendarEvents(toLocalISO(start), toLocalISO(end)).pipe(
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
    this.loadReservations();
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
    this.loadReservations();
  }
}