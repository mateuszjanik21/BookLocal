import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

import { ReservationService } from '../../../core/services/reservation';
import { BusinessService } from '../../../core/services/business-service';
import { OwnerCreateReservationPayload } from '../../../types/reservation.model';
import { Service, Employee } from '../../../types/business.model';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { CustomerService } from '../../../core/services/customer-service';
import { LoyaltyService } from '../../../core/services/loyalty-service';
import { CustomerListItem } from '../../../types/customer.models';

@Component({
  selector: 'app-add-reservation-owner-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './add-reservation-owner-modal.html',
})
export class AddReservationOwnerModalComponent implements OnInit {
  @Input() initialDetails!: { date: Date, employee?: Employee };
  @Output() closed = new EventEmitter<boolean>();

  private fb = inject(FormBuilder);
  private reservationService = inject(ReservationService);
  private businessService = inject(BusinessService);
  private toastr = inject(ToastrService);
  private customerService = inject(CustomerService);
  private loyaltyService = inject(LoyaltyService);

  reservationForm!: FormGroup;
  services: Service[] = [];
  employees: Employee[] = [];
  isLoading = true;
  isSaving = false;

  private customerSearchSubject = new Subject<string>();
  searchResults: CustomerListItem[] = [];
  selectedCustomer: CustomerListItem | null = null;
  businessId!: number;

  loyaltyPointsBalance = 0;
  loyaltyPointsToUse = 0;
  isSearchingCustomers = false;

  ngOnInit(): void {
    this.reservationForm = this.fb.group({
      serviceId: ['', Validators.required],
      employeeId: ['', Validators.required],
      guestName: ['', Validators.required],
      guestPhoneNumber: [''],
      date: ['', Validators.required],
      startTime: ['', Validators.required],
    });

    this.customerSearchSubject.pipe(
       debounceTime(300),
       distinctUntilChanged()
    ).subscribe(term => {
       if (term.length > 2) {
          this.isSearchingCustomers = true;
          this.customerService.getCustomers(this.businessId, term).subscribe(res => {
              this.searchResults = res.items;
              this.isSearchingCustomers = false;
          });
       } else {
          this.searchResults = [];
       }
    });

    this.businessService.getMyBusiness().subscribe(business => {
      this.businessId = business.id;
      this.services = business.categories.flatMap(c => c.services);
      this.employees = business.employees;

      const initialTime = this.initialDetails.date.toLocaleTimeString('pl-PL', { hour: '2-digit', minute: '2-digit' });
      
      this.reservationForm.patchValue({
        date: this.toISODateString(this.initialDetails.date),
        startTime: initialTime,
        employeeId: this.initialDetails.employee?.id.toString() || ''
      });

      this.isLoading = false;
    });
  }

  private toISODateString(date: Date): string {
    return new Date(date.getTime() - (date.getTimezoneOffset() * 60000))
      .toISOString()
      .split('T')[0];
  }

  onSearchChange(event: any) {
    this.selectedCustomer = null;
    this.loyaltyPointsBalance = 0;
    this.loyaltyPointsToUse = 0;
    this.customerSearchSubject.next(event.target.value);
  }

  selectCustomer(customer: CustomerListItem) {
      this.selectedCustomer = customer;
      this.searchResults = [];
      this.reservationForm.patchValue({
          guestName: customer.fullName,
          guestPhoneNumber: customer.phoneNumber || ''
      });
      
      this.loyaltyService.getCustomerLoyalty(this.businessId, customer.userId).subscribe(res => {
         this.loyaltyPointsBalance = res.balance.pointsBalance;
      });
  }
  
  get filteredServices(): Service[] {
    const employeeId = this.reservationForm.get('employeeId')?.value;
    if (!employeeId) return this.services;

    const selectedEmployee = this.employees.find(e => e.id.toString() === employeeId);
    if (!selectedEmployee || !selectedEmployee.assignedServiceIds) return this.services;

    return this.services.filter(s => selectedEmployee.assignedServiceIds?.includes(s.id));
  }

  get currentPrice(): number {
      const serviceId = Number(this.reservationForm.get('serviceId')?.value);
      const selectedService = this.services.find(s => s.id === serviceId);
      const basePrice = selectedService?.variants?.[0]?.price ?? 0;
      return Math.max(basePrice - this.loyaltyPointsToUse, 0);
  }

  get maxLoyaltyPoints(): number {
      const serviceId = Number(this.reservationForm.get('serviceId')?.value);
      const selectedService = this.services.find(s => s.id === serviceId);
      const basePrice = selectedService?.variants?.[0]?.price ?? 0;
      return Math.min(this.loyaltyPointsBalance, Math.max(Math.floor(basePrice - 1), 0));
  }

  onLoyaltyPointsChange(): void {
      if (this.loyaltyPointsToUse < 0) this.loyaltyPointsToUse = 0;
      if (this.loyaltyPointsToUse > this.maxLoyaltyPoints) {
          this.loyaltyPointsToUse = this.maxLoyaltyPoints;
      }
  }

  onSubmit(): void {
    if (this.reservationForm.invalid) {
      this.toastr.error('Wypełnij wszystkie wymagane pola.');
      this.reservationForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    const formValue = this.reservationForm.value;

    const serviceId = Number(formValue.serviceId);
    const selectedService = this.services.find(s => s.id === serviceId);  

    if (!selectedService || !selectedService.variants || selectedService.variants.length === 0) {
      this.toastr.error('Błąd: Wybrana usługa nie jest poprawna lub nie posiada wariantów cenowych.');
      this.isSaving = false;
      return;
    }

    const [year, month, day] = formValue.date.split('-').map(Number);
    const [hour, minute] = formValue.startTime.split(':').map(Number);
    const finalDate = new Date(year, month - 1, day, hour, minute);

    const payload: OwnerCreateReservationPayload = {
      serviceVariantId: selectedService.variants[0].serviceVariantId,
      employeeId: Number(formValue.employeeId),
      startTime: finalDate.toISOString(),
      guestName: formValue.guestName,
      guestPhoneNumber: formValue.guestPhoneNumber,
      customerId: this.selectedCustomer?.userId,
      loyaltyPointsUsed: this.loyaltyPointsToUse > 0 ? this.loyaltyPointsToUse : undefined,
      paymentMethod: 'Cash'
    };

    this.reservationService.createReservationAsOwner(payload).subscribe({
      next: () => {
        this.toastr.success('Rezerwacja została pomyślnie dodana do kalendarza!');
        this.isSaving = false;
        this.closed.emit(true);
      },
      error: (err) => {
        console.error(err);
        this.toastr.error(err.error?.title || 'Wystąpił błąd podczas zapisu rezerwacji.');
        this.isSaving = false;
      }
    });
  }
}