import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../../core/services/notification';
import { Subscription } from 'rxjs';
import { NotificationPayload } from '../../../types/notification.model';
import { RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './notification-bell.html',
})
export class NotificationBellComponent implements OnInit, OnDestroy {
  private notificationService = inject(NotificationService);
  private notificationSub?: Subscription;
  private toastr = inject(ToastrService);

  notifications: NotificationPayload[] = [];
  unreadCount = 0;

  ngOnInit(): void {
    this.notificationSub = this.notificationService.notification$.subscribe(payload => {
      this.notifications.unshift(payload);
      this.unreadCount++;
      this.toastr.info(payload.message, 'Nowa Rezerwacja!');

      if (payload.message.toLowerCase().includes('anulował')) {
        this.toastr.warning(payload.message, 'Rezerwacja Anulowana!');
      } else {
        this.toastr.info(payload.message, 'Nowa Rezerwacja!');
      }
    });
  }

  clearNotifications(): void {
    this.unreadCount = 0;
  }

  ngOnDestroy(): void {
    this.notificationSub?.unsubscribe();
  }
}