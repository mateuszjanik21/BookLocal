import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ServiceService } from '../../../core/services/service';
import { Service, ServiceVariant } from '../../../types/business.model';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-variant-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './variant-modal.html',
})
export class VariantModalComponent implements OnChanges {
  @Input() service: Service | null = null;
  @Input() variant: ServiceVariant | null = null;
  @Input() businessId!: number;
  @Output() closed = new EventEmitter<boolean>();
  @Input() isVisible: boolean = false;
  
  private fb = inject(FormBuilder);
  private serviceService = inject(ServiceService);
  private toastr = inject(ToastrService);

  isSubmitting = false;
  variantForm = this.fb.group({
    name: ['', Validators.required],
    price: [0, [Validators.required, Validators.min(0)]],
    durationMinutes: [30, [Validators.required, Validators.min(1)]],
    cleanupTimeMinutes: [0, [Validators.min(0)]]
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['variant'] && this.variant) {
      this.variantForm.patchValue({
        name: this.variant.name,
        price: this.variant.price,
        durationMinutes: this.variant.durationMinutes,
        cleanupTimeMinutes: this.variant.cleanupTimeMinutes
      });
    } else if (changes['isVisible'] && this.isVisible && !this.variant) {
       this.variantForm.reset({
          name: '',
          price: 0,
          durationMinutes: 30,
          cleanupTimeMinutes: 0
       });
    }
  }

  onSubmit() {
    if (this.variantForm.invalid || !this.service) return;

    this.isSubmitting = true;
    const formValue = this.variantForm.value;

    let updatedVariants = [...this.service.variants];

    if (this.variant) {
      updatedVariants = updatedVariants.map(v => 
        v.serviceVariantId === this.variant!.serviceVariantId 
          ? { ...v, ...formValue, serviceVariantId: v.serviceVariantId, isDefault: v.isDefault } as any
          : v
      );
    } else {
      updatedVariants.push({
        name: formValue.name!,
        price: formValue.price!,
        durationMinutes: formValue.durationMinutes!,
        cleanupTimeMinutes: formValue.cleanupTimeMinutes!,
        isDefault: false
      } as any);
    }

    const payload = {
      name: this.service.name,
      description: this.service.description,
      serviceCategoryId: this.service.serviceCategoryId,
      variants: updatedVariants
    };

    this.serviceService.updateService(this.businessId, this.service.id, payload as any)
      .pipe(finalize(() => this.isSubmitting = false))
      .subscribe({
        next: () => {
          this.toastr.success(this.variant ? 'Wariant zaktualizowany!' : 'Dodano nowy wariant!');
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
