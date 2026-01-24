import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService, CreateSubscriptionPlan, SubscriptionPlan } from '../../../core/services/admin-service';
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

  newPlan: CreateSubscriptionPlan = {
    name: '',
    priceMonthly: 0,
    priceYearly: 0,
    maxEmployees: 1,
    maxServices: 5,
    hasAdvancedReports: false,
    hasMarketingTools: false,
    commissionPercentage: 0
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
    this.adminService.createPlan(this.newPlan).subscribe({
      next: (plan) => {
        this.toastr.success('Plan utworzony pomyślnie.');
        this.plans.push(plan);
        this.isCreating = false;
        // Reset form
        this.newPlan = {
             name: '', priceMonthly: 0, priceYearly: 0, 
             maxEmployees: 1, maxServices: 5, 
             hasAdvancedReports: false, hasMarketingTools: false, commissionPercentage: 0
        };
      },
      error: () => {
        this.toastr.error('Błąd podczas tworzenia planu.');
        this.isCreating = false;
      }
    });
  }
}
