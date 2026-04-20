import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../../core/services/notification';
import { Subscription } from 'rxjs';
import { NotificationPayload } from '../../../types/notification.model';
import { RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

interface AppNotification extends NotificationPayload {
  isRead: boolean;
}

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

  notifications: AppNotification[] = [];
  unreadCount = 0;

  ngOnInit(): void {
    this.notificationSub = this.notificationService.notification$.subscribe(payload => {
      this.notifications.unshift({ ...payload, isRead: false });
      this.unreadCount++;

      if (payload.message.toLowerCase().includes('anulowa')) {
        this.toastr.warning(payload.message, 'Rezerwacja Anulowana!');
      } else {
        this.toastr.info(payload.message, 'Nowa Rezerwacja!');
      }
    });
  }

  markAsRead(notification: AppNotification): void {
    if (!notification.isRead) {
      notification.isRead = true;
      this.unreadCount = Math.max(0, this.unreadCount - 1);
    }
  }

  clearAll(): void {
    this.notifications = [];
    this.unreadCount = 0;
  }

  ngOnDestroy(): void {
    this.notificationSub?.unsubscribe();
  }
}