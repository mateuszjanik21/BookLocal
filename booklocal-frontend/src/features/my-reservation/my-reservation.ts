import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReservationService } from '../../core/services/reservation';
import { Reservation } from '../../types/reservation.model';

@Component({
  selector: 'app-my-reservations',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-reservation.html',
})
export class MyReservationsComponent implements OnInit {
  private reservationService = inject(ReservationService);
  reservations: Reservation[] = [];
  isLoading = true;

  ngOnInit(): void {
    this.reservationService.getMyReservations().subscribe(data => {
      this.reservations = data;
      this.isLoading = false;
    });
  }
}