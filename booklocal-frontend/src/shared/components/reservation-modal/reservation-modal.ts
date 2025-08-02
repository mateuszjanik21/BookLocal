import { Component, ElementRef, Input, ViewChild, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Service, Employee } from '../../../types/business.model';
import { ReservationService } from '../../../core/services/reservation';

@Component({
  selector: 'app-reservation-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './reservation-modal.html',
})
export class ReservationModalComponent {
  @ViewChild('dialog') dialog!: ElementRef<HTMLDialogElement>;

  public showModal(): void {
    this.dialog.nativeElement.showModal();
  }

  @Input() service: Service | null = null;
  @Input() employees: Employee[] = [];

  private fb = inject(FormBuilder);
  private reservationService = inject(ReservationService); 

  reservationForm: FormGroup = this.fb.group({
    employeeId: [null, Validators.required],
    startTime: ['', Validators.required]
  });

  onSubmit(): void {
    if (this.reservationForm.invalid || !this.service) return;

    const payload = {
      serviceId: this.service.id,
      employeeId: this.reservationForm.value.employeeId,
      startTime: this.reservationForm.value.startTime
    };

    this.reservationService.createReservation(payload).subscribe({
      next: () => {
        alert('Rezerwacja pomyślna!');
        this.dialog.nativeElement.close();
      },
      error: (err) => {
        console.error(err);
        alert(`Błąd rezerwacji: ${err.error.title || 'Sprawdź wprowadzone dane.'}`);
      }
    });
  }
}