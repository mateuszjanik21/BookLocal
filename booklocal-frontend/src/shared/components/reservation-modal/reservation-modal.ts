import { Component, ElementRef, Input, OnChanges, SimpleChanges, ViewChild, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Service, Employee, BusinessDetail, ServiceCategory } from '../../../types/business.model';
import { ReservationService } from '../../../core/services/reservation';
import { DiscountService, VerifyDiscountResult } from '../../../core/services/discount-service';
import { LoyaltyService } from '../../../core/services/loyalty-service';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth-service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-reservation-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DatePipe, FormsModule],
  templateUrl: './reservation-modal.html',
})
export class ReservationModalComponent implements OnChanges {
  @ViewChild('reservationDialog') dialog!: ElementRef<HTMLDialogElement>;
  @Input() service: Service | null = null;
  @Input() employees: Employee[] = [];
  @Input() business: BusinessDetail | null = null;
  @Input() businessId: number | null = null;
  
  private reservationService = inject(ReservationService);
  private discountService = inject(DiscountService);
  private loyaltyService = inject(LoyaltyService);
  private toastr = inject(ToastrService);
  private fb = inject(FormBuilder);
  private router = inject(Router); 
  private authService = inject(AuthService);

  currentStep: number = 1;
  availableSlots: string[] = [];
  timeGroups: { [key: string]: string[] } = {
    'Rano': [],
    'Południe': [],
    'Popołudnie': [],
    'Wieczór': []
  };
  activeGroup: string = 'Rano';
  isLoadingSlots = false;
  isReserving = false;
  minDate: string;

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

  isProcessingPayment = false;
  paymentStatus: 'idle' | 'processing' | 'success' | 'failed' = 'idle';
  isServicePreselected = false;

  loyaltyPointsBalance = 0;
  loyaltyPointsToUse = 0;

