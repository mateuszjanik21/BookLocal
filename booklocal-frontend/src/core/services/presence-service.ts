import { inject, Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { environment } from '../../environments/environment';
import { BehaviorSubject, Subject } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { Conversation } from '../../types/conversation.model';
import { NotificationService } from './notification';
import { Router } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class PresenceService {
  private hubConnection?: HubConnection;
  private onlineUsersSource = new BehaviorSubject<string[]>([]);
  onlineUsers$ = this.onlineUsersSource.asObservable();
  private conversationUpdatedSource = new Subject<Conversation>();
  conversationUpdated$ = this.conversationUpdatedSource.asObservable();

  private toastr = inject(ToastrService);
  private router = inject(Router);

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

    this.hubConnection.on('UpdateConversation', (updatedConvo: Conversation) => {
      this.conversationUpdatedSource.next(updatedConvo);
      if (!this.router.url.includes('/chat')) {
        const fullMessage = updatedConvo.lastMessage;
        const maxLength = 25;
        const toastMessage = fullMessage.length > maxLength 
        ? fullMessage.substring(0, maxLength) + '...' 
        : fullMessage;
        const toastTitle = `Nowa wiadomość od: ${updatedConvo.participantName}`;
        this.toastr.info(toastMessage, toastTitle, { 
          timeOut: 8000 
        })
          .onTap.subscribe(() => this.router.navigate(['/chat']));
      }
    });
  }

  stopHubConnection() {
    this.hubConnection?.stop().catch(err => console.error(err));
  }
}