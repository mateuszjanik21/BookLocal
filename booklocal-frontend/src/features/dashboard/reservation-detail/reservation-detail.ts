import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ReservationService } from '../../../core/services/reservation';
import { Reservation } from '../../../types/reservation.model';
import { ChatService } from '../../../core/services/chat'; 
import { ReservationStatusPipe } from '../../../shared/pipes/reservation-status.pipe';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs';

import { PaymentDto, PaymentService } from '../../../core/services/payment-service';
import { InvoiceService } from '../../../core/services/invoice-service';

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
  private invoiceService = inject(InvoiceService);
  private paymentService = inject(PaymentService);
  private toastr = inject(ToastrService);
  
  reservation: Reservation | null = null;
  payments: PaymentDto[] = [];
  isLoading = true;
  isUpdating = false;
  isGeneratingInvoice = false;
  isAddingPayment = false;

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
        this.loadPayments(id);
      },
      error: () => {
        this.toastr.error('Nie udało się załadować danych rezerwacji.');
        this.router.navigate(['/dashboard/reservations']);
      }
    });
  }

  loadPayments(reservationId: number): void {
      this.paymentService.getReservationPayments(reservationId).subscribe({
          next: (data) => this.payments = data
      });
  }

  addManualPayment(method: string): void {
      if (!this.reservation) return;
      
      const reservationId = this.reservation.reservationId;
      const totalPaid = this.totalPayments;
      const agreedPrice = this.reservation.agreedPrice;
      let remainingAmount = agreedPrice - totalPaid;

      // Ensure we don't show negative remaining amount (unless it's a refund scenario, but let's keep it simple)
      if (remainingAmount < 0) remainingAmount = 0;

      let defaultAmount = remainingAmount > 0 
          ? remainingAmount.toFixed(2).replace('.', ',') 
          : '0,00';

      const amountStr = prompt(
          `Podaj kwotę wpłaty (${method}):\nPozostało do zapłaty: ${remainingAmount.toFixed(2)} PLN`, 
          defaultAmount
      );
      
      if (!amountStr) return;
      
      const amount = parseFloat(amountStr.replace(',', '.'));
      if (isNaN(amount) || amount <= 0) {
        this.toastr.warning('Podano nieprawidłową kwotę.');
        return;
      }

      // Optional: Warning if overpaying
      if (totalPaid + amount > agreedPrice) {
          if (!confirm(`Kwota przewyższa ustaloną cenę (${agreedPrice} PLN). Czy na pewno chcesz dodać nadpłatę/napiwek?`)) {
              return;
          }
      }

      this.isAddingPayment = true;
      this.paymentService.createPayment({
          reservationId: reservationId,
          method: method,
          amount: amount
      }).pipe(finalize(() => this.isAddingPayment = false))
      .subscribe({
          next: () => {
              this.toastr.success('Płatność została dodana.');
              this.loadPayments(reservationId);
          },
          error: () => this.toastr.error('Błąd dodawania płatności.')
      });
  }

  startChat(customerId?: string): void {
    if (!customerId) return;
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

  generateInvoice(): void {
    if (!this.reservation || !this.reservation.businessId) return;

    if (!confirm('Czy na pewno chcesz wystawić fakturę dla tej rezerwacji?')) return;

    this.isGeneratingInvoice = true;
    this.invoiceService.generateInvoice(this.reservation.businessId, this.reservation.reservationId).subscribe({
        next: (invoice) => {
            this.toastr.success(`Faktura ${invoice.invoiceNumber} została wystawiona.`);
            this.isGeneratingInvoice = false;
            this.router.navigate(['/dashboard/invoices']);
        },
        error: (err) => {
           this.toastr.error(err.error || 'Nie udało się wystawić faktury.');
           this.isGeneratingInvoice = false;
        }
    });
  }

  get totalPayments(): number {
    return this.payments.reduce((sum, p) => sum + p.amount, 0);
  }
}