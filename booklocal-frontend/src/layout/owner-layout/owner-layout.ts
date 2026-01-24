import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { OwnerNavbarComponent } from '../owner-navbar/owner-navbar';
import { NotificationService } from '../../core/services/notification';
import { BusinessService } from '../../core/services/business-service';
import { take } from 'rxjs';
import { CommonModule } from '@angular/common';
import { PresenceService } from '../../core/services/presence-service';
import { SubscriptionService } from '../../core/services/subscription-service';

@Component({
  selector: 'app-owner-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, OwnerNavbarComponent],
  templateUrl: './owner-layout.html',
})
export class OwnerLayoutComponent implements OnInit, OnDestroy {
  private notificationService = inject(NotificationService);
  private businessService = inject(BusinessService);
  private subscriptionService = inject(SubscriptionService); // Inject
  public presenceService = inject(PresenceService);

  public businessName: string | null = null;
  public businessId: number | null = null;
  
  // Capabilities
  public hasReports = false;
  public hasMarketing = false;

  ngOnInit(): void {
    this.businessService.getMyBusiness().pipe(take(1)).subscribe(business => {
      if (business) {
        this.notificationService.startConnection(business.id);
        this.businessName = business.name;
        this.businessId = business.id;
        
        // Listen for subscription updates
        this.subscriptionService.currentSubscription$.subscribe(sub => {
             if (sub) {
                const s = sub as any;
                this.hasReports = !!s.hasAdvancedReports;
                this.hasMarketing = !!s.hasMarketingTools;
             }
        });
        
        // Trigger initial load
        this.subscriptionService.refreshSubscription();
      }
    });
  }

  ngOnDestroy(): void {
    this.notificationService.stopConnection();
  }
}