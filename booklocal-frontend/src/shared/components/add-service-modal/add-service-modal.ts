import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ServiceService } from '../../../core/services/service';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-add-service-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './add-service-modal.html',
  styleUrl: './add-service-modal.css'
})
export class AddServiceModalComponent {
  @Input() businessId: number | null = null;
  @Input() categoryId: number | null = null;
  @Output() closed = new EventEmitter<boolean>();
  @Input() isVisible: boolean = false;
  private fb = inject(FormBuilder);
  private serviceService = inject(ServiceService);
  private toastr = inject(ToastrService);

  isSubmitting = false;
  serviceForm = this.fb.group({
    name: ['', Validators.required],
    description: [''],
    price: [null, [Validators.required, Validators.min(0)]],
    durationMinutes: [null, [Validators.required, Validators.min(1)]]
  });

  onSubmit() {
    if (this.serviceForm.invalid || !this.businessId || !this.categoryId) return;
    this.isSubmitting = true;
    
    const payload = {
      ...this.serviceForm.value,
      serviceCategoryId: this.categoryId
    }

    this.serviceService.addService(this.businessId, payload as any)
      .pipe(finalize(() => this.isSubmitting = false))
      .subscribe({
        next: () => {
          this.toastr.success('Usługa dodana pomyślnie!');
          this.closed.emit(true);
        },
        error: (err) => this.toastr.error('Wystąpił błąd podczas dodawania usługi.')
      });
  }

  closeModal(event: MouseEvent) {
    if ((event.target as HTMLElement).classList.contains('modal')) {
      this.closed.emit(false);
    }
  }
}