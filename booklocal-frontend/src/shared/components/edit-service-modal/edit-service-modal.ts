import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ServiceService } from '../../../core/services/service';
import { Service } from '../../../types/business.model';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-edit-service-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './edit-service-modal.html',
  styleUrl: './edit-service-modal.css'
})
export class EditServiceModalComponent implements OnChanges {
  @Input() service: Service | null = null;
  @Input() businessId!: number;
  @Output() closed = new EventEmitter<boolean>();
  @Input() isVisible: boolean = false;
  
  private fb = inject(FormBuilder);
  private serviceService = inject(ServiceService);
  private toastr = inject(ToastrService);

  isSubmitting = false;
  serviceForm = this.fb.group({
    name: ['', Validators.required],
    description: ['']
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['service'] && this.service) {
      this.serviceForm.patchValue(this.service);
    }
  }

  onSubmit() {
    if (this.serviceForm.invalid || !this.service) return;

    this.isSubmitting = true;
    const formValue = this.serviceForm.value;

    const payload = {
      name: formValue.name,
      description: formValue.description,
      serviceCategoryId: this.service.serviceCategoryId,
      variants: this.service.variants
    };

    this.serviceService.updateService(this.businessId, this.service.id, payload as any)
      .pipe(finalize(() => this.isSubmitting = false))
      .subscribe({
        next: () => {
          this.toastr.success('Dane usługi zostały zaktualizowane!');
          this.closed.emit(true);
        },
        error: (err) => this.toastr.error(`Błąd: ${err.error.title || 'Sprawdź dane.'}`)
      });
  }

  closeModal(event: MouseEvent) {
    if ((event.target as HTMLElement).classList.contains('modal')) {
      this.closed.emit(false);
    }
  }
}