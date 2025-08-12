import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

import { ReservationService } from '../../core/services/reservation';
import { Reservation } from '../../types/reservation.model';
import { ReservationStatusPipe } from '../../shared/pipes/reservation-status.pipe';
// NOWOŚĆ: Import modala do dodawania opinii
import { AddReviewModalComponent } from '../../shared/components/add-review-modal/add-review-modal';

@Component({
  selector: 'app-my-reservations',
  standalone: true,
  // NOWOŚĆ: Dodajemy modal i pipę do importów
  imports: [CommonModule, RouterModule, ReservationStatusPipe, AddReviewModalComponent],
  templateUrl: './my-reservation.html',
})
export class MyReservationsComponent implements OnInit {
  private reservationService = inject(ReservationService);
  private toastr = inject(ToastrService);

  upcomingReservations: Reservation[] = [];
  pastReservations: Reservation[] = [];
  isLoading = true;

  // NOWOŚĆ: Właściwości do zarządzania modalem opinii
  isReviewModalVisible = false;
  reservationToReview: Reservation | null = null;

  ngOnInit(): void {
    this.loadReservations();
  }

  loadReservations(): void {
    this.isLoading = true;
    this.reservationService.getMyReservations().subscribe(data => {
      const now = new Date();
      // Sortowanie od najnowszych do najstarszych
      data.sort((a, b) => new Date(b.startTime).getTime() - new Date(a.startTime).getTime());
      
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
          const errorMessage = err?.error?.message || err?.error || 'Nie udało się anulować rezerwacji. Spróbuj ponownie.';
          this.toastr.error(errorMessage);
        }
      });
    }
  }

  openReviewModal(reservation: Reservation): void {
    this.reservationToReview = reservation;
    this.isReviewModalVisible = true;
  }
  
  closeReviewModal(reviewAdded: boolean): void {
    this.isReviewModalVisible = false;
    this.reservationToReview = null;
    if (reviewAdded) {
      this.loadReservations();
    }
  }
}