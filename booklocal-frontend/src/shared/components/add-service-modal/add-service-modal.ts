import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ServiceService } from '../../../core/services/service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-add-service-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './add-service-modal.html',
})
export class AddServiceModalComponent {
  @Input() businessId: number | null = null;
  @Output() closed = new EventEmitter<void>();

  private fb = inject(FormBuilder);
  private serviceService = inject(ServiceService);
  private toastr = inject(ToastrService);

  serviceForm = this.fb.group({
    name: ['', Validators.required],
    description: [''],
    price: [0, [Validators.required, Validators.min(0)]],
    durationMinutes: [30, [Validators.required, Validators.min(1)]]
  });

  onSubmit() {
    if (this.serviceForm.invalid || !this.businessId) return;

    this.serviceService.addService(this.businessId, this.serviceForm.value as any)
      .subscribe({
        next: () => {
          this.toastr.success('Usługa dodana pomyślnie!');
          this.closed.emit();
        },
        error: (err) => this.toastr.error('Wystąpił błąd podczas dodawania usługi.')
      });
  }
}