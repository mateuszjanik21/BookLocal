import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReservationService } from '../../core/services/reservation';
import { Reservation } from '../../types/reservation.model';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-my-reservations',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './my-reservation.html',
})
export class MyReservationsComponent implements OnInit {
  private reservationService = inject(ReservationService);

  upcomingReservations: Reservation[] = [];
  pastReservations: Reservation[] = [];
  isLoading = true;

  ngOnInit(): void {
    this.reservationService.getMyReservations().subscribe(data => {
      const now = new Date();
      this.upcomingReservations = data.filter(r => new Date(r.startTime) >= now);
      this.pastReservations = data.filter(r => new Date(r.startTime) < now);
      this.isLoading = false;
    });
  }
}