import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { environment } from '../../environments/environment';
import { BehaviorSubject, Subject, take } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { Conversation } from '../../types/conversation.model';
import { Router } from '@angular/router';
import { AuthService } from './auth-service';

@Injectable({ providedIn: 'root' })
export class PresenceService {
  private http = inject(HttpClient);
  private toastr = inject(ToastrService);
  private router = inject(Router);
  private authService = inject(AuthService);
  private apiUrl = environment.apiUrl;

  private hubConnection?: HubConnection;
  
  private onlineUsersSource = new BehaviorSubject<string[]>([]);
  public onlineUsers$ = this.onlineUsersSource.asObservable();
  
  private conversationUpdatedSource = new Subject<Conversation>();
  public conversationUpdated$ = this.conversationUpdatedSource.asObservable();

  private totalUnreadCountSource = new BehaviorSubject<number>(0);
  public totalUnreadCount$ = this.totalUnreadCountSource.asObservable();

  createHubConnection() {
    const token = localStorage.getItem('authToken');
    if (!token) return;

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/presenceHub`, { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start().catch(err => console.error("Błąd połączenia z PresenceHub:", err));

    this.hubConnection.on('GetOnlineUsers', (userIds: string[]) => {
      this.onlineUsersSource.next(userIds);
    });

    this.hubConnection.on('UserIsOnline', userId => {
      const currentUsers = this.onlineUsersSource.getValue();
      if (!currentUsers.includes(userId)) {
        this.onlineUsersSource.next([...currentUsers, userId]);
      }
    });

    this.hubConnection.on('UserIsOffline', userId => {
      const currentUsers = this.onlineUsersSource.getValue();
      this.onlineUsersSource.next(currentUsers.filter(x => x !== userId));
    });

    this.hubConnection.on('UpdateConversation', (updatedConvo: Conversation) => {
      this.conversationUpdatedSource.next(updatedConvo);

      const isAlreadyInChat = this.router.url.includes('/chat');

      if (!isAlreadyInChat) {
        const fullMessage = updatedConvo.lastMessage;
        const maxLength = 25;
        const toastMessage = fullMessage.length > maxLength
          ? fullMessage.substring(0, maxLength) + '...'
          : fullMessage;
        const toastTitle = `Nowa wiadomość od: ${updatedConvo.participantName}`;

        this.toastr.info(toastMessage, toastTitle, {
          timeOut: 8000
        })
          .onTap.subscribe(() => {
            this.authService.currentUser$.pipe(take(1)).subscribe(user => {
              if (user && user.roles.includes('owner')) {
                this.router.navigate(['/dashboard/chat']);
              } else {
                this.router.navigate(['/chat']);
              }
            });
          });
      }
      
      this.refreshUnreadCount();
    });

    this.refreshUnreadCount();
  }

  stopHubConnection() {
    this.hubConnection?.stop().catch(err => console.error(err));
  }

  refreshUnreadCount() {
      this.http.get<number>(`${this.apiUrl}/messages/unread-count`).subscribe({
          next: count => this.totalUnreadCountSource.next(count),
          error: err => console.error('Failed to fetch unread count', err)
      });
  }
}