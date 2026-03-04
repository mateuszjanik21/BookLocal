import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Discount, DiscountService } from '../../../../core/services/discount-service';
import { BusinessService } from '../../../../core/services/business-service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-discount-manager',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './discount-manager.html',
})
export class DiscountManagerComponent implements OnInit {
  private discountService = inject(DiscountService);
  private businessService = inject(BusinessService);
  private fb = inject(FormBuilder);
  private toastr = inject(ToastrService);

  businessId: number | null = null;
  discounts: Discount[] = [];
  isLoading = false;
  isCreating = false;

  get totalDiscounts(): number { return this.discounts.length; }
  get activeDiscounts(): number { return this.discounts.filter(d => d.isActive).length; }
  get totalUses(): number { return this.discounts.reduce((sum, d) => sum + d.usedCount, 0); }
  get topDiscount(): Discount | null {
      if (this.discounts.length === 0) return null;
      return [...this.discounts].sort((a, b) => b.usedCount - a.usedCount)[0];
  }

  discountForm = this.fb.group({
    code: ['', [Validators.required, Validators.minLength(3)]],
    type: [0, [Validators.required]],
    value: [10, [Validators.required, Validators.min(0.01)]],
    maxUses: [null as number | null],
    maxUsesPerCustomer: [null as number | null],
    validFrom: [null as string | null],
    validTo: [null as string | null]
  });

  ngOnInit() {
    this.businessService.getMyBusiness().subscribe(b => {
      this.businessId = b.id;
      this.loadDiscounts();
    });
  }

  loadDiscounts() {
    if (!this.businessId) return;
    this.isLoading = true;
    this.discountService.getDiscounts(this.businessId).subscribe({
      next: (data) => {
        this.discounts = data;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  openCreateModal() {
    const modal = document.getElementById('create_discount_modal') as HTMLDialogElement;
    if (modal) modal.showModal();
  }

  closeModal() {
    const modal = document.getElementById('create_discount_modal') as HTMLDialogElement;
    if (modal) modal.close();
    this.discountForm.reset({ type: 0, value: 10 });
  }

  createDiscount() {
    if (this.discountForm.invalid || !this.businessId) return;
    
    this.isCreating = true;
    const formVal = this.discountForm.value;

    const payload = {
        code: formVal.code!,
        type: Number(formVal.type),
        value: formVal.value!,
        maxUses: formVal.maxUses ? formVal.maxUses : undefined,
        maxUsesPerCustomer: formVal.maxUsesPerCustomer ? formVal.maxUsesPerCustomer : undefined,
        validFrom: formVal.validFrom ? formVal.validFrom : undefined,
        validTo: formVal.validTo ? formVal.validTo : undefined
    };

    this.discountService.createDiscount(this.businessId, payload).subscribe({
        next: (newDiscount) => {
            this.discounts.unshift(newDiscount);
            this.toastr.success('Kupon utworzony.');
            this.isCreating = false;
            this.closeModal();
        },
        error: (err) => {
            this.toastr.error(err.error || 'Błąd tworzenia.');
            this.isCreating = false;
        }
    });
  }

  toggleStatus(discount: Discount) {
      if (!this.businessId) return;
      this.discountService.toggleDiscount(this.businessId, discount.discountId).subscribe({
          next: () => {
              discount.isActive = !discount.isActive;
              this.toastr.success('Status zaktualizowany.');
          },
          error: () => this.toastr.error('Wystąpił błąd.')
      });
  }

  copyCode(code: string) {
      navigator.clipboard.writeText(code).then(() => {
          this.toastr.success(`Skopiowano kod ${code}`);
      }).catch(() => {
          this.toastr.error('Nie udało się skopiować kodu.');
      });
  }

  getProgress(discount: Discount): number {
      if (!discount.maxUses) return 0;
      return Math.min(100, Math.round((discount.usedCount / discount.maxUses) * 100));
  }
}
