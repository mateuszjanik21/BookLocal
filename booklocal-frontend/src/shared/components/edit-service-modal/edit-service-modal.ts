import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ServiceService } from '../../../core/services/service';
import { Service } from '../../../types/business.model';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-edit-service-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './edit-service-modal.html',
})
export class EditServiceModalComponent implements OnChanges {
  @Input() service: Service | null = null;
  businessId = input.required<number>();
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

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['service'] && this.service) {
      this.serviceForm.patchValue(this.service);
    }
  }

  onSubmit() {
    if (this.serviceForm.invalid || !this.businessId() || !this.service) return;

    this.serviceService.updateService(this.businessId(), this.service.id, this.serviceForm.value as any)
      .subscribe({
        next: () => {
          this.toastr.success('Dane usługi zostały zaktualizowane!');
          this.closed.emit();
        },
        error: (err) => this.toastr.error(`Błąd: ${err.error.title || 'Sprawdź dane.'}`)
      });
  }
}