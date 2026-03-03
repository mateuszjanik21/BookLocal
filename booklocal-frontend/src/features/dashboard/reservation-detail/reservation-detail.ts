import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ReservationService } from '../../../core/services/reservation';
import { Reservation } from '../../../types/reservation.model';
import { ChatService } from '../../../core/services/chat'; 
import { ReservationStatusPipe } from '../../../shared/pipes/reservation-status.pipe';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs';

import { PaymentDto, PaymentService } from '../../../core/services/payment-service';
import { InvoiceService } from '../../../core/services/invoice-service';
import { CustomerDetailsModalComponent } from '../../../shared/components/customer-details-modal/customer-details-modal';

@Component({
  selector: 'app-reservation-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, ReservationStatusPipe, FormsModule, CustomerDetailsModalComponent],
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

  isPaymentModalOpen = false;
  selectedPaymentMethod = 'Cash';
  paymentAmountInput = 0;

  isCustomerModalOpen = false;

  openCustomerModal() {
      this.isCustomerModalOpen = true;
  }

  closeCustomerModal() {
      this.isCustomerModalOpen = false;
  }

  get remainingAmountToPay(): number {
      if(!this.reservation) return 0;
      const rem = this.reservation.agreedPrice - this.totalPayments;
      return rem > 0 ? rem : 0;
  }

  openPaymentModal(): void {
      if(!this.reservation) return;
      this.selectedPaymentMethod = 'Card';
      this.paymentAmountInput = this.remainingAmountToPay;
      this.isPaymentModalOpen = true;
  }

  closePaymentModal(): void {
      this.isPaymentModalOpen = false;
      this.paymentAmountInput = 0;
  }

  onPaymentAmountChange(amount: number) {
      this.paymentAmountInput = amount;
  }

  confirmManualPayment(): void {
      if (!this.reservation || this.paymentAmountInput <= 0) return;
      
      this.isAddingPayment = true;
      this.paymentService.createPayment({
          reservationId: this.reservation.reservationId,
          method: this.selectedPaymentMethod,
          amount: this.paymentAmountInput
      }).pipe(finalize(() => this.isAddingPayment = false))
      .subscribe({
          next: () => {
              this.toastr.success('Płatność została dodana.');
              this.loadPayments(this.reservation!.reservationId);
              this.closePaymentModal();
          },
          error: () => {
              this.toastr.error('Błąd dodawania płatności.');
          }
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