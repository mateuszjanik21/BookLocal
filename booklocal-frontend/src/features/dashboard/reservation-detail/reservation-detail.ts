import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ReservationService } from '../../../core/services/reservation';
import { Reservation } from '../../../types/reservation.model';
import { ChatService } from '../../../core/services/chat'; 
import { ReservationStatusPipe } from '../../../shared/pipes/reservation-status.pipe';

@Component({
  selector: 'app-reservation-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, ReservationStatusPipe],
  templateUrl: './reservation-detail.html',
})
export class ReservationDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private reservationService = inject(ReservationService);
  private chatService = inject(ChatService);
  private router = inject(Router);
  
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

  startChat(customerId: string): void {
    this.chatService.startConversationAsOwner(customerId).subscribe(() => {
      this.router.navigate(['/dashboard/chat']);
    });
  }
}