import { Component, ElementRef, Input, OnChanges, SimpleChanges, ViewChild, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Service, Employee } from '../../../types/business.model';
import { ReservationService } from '../../../core/services/reservation';
import { DiscountService, VerifyDiscountResult } from '../../../core/services/discount-service';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';

@Component({
  selector: 'app-reservation-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DatePipe],
  templateUrl: './reservation-modal.html',
})
export class ReservationModalComponent implements OnChanges {
  @ViewChild('reservationDialog') dialog!: ElementRef<HTMLDialogElement>;
  @Input() service: Service | null = null;
  @Input() employees: Employee[] = [];
  
  private reservationService = inject(ReservationService);
  private discountService = inject(DiscountService);
  private toastr = inject(ToastrService);
  private fb = inject(FormBuilder);
  private router = inject(Router); 

  currentStep = 1;
  availableSlots: string[] = [];
  isLoadingSlots = false;
  isReserving = false;
  minDate: string;

  // Discount logic
  isVerifyingDiscount = false;
  verifiedDiscount: VerifyDiscountResult | null = null;
  discountError: string | null = null;

  reservationForm = this.fb.group({
    employeeId: ['', Validators.required],
    date: ['', Validators.required],
    startTime: ['', Validators.required],
    discountCode: [''],
    paymentMethod: ['Cash', Validators.required]
  });

  // Payment Mock
  isProcessingPayment = false;
  paymentStatus: 'idle' | 'processing' | 'success' | 'failed' = 'idle';

  constructor() {
    const today = new Date();
    this.minDate = today.toISOString().split('T')[0];
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['service'] && this.service) {
      this.resetModal();
      if (this.employees.length === 1) {
        this.reservationForm.get('employeeId')?.setValue(this.employees[0].id.toString());
        this.currentStep = 2;
      }
    }
  }

  onDateChange(): void {
    const { employeeId, date } = this.reservationForm.value;
    if (!employeeId || !date || !this.service) return;

    this.isLoadingSlots = true;
    this.reservationForm.get('startTime')?.reset();
    
    this.reservationService.getAvailableSlots(+employeeId, this.service.id, date).subscribe({
      next: (slots) => {
        this.availableSlots = slots;
        this.isLoadingSlots = false;
      },
      error: () => {
        this.toastr.error('Nie udało się pobrać dostępnych terminów.');
        this.isLoadingSlots = false;
      }
    });
  }

  verifyDiscount() {
      const code = this.reservationForm.get('discountCode')?.value;
      if (!code || !this.service) return;

      this.isVerifyingDiscount = true;
      this.discountError = null;
      this.verifiedDiscount = null;
      
      const price = this.service.variants?.[0]?.price ?? 0;
      const serviceId = this.service.id; 
      
      this.discountService.verifyDiscount(this.service.businessId, {
          code,
          serviceId: this.service.id,
          originalPrice: price
      }).subscribe({
          next: (res) => {
              this.isVerifyingDiscount = false;
              if (res.isValid) {
                  this.verifiedDiscount = res;
                  this.toastr.success('Kod rabatowy aktywny!');
              } else {
                  this.discountError = res.message || 'Kod nieprawidłowy.';
                  this.toastr.warning(this.discountError!);
              }
          },
          error: () => {
              this.isVerifyingDiscount = false;
              this.discountError = 'Błąd weryfikacji kodu.';
          }
      });
  }

  removeDiscount() {
      this.verifiedDiscount = null;
      this.discountError = null;
      this.reservationForm.get('discountCode')?.setValue('');
  }

  onSubmit(): void {
    if (this.reservationForm.invalid || !this.service) return;
    
    const { paymentMethod } = this.reservationForm.value;

    if (paymentMethod === 'Online' && this.paymentStatus !== 'success') {
       this.processOnlinePayment();
       return;
    }

    this.finalizeReservation();
  }

  processOnlinePayment() {
      this.isProcessingPayment = true;
      this.paymentStatus = 'processing';
      
      // MOCK Payment Gateway Delay
      setTimeout(() => {
          this.isProcessingPayment = false;
          this.paymentStatus = 'success';
          this.toastr.success('Płatność zakończona sukcesem!');
          
          setTimeout(() => this.finalizeReservation(), 1000);
      }, 2000);
  }

  finalizeReservation() {
    this.isReserving = true;
    const { employeeId, startTime, discountCode, paymentMethod } = this.reservationForm.value;

    const payload = {
      serviceVariantId: this.service!.variants?.[0]?.serviceVariantId ?? 0,
      employeeId: +employeeId!,
      startTime: startTime!,
      discountCode: (this.verifiedDiscount && discountCode) ? discountCode : undefined,
      paymentMethod: paymentMethod!
    };
    
    this.reservationService.createReservation(payload).subscribe({
      next: () => {
        this.toastr.success('Twoja wizyta została pomyślnie zarezerwowana!');
        this.isReserving = false;
        this.closeModal();
        this.router.navigate(['/my-reservations']);
      },
      error: (err) => {
        this.toastr.error(err.error.message || err.error || 'Wystąpił błąd podczas rezerwacji.');
        this.isReserving = false;
        // Reset payment status if error, to allow retry
        if (paymentMethod === 'Online') this.paymentStatus = 'idle'; 
      }
    });
  }

  nextStep(): void {
    if (this.currentStep < 4) this.currentStep++;
  }

  prevStep(): void {
    if (this.currentStep > 1) this.currentStep--;
  }

  resetModal(): void {
    this.currentStep = 1;
    this.reservationForm.reset();
    this.availableSlots = [];
    this.verifiedDiscount = null;
    this.discountError = null;
  }
  
  showModal(): void {
    this.dialog.nativeElement.showModal();
  }

  closeModal(): void {
    this.dialog.nativeElement.close();
  }

  get currentPrice(): number {
      const basePrice = this.service?.variants?.[0]?.price ?? 0;
      if (this.verifiedDiscount) {
          return this.verifiedDiscount.finalPrice;
      }
      return basePrice;
  }
}