import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth-service';
import { BusinessService } from '../../core/services/business-service';
import { Observable, map } from 'rxjs';
import { NotificationBellComponent } from '../../shared/components/notification-bell/notification-bell';

@Component({
  selector: 'app-owner-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule, NotificationBellComponent ],
  templateUrl: './owner-navbar.html',
})
export class OwnerNavbarComponent {
  authService = inject(AuthService);
  businessService = inject(BusinessService);

  businessId$: Observable<number | null> = this.businessService.getMyBusiness().pipe(
    map(business => business ? business.id : null)
  );
}