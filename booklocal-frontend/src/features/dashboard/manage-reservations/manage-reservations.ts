import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReservationService } from '../../../core/services/reservation';
import { Reservation } from '../../../types/reservation.model';
import { ChatService } from '../../../core/services/chat';
import { Router } from '@angular/router';

@Component({
  selector: 'app-manage-reservations',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './manage-reservations.html',
})
export class ManageReservationsComponent implements OnInit {
  private reservationService = inject(ReservationService);
  private chatService = inject(ChatService);
  private router = inject(Router);

  reservations: Reservation[] = [];
  isLoading = true;

  ngOnInit(): void {
    this.reservationService.getMyReservations().subscribe({
      next: (data) => {
        this.reservations = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error("Błąd podczas pobierania rezerwacji:", err);
        this.isLoading = false;
      }
    });
  }

  startChat(customerId: string) {
    this.chatService.startConversationAsOwner(customerId).subscribe(response => {
      this.router.navigate(['/dashboard/chat']);
    });
  }
}