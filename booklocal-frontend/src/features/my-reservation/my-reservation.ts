import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { ReservationService } from '../../core/services/reservation';
import { Reservation } from '../../types/reservation.model';
import { ReservationStatusPipe } from '../../shared/pipes/reservation-status.pipe';
import { AddReviewModalComponent } from '../../shared/components/add-review-modal/add-review-modal';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-my-reservations',
  standalone: true,
  imports: [CommonModule, RouterModule, ReservationStatusPipe, AddReviewModalComponent],
  templateUrl: './my-reservation.html',
})
export class MyReservationsComponent implements OnInit {
  private reservationService = inject(ReservationService);
  private toastr = inject(ToastrService);

  activeTab: 'upcoming' | 'past' = 'upcoming';

  upcomingReservations: Reservation[] = [];
  isLoadingUpcoming = true;
  isLoadingMoreUpcoming = false;
  pageNumberUpcoming = 1;
  totalCountUpcoming = 0;

  pastReservations: Reservation[] = [];
  isLoadingPast = true;
  isLoadingMorePast = false;
  pageNumberPast = 1;
  totalCountPast = 0;

  pageSize = 10;
  
  isReviewModalVisible = false;
  reservationToReview: Reservation | null = null;

  ngOnInit(): void {
    this.loadUpcomingReservations(true);
    this.loadPastReservations(true);
  }

  loadUpcomingReservations(isInitialLoad = false): void {
    if (isInitialLoad) {
      this.isLoadingUpcoming = true;
      this.pageNumberUpcoming = 1;
    } else {
      this.isLoadingMoreUpcoming = true;
    }

    this.reservationService.getMyReservations('upcoming', this.pageNumberUpcoming, this.pageSize)
      .pipe(finalize(() => {
        this.isLoadingUpcoming = false;
        this.isLoadingMoreUpcoming = false;
      }))
      .subscribe(data => {
        this.upcomingReservations = isInitialLoad ? data.items : [...this.upcomingReservations, ...data.items];
        this.totalCountUpcoming = data.totalCount;
      });
  }

  loadPastReservations(isInitialLoad = false): void {
    if (isInitialLoad) {
      this.isLoadingPast = true;
      this.pageNumberPast = 1;
    } else {
      this.isLoadingMorePast = true;
    }
    
    this.reservationService.getMyReservations('past', this.pageNumberPast, this.pageSize)
      .pipe(finalize(() => {
        this.isLoadingPast = false;
        this.isLoadingMorePast = false;
      }))
      .subscribe(data => {
        this.pastReservations = isInitialLoad ? data.items : [...this.pastReservations, ...data.items];
        this.totalCountPast = data.totalCount;
      });
  }

  loadMoreUpcoming(): void {
    if (!this.hasMoreUpcoming) return;
    this.pageNumberUpcoming++;
    this.loadUpcomingReservations();
  }
  
  loadMorePast(): void {
    if (!this.hasMorePast) return;
    this.pageNumberPast++;
    this.loadPastReservations();
  }

  get hasMoreUpcoming(): boolean {
    return this.upcomingReservations.length < this.totalCountUpcoming;
  }
  
  get hasMorePast(): boolean {
    return this.pastReservations.length < this.totalCountPast;
  }

  onCancelReservation(reservationId: number): void {
    if (confirm('Czy na pewno chcesz anulować tę rezerwację?')) {
      this.reservationService.cancelReservation(reservationId).subscribe({
        next: () => {
          this.toastr.success('Twoja rezerwacja została anulowana.');
          this.loadUpcomingReservations(true);
          this.loadPastReservations(true); 
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
      this.loadPastReservations(true);
    }
  }
}