  constructor() {
    const today = new Date();
    this.minDate = today.toISOString().split('T')[0];
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['service']) {
      this.resetModal();
      if (this.service) {
        this.isServicePreselected = true;
        if (this.employees.length === 1) {
          this.reservationForm.get('employeeId')?.setValue(this.employees[0].id.toString());
          this.currentStep = 3;
          this.onDateChange();
        }
      }
    }
  }

  onDateChange(): void {
    const { employeeId, date } = this.reservationForm.value;
    if (!employeeId || !date || !this.service) return;

    this.isLoadingSlots = true;
    this.reservationForm.get('startTime')?.reset();
    
    this.reservationService.getAvailableSlots(+employeeId, this.service.variants?.[0]?.serviceVariantId ?? 0, date).subscribe({
      next: (slots) => {
        this.availableSlots = slots;
        this.groupSlots(slots);
        this.isLoadingSlots = false;
      },
      error: () => {
        this.toastr.error('Nie udało się pobrać dostępnych terminów.');
        this.isLoadingSlots = false;
      }
    });
  }

  groupSlots(slots: string[]): void {
    this.timeGroups = { 'Rano': [], 'Południe': [], 'Popołudnie': [], 'Wieczór': [] };
    
    slots.forEach(slot => {
      const date = new Date(slot);
      const hour = date.getHours();
      
      if (hour >= 6 && hour < 11) this.timeGroups['Rano'].push(slot);
      else if (hour >= 11 && hour < 15) this.timeGroups['Południe'].push(slot);
      else if (hour >= 15 && hour < 18) this.timeGroups['Popołudnie'].push(slot);
      else if (hour >= 18) this.timeGroups['Wieczór'].push(slot);
    });

    const groupsWithSlots = Object.keys(this.timeGroups).filter(key => this.timeGroups[key].length > 0);
    if (groupsWithSlots.length > 0 && this.timeGroups[this.activeGroup].length === 0) {
      this.activeGroup = groupsWithSlots[0];
    }
  }

  selectGroup(group: string): void {
    this.activeGroup = group;
  }

  selectService(service: Service, variant: any): void {
    this.service = {
       ...service,
       variants: [variant]
    };
    this.onDateChange();
    this.nextStep();
  }

  verifyDiscount() {
      const code = this.reservationForm.get('discountCode')?.value;
      if (!code || !this.service) return;

      this.isVerifyingDiscount = true;
      this.discountError = null;
      this.verifiedDiscount = null;
      
      const price = this.service.variants?.[0]?.price ?? 0;
      const serviceId = this.service.id; 
      const currentUser: any = this.authService.currentUserValue;
      
      const businessId = this.service.businessId || this.business?.id;
      if (!businessId) {
          this.toastr.error('Błąd: Nie znaleziono identyfikatora firmy.');
          return;
      }

      this.discountService.verifyDiscount(businessId, {
          code,
          serviceId: this.service.id,
          customerId: currentUser ? currentUser.id : undefined,
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

    if (paymentMethod === 'Online' && this.paymentStatus !== 'success' && this.currentPrice > 0) {
       this.processOnlinePayment();
       return;
    }

    this.finalizeReservation();
  }

  processOnlinePayment() {
      this.isProcessingPayment = true;
      this.paymentStatus = 'processing';
      
      setTimeout(() => {
          this.isProcessingPayment = false;
          this.paymentStatus = 'success';
          this.toastr.success('Płatność zakończona sukcesem!');
          
          setTimeout(() => this.finalizeReservation(), 1000);
      }, 2000);
  }

  finalizeReservation() {
    if (this.isReserving) return;
    this.isReserving = true;
    const { employeeId, startTime, discountCode, paymentMethod } = this.reservationForm.value;

    const payload = {
      serviceVariantId: this.service!.variants?.[0]?.serviceVariantId ?? 0,
      employeeId: +employeeId!,
      startTime: startTime!,
      discountCode: (this.verifiedDiscount && discountCode) ? discountCode : undefined,
      paymentMethod: paymentMethod!,
      loyaltyPointsUsed: this.loyaltyPointsToUse > 0 ? this.loyaltyPointsToUse : undefined
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
        if (paymentMethod === 'Online') this.paymentStatus = 'idle'; 
      }
    });
  }

  nextStep(): void {
    if (this.currentStep === 1 && this.service) {
      this.currentStep = 3;
      this.onDateChange();
      return;
    }
    
    if (this.currentStep < 4) {
      this.currentStep++;
      if (this.currentStep === 3) {
        this.onDateChange();
      }
    }
  }

  prevStep(): void {
    if (this.currentStep === 3 && this.isServicePreselected) {
      this.currentStep = 1;
      return;
    }
    if (this.currentStep > 1) this.currentStep--;
  }

  resetModal(): void {
    this.currentStep = 1;
    this.isServicePreselected = !!this.service;
    this.reservationForm.reset();
    this.reservationForm.get('paymentMethod')?.setValue('Cash');
    this.availableSlots = [];
    this.verifiedDiscount = null;
    this.discountError = null;
    this.loyaltyPointsToUse = 0;
  }
  
  showModal(): void {
    this.loadLoyaltyBalance();
    this.dialog.nativeElement.showModal();
  }

  private _showModalBusinessId: number | undefined;

  get selectedEmployee(): Employee | undefined {
    const employeeId = this.reservationForm.get('employeeId')?.value;
    return this.employees.find(e => e.id.toString() === employeeId);
  }

  get filteredCategories(): ServiceCategory[] {
    if (!this.business?.categories) return [];
    const employeeId = this.reservationForm.get('employeeId')?.value;
    if (!employeeId) return this.business.categories;

    const selectedEmployee = this.selectedEmployee;
    if (!selectedEmployee || !selectedEmployee.assignedServiceIds) return this.business.categories;

    return this.business.categories.map(cat => ({
      ...cat,
      services: cat.services.filter(s => selectedEmployee.assignedServiceIds?.includes(s.id))
    })).filter(cat => cat.services.length > 0);
  }

  closeModal(): void {
    this.dialog.nativeElement.close();
  }

  get currentPrice(): number {
      const basePrice = this.service?.variants?.[0]?.price ?? 0;
      let price = basePrice;
      if (this.verifiedDiscount) {
          price = this.verifiedDiscount.finalPrice;
      }
      price -= this.loyaltyPointsToUse;
      return Math.max(price, 0);
  }

  get maxLoyaltyPoints(): number {
      const basePrice = this.verifiedDiscount ? this.verifiedDiscount.finalPrice : (this.service?.variants?.[0]?.price ?? 0);
      return Math.min(this.loyaltyPointsBalance, Math.max(Math.floor(basePrice - 1), 0));
  }

  onLoyaltyPointsChange(): void {
      if (this.loyaltyPointsToUse < 0) this.loyaltyPointsToUse = 0;
      if (this.loyaltyPointsToUse > this.maxLoyaltyPoints) {
          this.loyaltyPointsToUse = this.maxLoyaltyPoints;
      }
  }

  private loadLoyaltyBalance(): void {
      const currentUser: any = this.authService.currentUserValue;
      const businessId = this._showModalBusinessId ?? this.businessId ?? this.service?.businessId ?? this.business?.id;
      if (!currentUser?.id || !businessId) {
          return;
      }
      this.loyaltyService.getCustomerLoyalty(businessId, currentUser.id).subscribe({
          next: (data) => {
              this.loyaltyPointsBalance = data.balance.pointsBalance;
          },
          error: (err) => {
              console.warn('[Loyalty] Błąd ładowania punktów:', err);
              this.loyaltyPointsBalance = 0;
          }
      });
  }
}