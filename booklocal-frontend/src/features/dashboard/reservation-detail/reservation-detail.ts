import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ReservationService } from '../../../core/services/reservation';
import { Reservation } from '../../../types/reservation.model';
import { ChatService } from '../../../core/services/chat'; 
import { ReservationStatusPipe } from '../../../shared/pipes/reservation-status.pipe';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-reservation-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, ReservationStatusPipe],
  templateUrl: './reservation-detail.html',
})
export class ReservationDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private reservationService = inject(ReservationService);
  private chatService = inject(ChatService);
  private toastr = inject(ToastrService);
  
  reservation: Reservation | null = null;
  isLoading = true;
  isUpdating = false;

  statuses = ['Confirmed', 'Completed', 'Cancelled'];

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadReservation(+id);
    }
  }

  loadReservation(id: number): void {
    this.isLoading = true;
    this.reservationService.getReservationById(id).pipe(
      finalize(() => this.isLoading = false)
    ).subscribe({
      next: (data) => {
        this.reservation = data;
      },
      error: () => {
        this.toastr.error('Nie udało się załadować danych rezerwacji.');
        this.router.navigate(['/dashboard/reservations']);
      }
    });
  }

  startChat(customerId: string): void {
    this.chatService.startConversationAsOwner(customerId).subscribe({
      next: () => {
        this.router.navigate(['/dashboard/chat']);
      },
      error: () => {
        this.toastr.error("Nie udało się rozpocząć konwersacji.");
      }
    });
  }

  changeStatus(newStatus: string): void {
    if (!this.reservation || this.reservation.status === newStatus || this.isUpdating) {
      return;
    }

    if (confirm(`Czy na pewno chcesz zmienić status rezerwacji na "${newStatus}"?`)) {
      this.isUpdating = true;
      this.reservationService.updateReservationStatus(this.reservation.reservationId, newStatus).pipe(
        finalize(() => this.isUpdating = false)
      ).subscribe({
        next: () => {
          this.toastr.success('Status rezerwacji został zaktualizowany.');
          if (this.reservation) {
            this.reservation.status = newStatus;
          }
        },
        error: () => this.toastr.error('Wystąpił błąd podczas aktualizacji statusu.')
      });
    }
  }
}