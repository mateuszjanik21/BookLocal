import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { BehaviorSubject, Observable, Subject, take, tap } from 'rxjs';
import { Conversation } from '../../types/conversation.model';
import { Message } from '../../types/message.model';
import { AuthService } from './auth-service';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;
  private hubConnection?: HubConnection;
  private authService = inject(AuthService);

  private messageThreadSource = new BehaviorSubject<Message[]>([]);
  public messageThread$ = this.messageThreadSource.asObservable();
  public newMessages$ = new Subject<Message>();

  private conversationToOpen = new BehaviorSubject<number | null>(null);
  public conversationToOpen$ = this.conversationToOpen.asObservable();

  startConversation(businessId: number): Observable<Conversation> {
  return this.http.post<Conversation>(`${this.apiUrl}/messages/start`, { businessId }).pipe(
    tap(conversation => {
      this.conversationToOpen.next(conversation.conversationId);
    })
  );
}

  getMyConversations(): Observable<Conversation[]> {
    return this.http.get<Conversation[]>(`${this.apiUrl}/messages/my-conversations`);
  }

  getMessages(conversationId: number): Observable<Message[]> {
    return this.http.get<Message[]>(`${this.apiUrl}/messages/${conversationId}`).pipe(
      tap(messages => this.messageThreadSource.next(messages))
    );
  }

  async sendMessage(conversationId: number, content: string) {
    return this.hubConnection?.invoke('SendMessage', conversationId, content)
      .catch(error => console.error("Błąd podczas wysyłania wiadomości:", error));
  }

  startConversationAsOwner(customerId: string): Observable<Conversation> {
    return this.http.post<Conversation>(`${this.apiUrl}/messages/start-as-owner`, { customerId }).pipe(
      tap(conversation => {
        this.conversationToOpen.next(conversation.conversationId);
      })
    );
  }

  clearConversationToOpen(): void {
    this.conversationToOpen.next(null);
  }

  async markMessagesAsRead(conversationId: number) {
    return this.hubConnection?.invoke('MarkMessagesAsRead', conversationId)
      .catch(error => console.error("Błąd podczas oznaczania wiadomości jako przeczytane:", error));
  }

  public stopHubConnection() {
    if (this.hubConnection) {
      this.hubConnection.stop().catch(error => console.error(error));
    }
  }

  createHubConnection(conversationId: number): Promise<void> {
    this.stopHubConnection();
    const token = localStorage.getItem('authToken');
    if (!token) {
      return Promise.reject("Brak tokenu.");
    }

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${this.apiUrl}/chatHub`, { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .build();

    return this.hubConnection.start()
      .then(() => {
        console.log(`Połączenie z ChatHub nawiązane dla konwersacji ${conversationId}.`);
        this.hubConnection?.invoke('JoinConversation', conversationId);
        
        this.hubConnection?.on('ReceiveMessage', (message: Message) => {
            const currentMessages = this.messageThreadSource.getValue();
            this.messageThreadSource.next([...currentMessages, message]);
            if (message.conversationId === conversationId) {
              this.markMessagesAsRead(conversationId);
            }
        });

        this.hubConnection?.on('MessagesRead', (readConversationId: number) => {
            if (readConversationId === conversationId) {
              this.authService.currentUser$.pipe(take(1)).subscribe(user => {
                if (user) {
                  const messages = this.messageThreadSource.getValue();
                  messages.forEach(message => {
                    if (!message.isRead && message.senderId === user.id) {
                      message.isRead = true;
                    }
                  });
                  this.messageThreadSource.next([...messages]);
                }
              });
            }
        });

        this.hubConnection?.onclose(error => {
            console.warn("Połączenie z ChatHub zostało zamknięte.", error);
        });
      });
  }
}