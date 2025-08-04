import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { environment } from '../../environments/environment';
import { BehaviorSubject } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

@Injectable({ providedIn: 'root' })
export class PresenceService {
  private hubConnection?: HubConnection;
  private onlineUsersSource = new BehaviorSubject<string[]>([]);
  onlineUsers$ = this.onlineUsersSource.asObservable();

  constructor(private toastr: ToastrService) { }

  createHubConnection() {
    const token = localStorage.getItem('authToken');
    if (!token) return;

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/presenceHub`, { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start().catch(err => console.error("Błąd połączenia z PresenceHub:", err));

    this.hubConnection.on('GetOnlineUsers', (userIds: string[]) => {
      console.log('PRESENCE SERVICE: Otrzymano listę początkową:', userIds);
      this.onlineUsersSource.next(userIds);
    });

    this.hubConnection.on('UserIsOnline', userId => {
      console.log('PRESENCE SERVICE: Użytkownik dołączył:', userId);
      const currentUsers = this.onlineUsersSource.getValue();
      if (!currentUsers.includes(userId)) {
        this.onlineUsersSource.next([...currentUsers, userId]);
      }
    });

    this.hubConnection.on('UserIsOffline', userId => {
      console.log('PRESENCE SERVICE: Użytkownik wyszedł:', userId);
      const currentUsers = this.onlineUsersSource.getValue();
      this.onlineUsersSource.next(currentUsers.filter(x => x !== userId));
    });
  }

  stopHubConnection() {
    this.hubConnection?.stop().catch(err => console.error(err));
  }
}