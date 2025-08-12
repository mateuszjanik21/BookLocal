import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { OwnerNavbarComponent } from '../owner-navbar/owner-navbar';
import { NotificationService } from '../../core/services/notification';
import { BusinessService } from '../../core/services/business-service';
import { take } from 'rxjs';

@Component({
  selector: 'app-owner-layout',
  standalone: true,
  imports: [RouterModule, OwnerNavbarComponent],
  templateUrl: './owner-layout.html',
})
export class OwnerLayoutComponent implements OnInit, OnDestroy {
  private notificationService = inject(NotificationService);
  private businessService = inject(BusinessService);

  public businessName: string | null = null;
  public businessId: number | null = null;

  ngOnInit(): void {
    this.businessService.getMyBusiness().pipe(take(1)).subscribe(business => {
      if (business) {
        this.notificationService.startConnection(business.id);
        this.businessName = business.name;
        this.businessId = business.id;
      }
    });
  }

  ngOnDestroy(): void {
    this.notificationService.stopConnection();
  }
}