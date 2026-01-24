import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ServiceBundle } from '../../../../types/service-bundle.model';
import { Employee } from '../../../../types/business.model';
import { AvailabilityService } from '../../../../core/services/availability';
import { EmployeeService } from '../../../../core/services/employee-service';
import { ReservationService } from '../../../../core/services/reservation';
import { ToastrService } from 'ngx-toastr';
import { switchMap } from 'rxjs';

@Component({
  selector: 'app-book-bundle-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './book-bundle-modal.html'
})
export class BookBundleModalComponent implements OnInit {
  @Input() bundle: ServiceBundle | null = null;
  @Input() businessId: number | null = null;
  @Output() close = new EventEmitter<void>();

  private availabilityService = inject(AvailabilityService);
  private employeeService = inject(EmployeeService); // We need this to list employees
  private toastr = inject(ToastrService);

  currentStep = 1;
  employees: Employee[] = [];
  selectedEmployee: Employee | null = null;
  
  selectedDate: Date = new Date();
  availableSlots: string[] = [];
  selectedSlot: string | null = null; // Formatting ISO string or just time? Backend returns DateTime ISO

  isLoadingSlots = false;
  isSaving = false;
  @Input() isOwnerMode = true; // Default to owner mode for backward compat, or check usages? 
  // Wait, existing usage in OwnerLayout does not pass isOwnerMode, so default true is safer? 
  // Actually, I should check who uses it. OwnerDashboard uses it.
  
  // Let's set default to true for now, but explicit is better. I'll check usages later.
  
  guestName = '';
  guestPhone = '';

  private reservationService = inject(ReservationService);

  ngOnInit() {
    if (this.businessId) {
        this.employeeService.getEmployees(this.businessId).subscribe(emps => this.employees = emps);
    }
  }

  selectEmployee(emp: Employee) {
    this.selectedEmployee = emp;
  }

  onDateChange(dateStr: string) {
      this.selectedDate = new Date(dateStr);
      this.fetchSlots();
  }

  fetchSlots() {
      if (!this.selectedEmployee || !this.bundle) return;
      
      this.isLoadingSlots = true;
      this.selectedSlot = null;
      
      const dateIso = this.selectedDate.toISOString(); 

      this.availabilityService.getBundleAvailableSlots(
          this.selectedEmployee.id, 
          dateIso, 
          this.bundle.serviceBundleId
      ).subscribe({
          next: slots => {
              this.availableSlots = slots;
              this.isLoadingSlots = false;
          },
          error: () => {
              this.toastr.error('Błąd pobierania terminów.');
              this.isLoadingSlots = false;
          }
      });
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
      if (this.currentStep === 1) {
          this.fetchSlots();
      }
      this.currentStep++;
  }

  confirmBooking() {
      if (!this.selectedSlot || !this.selectedEmployee || !this.bundle) {return;}
      
      if (this.isOwnerMode && !this.guestName) {
         this.toastr.warning('Uzupełnij wymagane dane (Imię i Nazwisko).');
         return;
      }

      this.isSaving = true;

      if (this.isOwnerMode) {
          this.reservationService.createBundleReservationAsOwner({
            serviceBundleId: this.bundle.serviceBundleId,
            employeeId: this.selectedEmployee.id,
            startTime: this.selectedSlot,
            guestName: this.guestName,
            guestPhoneNumber: this.guestPhone,
            paymentMethod: 'Cash'
          }).subscribe({
            next: () => this.handleSuccess(),
            error: (err) => this.handleError(err)
          });
      } else {
          // Customer Mode
          this.reservationService.createBundleReservation({
            serviceBundleId: this.bundle.serviceBundleId,
            employeeId: this.selectedEmployee.id,
            startTime: this.selectedSlot,
            paymentMethod: 'Cash' // For now default or add selector? 
          }).subscribe({
             next: () => this.handleSuccess(),
             error: (err) => this.handleError(err)
          });
      }
  }

  private handleSuccess() {
      this.toastr.success('Pakiet został pomyślnie zarezerwowany.');
      this.isSaving = false;
      this.close.emit();
  }

  private handleError(err: any) {
      console.error(err);
      this.toastr.error('Wystąpił błąd podczas rezerwacji pakietu.');
      this.isSaving = false;
  }
}
