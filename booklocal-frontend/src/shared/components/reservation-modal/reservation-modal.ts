import { Component, ElementRef, Input, OnChanges, SimpleChanges, ViewChild, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Service, Employee } from '../../../types/business.model';
import { ReservationService } from '../../../core/services/reservation';
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
  private toastr = inject(ToastrService);
  private fb = inject(FormBuilder);
  private router = inject(Router); 

  currentStep = 1;
  availableSlots: string[] = [];
  isLoadingSlots = false;
  isReserving = false;
  minDate: string;

  reservationForm = this.fb.group({
    employeeId: ['', Validators.required],
    date: ['', Validators.required],
    startTime: ['', Validators.required],
  });

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

  onSubmit(): void {
    if (this.reservationForm.invalid || !this.service) return;
    
    this.isReserving = true;
    const { employeeId, startTime } = this.reservationForm.value;

    const payload = {
      serviceId: this.service.id,
      employeeId: +employeeId!,
      startTime: startTime!
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
      }
    });
  }

  nextStep(): void {
    if (this.currentStep < 3) this.currentStep++;
  }

  prevStep(): void {
    if (this.currentStep > 1) this.currentStep--;
  }

  resetModal(): void {
    this.currentStep = 1;
    this.reservationForm.reset();
    this.availableSlots = [];
  }
  
  showModal(): void {
    this.dialog.nativeElement.showModal();
  }

  closeModal(): void {
    this.dialog.nativeElement.close();
  }
}