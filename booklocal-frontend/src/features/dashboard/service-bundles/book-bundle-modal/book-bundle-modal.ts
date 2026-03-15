import { Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ServiceBundle } from '../../../../types/service-bundle.model';
import { Employee } from '../../../../types/business.model';
import { AvailabilityService } from '../../../../core/services/availability';
import { EmployeeService } from '../../../../core/services/employee-service';
import { ReservationService } from '../../../../core/services/reservation';
import { LoyaltyService } from '../../../../core/services/loyalty-service';
import { AuthService } from '../../../../core/services/auth-service';
import { ToastrService } from 'ngx-toastr';
import { switchMap } from 'rxjs';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-book-bundle-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe, RouterModule],
  templateUrl: './book-bundle-modal.html'
})
export class BookBundleModalComponent implements OnInit {
  @ViewChild('bundleDialog') dialog!: ElementRef<HTMLDialogElement>;
  @Input() bundle: ServiceBundle | null = null;
  @Input() businessId: number | null = null;
  @Output() close = new EventEmitter<void>();

  private availabilityService = inject(AvailabilityService);
  private employeeService = inject(EmployeeService);
  private toastr = inject(ToastrService);

  currentStep = 0;
  employees: Employee[] = [];
  selectedEmployee: Employee | null = null;
  
  availableSlots: string[] = [];
  timeGroups: { [key: string]: string[] } = {
    'Rano': [],
    'Południe': [],
    'Popołudnie': [],
    'Wieczór': []
  };
  activeGroup: string = 'Rano';
  selectedSlot: string | null = null;
  isLoadingSlots = false;
  isSaving = false;
  minDate: string;

  @Input() isOwnerMode = true;
  
  guestName = '';
  guestPhone = '';

  paymentMethod: 'Cash' | 'Online' = 'Cash';
  isProcessingPayment = false;
  paymentStatus: 'idle' | 'processing' | 'success' | 'failed' = 'idle';

  private reservationService = inject(ReservationService);
  private loyaltyService = inject(LoyaltyService);
  private authService = inject(AuthService);

  get isCustomer(): boolean {
    const user = this.authService.currentUserValue;
    return !!user && user.roles.includes('customer');
  }

  loyaltyPointsBalance = 0;
  loyaltyPointsToUse = 0;

  constructor() {
    const today = new Date();
    this.minDate = today.toISOString().split('T')[0];
  }

  ngOnInit() {
  }

  loadEmployees() {
    if (this.businessId) {
        this.employeeService.getEmployees(this.businessId).subscribe(emps => {
            this.employees = emps;
        });
    }
  }

  selectEmployee(emp: Employee) {
    this.selectedEmployee = emp;
  }

  onDateChange(dateStr: string) {
      if (!this.selectedEmployee || !this.bundle || !dateStr) return;
      
      this.isLoadingSlots = true;
      this.availableSlots = [];
      
      this.availabilityService.getBundleAvailableSlots(
          this.selectedEmployee.id, 
          dateStr, 
          this.bundle.serviceBundleId
      ).subscribe({
          next: slots => {
              this.availableSlots = slots;
              this.groupSlots(slots);
              this.isLoadingSlots = false;
          },
          error: () => {
              this.toastr.error('Błąd pobierania terminów.');
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

  selectSlot(slot: string) {
      this.selectedSlot = slot;
  }

  canProceed(): boolean {
      if (this.currentStep === 1) return !!this.selectedEmployee;
      if (this.currentStep === 2) return !!this.selectedSlot;
      return true;
  }

  nextStep() {
      this.currentStep++;
  }

  onSubmit() {
    if (this.paymentMethod === 'Online' && this.paymentStatus !== 'success') {
        this.processOnlinePayment();
        return;
    }
    this.confirmBooking();
  }

  processOnlinePayment() {
      this.isProcessingPayment = true;
      this.paymentStatus = 'processing';
      
      setTimeout(() => {
          this.isProcessingPayment = false;
          this.paymentStatus = 'success';
          this.toastr.success('Płatność zakończona sukcesem!');
          
          setTimeout(() => this.confirmBooking(), 1000);
      }, 2000);
  }

  confirmBooking() {
      if (!this.selectedSlot || !this.selectedEmployee || !this.bundle) {return;}
      
      if (this.isOwnerMode && !this.guestName) {
         this.toastr.warning('Uzupełnij wymagane dane (Imię i Nazwisko).');
         return;
      }

      this.isSaving = true;

      const basePayload = {
        serviceBundleId: this.bundle.serviceBundleId,
        employeeId: this.selectedEmployee.id,
        startTime: this.selectedSlot,
        paymentMethod: this.paymentMethod,
        loyaltyPointsUsed: this.loyaltyPointsToUse > 0 ? this.loyaltyPointsToUse : undefined
      };

      if (this.isOwnerMode) {
          this.reservationService.createBundleReservationAsOwner({
            ...basePayload,
            guestName: this.guestName,
            guestPhoneNumber: this.guestPhone
          }).subscribe({
            next: () => this.handleSuccess(),
            error: (err) => this.handleError(err)
          });
      } else {
          this.reservationService.createBundleReservation(basePayload).subscribe({
             next: () => this.handleSuccess(),
             error: (err) => this.handleError(err)
          });
      }
  }

  private router = inject(Router);

  private handleSuccess() {
      this.toastr.success('Pakiet został pomyślnie zarezerwowany.');
      this.isSaving = false;
      this.closeModal();
      this.router.navigate(['/my-reservations']);
  }

  private handleError(err: any) {
      console.error(err);
      this.toastr.error('Wystąpił błąd podczas rezerwacji pakietu.');
      this.isSaving = false;
  }

  showModal() {
      this.resetModal();
      this.loadEmployees();
      this.loadLoyaltyBalance();
      this.dialog.nativeElement.showModal();
  }

  closeModal() {
      this.dialog.nativeElement.close();
  }

  resetModal() {
      this.currentStep = 0;
      this.selectedEmployee = null;
      this.selectedSlot = null;
      this.availableSlots = [];
      this.timeGroups = { 'Rano': [], 'Południe': [], 'Popołudnie': [], 'Wieczór': [] };
      this.paymentStatus = 'idle';
      this.isProcessingPayment = false;
      this.isSaving = false;
      this.guestName = '';
      this.guestPhone = '';
      this.loyaltyPointsToUse = 0;
  }

  prevStep() {
      if (this.currentStep > 0) this.currentStep--;
  }

  get currentPrice(): number {
      const basePrice = this.bundle?.totalPrice ?? 0;
      return Math.max(basePrice - this.loyaltyPointsToUse, 1);
  }

  get maxLoyaltyPoints(): number {
      const basePrice = this.bundle?.totalPrice ?? 0;
      return Math.min(this.loyaltyPointsBalance, Math.max(Math.floor(basePrice - 1), 0));
  }

  onLoyaltyPointsChange(): void {
      if (this.loyaltyPointsToUse < 0) this.loyaltyPointsToUse = 0;
      if (this.loyaltyPointsToUse > this.maxLoyaltyPoints) {
          this.loyaltyPointsToUse = this.maxLoyaltyPoints;
      }
  }

  private loadLoyaltyBalance(): void {
      if (this.isOwnerMode) return;
      const currentUser: any = this.authService.currentUserValue;
      if (!currentUser?.id || !this.businessId) return;

      this.loyaltyService.getCustomerLoyalty(this.businessId, currentUser.id).subscribe({
          next: (data) => {
              this.loyaltyPointsBalance = data.balance.pointsBalance;
          },
          error: () => {
              this.loyaltyPointsBalance = 0;
          }
      });
  }
}
