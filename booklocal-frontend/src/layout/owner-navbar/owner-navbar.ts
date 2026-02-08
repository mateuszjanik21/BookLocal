import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth-service';
import { NotificationBellComponent } from '../../shared/components/notification-bell/notification-bell';

import { PresenceService } from '../../core/services/presence-service';

@Component({
  selector: 'app-owner-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule, NotificationBellComponent ],
  templateUrl: './owner-navbar.html',
})
export class OwnerNavbarComponent {
  authService = inject(AuthService);
  presenceService = inject(PresenceService);
  @Input() businessId: number | null = null;
}