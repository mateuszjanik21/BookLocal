import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { LoyaltyService } from '../../../core/services/loyalty-service';
import { BusinessService } from '../../../core/services/business-service';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-loyalty-settings',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './loyalty-settings.html',
})
export class LoyaltySettingsComponent implements OnInit {
  private fb = inject(FormBuilder);
  private loyaltyService = inject(LoyaltyService);
  private businessService = inject(BusinessService);
  private toastr = inject(ToastrService);

  businessId: number | null = null;
  isLoading = false;
  isSaving = false;

  configForm = this.fb.group({
    isActive: [false],
    spendAmountForOnePoint: [10.00, [Validators.required, Validators.min(0.01)]]
  });

  ngOnInit() {
    this.businessService.getMyBusiness().subscribe(b => {
      this.businessId = b.id;
      this.loadConfig();
    });
  }

  loadConfig() {
    if (!this.businessId) return;
    this.isLoading = true;
    this.loyaltyService.getConfig(this.businessId).subscribe({
      next: (config) => {
        this.configForm.patchValue({
          isActive: config.isActive,
          spendAmountForOnePoint: config.spendAmountForOnePoint
        });
        this.isLoading = false;
      },
      error: () => {
        this.toastr.error('Błąd ładowania konfiguracji.');
        this.isLoading = false;
      }
    });
  }

  saveConfig() {
    if (!this.businessId || this.configForm.invalid) return;

    this.isSaving = true;
    const payload = {
        isActive: this.configForm.value.isActive!,
        spendAmountForOnePoint: this.configForm.value.spendAmountForOnePoint!
    };

    this.loyaltyService.updateConfig(this.businessId, payload)
        .pipe(finalize(() => this.isSaving = false))
        .subscribe({
            next: () => this.toastr.success('Konfiguracja zapisana.'),
            error: () => this.toastr.error('Błąd zapisu.')
        });
  }

  isRecalculating = false;

  recalculatePoints() {
      if (!this.businessId) return;
      
      this.isRecalculating = true;
      this.loyaltyService.recalculatePoints(this.businessId)
        .pipe(finalize(() => this.isRecalculating = false))
        .subscribe({
            next: (res) => this.toastr.success(res.message || 'Punkty przeliczone.'),
            error: (err) => this.toastr.error(err.error || 'Wystąpił błąd.')
        });
  }
}
