import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ReservationService } from '../../../core/services/reservation';
import { Reservation } from '../../../types/reservation.model';

@Component({
  selector: 'app-reservation-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './reservation-detail.html',
})
export class ReservationDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private reservationService = inject(ReservationService);
  reservation: Reservation | null = null;
  isLoading = true;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.reservationService.getReservationById(+id).subscribe(data => {
        this.reservation = data;
        this.isLoading = false;
      });
    }
  }
}