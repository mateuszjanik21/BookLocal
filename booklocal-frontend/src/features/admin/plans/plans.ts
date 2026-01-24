import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService, SubscriptionPlan } from '../../../core/services/admin-service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-admin-plans',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './plans.html'
})
export class AdminPlansComponent implements OnInit {
  private adminService = inject(AdminService);
  private toastr = inject(ToastrService);

  plans: SubscriptionPlan[] = [];
  isLoading = false;

  newPlan: any = {
    name: '',
    priceMonthly: 0,
    priceYearly: 0,
    maxEmployees: 1,
    maxServices: 5,
    hasAdvancedReports: false,
    hasMarketingTools: false,
    commissionPercentage: 0,
    isActive: true 
  };

  isCreating = false;

  ngOnInit() {
    this.loadPlans();
  }

  loadPlans() {
    this.isLoading = true;
    this.adminService.getPlans().subscribe({
      next: (data) => {
        this.plans = data;
        this.isLoading = false;
      },
      error: () => {
        this.toastr.error('Nie udało się pobrać planów.');
        this.isLoading = false;
      }
    });
  }

  createPlan() {
    this.isCreating = true;
    
    if (this.editingId) {
      this.adminService.updatePlan(this.editingId, this.newPlan).subscribe({
        next: () => {
          this.toastr.success('Plan zaktualizowany pomyślnie.');
          this.loadPlans();
          this.cancelEdit();
          this.isCreating = false;
        },
        error: () => {
          this.toastr.error('Błąd podczas aktualizacji planu.');
          this.isCreating = false;
        }
      });
    } else {
      this.adminService.createPlan(this.newPlan).subscribe({
        next: (plan) => {
          this.toastr.success('Plan utworzony pomyślnie.');
          this.plans.push(plan);
          this.isCreating = false;
          this.resetForm();
        },
        error: () => {
          this.toastr.error('Błąd podczas tworzenia planu.');
          this.isCreating = false;
        }
      });
    }
  }

  editingId: number | null = null;
  get isEditing(): boolean {
    return !!this.editingId;
  }

  editPlan(plan: SubscriptionPlan) {
    this.editingId = plan.planId;
    this.newPlan = {
      name: plan.name,
      priceMonthly: plan.priceMonthly,
      priceYearly: plan.priceYearly,
      maxEmployees: plan.maxEmployees,
      maxServices: plan.maxServices,
      hasAdvancedReports: plan.hasAdvancedReports,
      hasMarketingTools: plan.hasMarketingTools,
      commissionPercentage: plan.commissionPercentage,
      isActive: plan.isActive
    };
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  cancelEdit() {
    this.editingId = null;
    this.resetForm();
  }

  deletePlan(plan: SubscriptionPlan) {
    if(!confirm(`Czy na pewno chcesz dezaktywować plan "${plan.name}"?`)) return;

    this.adminService.deletePlan(plan.planId).subscribe({
      next: () => {
        this.toastr.success('Plan został dezaktywowany.');
        const found = this.plans.find(p => p.planId === plan.planId);
        if (found) {
            found.isActive = false;
        }
      },
      error: () => {
        this.toastr.error('Błąd podczas usuwania planu.');
      }
    });
  }

  activatePlan(plan: SubscriptionPlan) {
    if(!confirm(`Czy na pewno chcesz przywrócić plan "${plan.name}"?`)) return;

    const planToActivate = { ...plan, isActive: true };

    this.adminService.updatePlan(plan.planId, planToActivate).subscribe({
        next: () => {
            this.toastr.success('Plan został pomyślnie aktywowany.');
            const found = this.plans.find(p => p.planId === plan.planId);
            if (found) {
                found.isActive = true;
            }
        },
        error: () => {
            this.toastr.error('Błąd podczas aktywacji planu.');
        }
    });
  }

  resetForm() {
    this.newPlan = {
      name: '', priceMonthly: 0, priceYearly: 0, 
      maxEmployees: 1, maxServices: 5, 
      hasAdvancedReports: false, hasMarketingTools: false, commissionPercentage: 0,
      isActive: true
    };
  }
}