import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReservationService } from '../../core/services/reservation';
import { Reservation } from '../../types/reservation.model';
import { RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { ReservationStatusPipe } from '../../shared/pipes/reservation-status.pipe';

@Component({
  selector: 'app-my-reservations',
  standalone: true,
  imports: [CommonModule, RouterModule, ReservationStatusPipe],
  templateUrl: './my-reservation.html',
})
export class MyReservationsComponent implements OnInit {
  private reservationService = inject(ReservationService);
  private toastr = inject(ToastrService);

  upcomingReservations: Reservation[] = [];
  pastReservations: Reservation[] = [];
  isLoading = true;

  ngOnInit(): void {
    this.loadReservations();
  }

  loadReservations(): void {
    this.isLoading = true;
    this.reservationService.getMyReservations().subscribe(data => {
      const now = new Date();
      this.upcomingReservations = data.filter(r => new Date(r.startTime) >= now);
      this.pastReservations = data.filter(r => new Date(r.startTime) < now);
      this.isLoading = false;
    });
  }

  onCancelReservation(reservationId: number): void {
    if (confirm('Czy na pewno chcesz anulować tę rezerwację?')) {
      this.reservationService.cancelReservation(reservationId).subscribe({
        next: () => {
          this.toastr.success('Twoja rezerwacja została anulowana.');
          this.loadReservations(); 
        },
        error: (err) => {
          this.toastr.error(err.error.message || 'Nie udało się anulować rezerwacji.');
        }
      });
    }
  }
}