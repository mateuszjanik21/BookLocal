import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../environments/environment';
import { NotificationPayload } from '../../types/notification.model';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private hubConnection?: signalR.HubConnection;

  private notificationSubject = new Subject<NotificationPayload>();
  public notification$ = this.notificationSubject.asObservable();

  public startConnection(businessId: number): void {
    const token = localStorage.getItem('authToken');
    if (!token || this.hubConnection) return; 

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/notificationHub`, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => {
        console.log('Połączenie SignalR nawiązane.');
        this.hubConnection?.invoke('JoinBusinessGroup', businessId.toString())
          .catch(err => console.error('Błąd dołączania do grupy:', err));

        this.listenForNotifications();
      })
      .catch(err => console.error('Błąd podczas nawiązywania połączenia SignalR:', err));
  }

  public stopConnection(): void {
    this.hubConnection?.stop().then(() => {
        console.log('Połączenie SignalR zakończone.');
        this.hubConnection = undefined;
    });
  }

  private listenForNotifications(): void {
    this.hubConnection?.on('NewReservationNotification', (payload: NotificationPayload) => {
      this.notificationSubject.next(payload);
    });

    this.hubConnection?.on('ReservationCancelledNotification', (payload: NotificationPayload) => {
      this.notificationSubject.next(payload);
    });
  }
}