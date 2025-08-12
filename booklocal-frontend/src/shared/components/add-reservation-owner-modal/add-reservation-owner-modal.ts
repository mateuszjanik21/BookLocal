import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

import { ReservationService } from '../../../core/services/reservation';
import { BusinessService } from '../../../core/services/business-service';
import { OwnerCreateReservationPayload } from '../../../types/reservation.model';
import { Service, Employee } from '../../../types/business.model';

@Component({
  selector: 'app-add-reservation-owner-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './add-reservation-owner-modal.html',
})
export class AddReservationOwnerModalComponent implements OnInit {
  @Input() initialDetails!: { date: Date, employee?: Employee };
  @Output() closed = new EventEmitter<boolean>();

  private fb = inject(FormBuilder);
  private reservationService = inject(ReservationService);
  private businessService = inject(BusinessService);
  private toastr = inject(ToastrService);

  reservationForm!: FormGroup;
  services: Service[] = [];
  employees: Employee[] = [];
  isLoading = true;
  isSaving = false;

  ngOnInit(): void {
    this.reservationForm = this.fb.group({
      serviceId: ['', Validators.required],
      employeeId: ['', Validators.required],
      guestName: ['', Validators.required],
      guestPhoneNumber: [''],
      date: ['', Validators.required],
      startTime: ['', Validators.required],
    });

    this.businessService.getMyBusiness().subscribe(business => {
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

  onSubmit(): void {
    if (this.reservationForm.invalid) {
      this.toastr.error('Wypełnij wszystkie wymagane pola.');
      return;
    }

    this.isSaving = true;

    const formValue = this.reservationForm.value;
    const [year, month, day] = formValue.date.split('-').map(Number);
    const [hour, minute] = formValue.startTime.split(':').map(Number);
    
    const finalDate = new Date(year, month - 1, day, hour, minute);

    const payload: OwnerCreateReservationPayload = {
      serviceId: +formValue.serviceId,
      employeeId: +formValue.employeeId,
      startTime: finalDate.toISOString(),
      guestName: formValue.guestName,
      guestPhoneNumber: formValue.guestPhoneNumber
    };

    this.reservationService.createReservationAsOwner(payload).subscribe({
      next: () => {
        this.toastr.success('Rezerwacja została pomyślnie dodana do kalendarza!');
        this.isSaving = false;
        this.closed.emit(true);
      },
      error: (err) => {
        this.toastr.error(err.error?.title || 'Wystąpił błąd podczas zapisu rezerwacji.');
        this.isSaving = false;
      }
    });
  }
}