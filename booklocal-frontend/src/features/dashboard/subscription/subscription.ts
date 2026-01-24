import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CurrentSubscription, SubscriptionPlan, SubscriptionService } from '../../../core/services/subscription-service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-subscription',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './subscription.html'
})
export class SubscriptionManagerComponent implements OnInit {
  private subscriptionService = inject(SubscriptionService);
  private toastr = inject(ToastrService);

  plans: SubscriptionPlan[] = [];
  currentSubscription: CurrentSubscription | null = null;
  isLoading = true;

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.isLoading = true;
    this.subscriptionService.getPublicPlans().subscribe({
      next: (plans) => {
        this.plans = plans;
        this.loadCurrentSubscription();
      },
      error: () => {
        this.toastr.error('Nie udało się pobrać planów subskrypcyjnych.');
        this.isLoading = false;
      }
    });
  }

  loadCurrentSubscription() {
    this.subscriptionService.getCurrentSubscription().subscribe({
      next: (sub) => {
        this.currentSubscription = sub;
        this.isLoading = false;
      },
      error: () => {
        console.error('Failed to load current subscription');
        this.isLoading = false;
      }
    });
  }

  selectPlan(plan: SubscriptionPlan) {
    if (this.currentSubscription?.planId === plan.planId) {
      return;
    }

    if (!confirm(`Czy na pewno chcesz zmienić plan na ${plan.name}?`)) {
        return;
    }

    this.subscriptionService.subscribe(plan.planId).subscribe({
      next: () => {
        this.toastr.success(`Pomyślnie zmieniono plan na ${plan.name}.`);
        this.loadCurrentSubscription();
      },
      error: (err) => {
        this.toastr.error('Wystąpił błąd podczas zmiany planu.');
        console.error(err);
      }
    });
  }
}
