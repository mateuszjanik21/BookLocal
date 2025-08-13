import { Component, OnDestroy, OnInit, inject, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription, take } from 'rxjs';

import { ChatService } from '../../core/services/chat';
import { AuthService } from '../../core/services/auth-service';
import { Conversation } from '../../types/conversation.model';
import { UserDto } from '../../types/auth.models';
import { PresenceService } from '../../core/services/presence-service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.html',
})
export class ChatComponent implements OnInit, OnDestroy, AfterViewChecked {
  @ViewChild('messageContainer') private messageContainer!: ElementRef;

  private subscriptions = new Subscription();
  private toastr = inject(ToastrService);

  chatService = inject(ChatService);
  authService = inject(AuthService);
  presenceService = inject(PresenceService);

  conversations: Conversation[] = [];
  currentUser: UserDto | null = null;
  messageContent = '';
  activeConversationId: number | null = null;
  activeParticipantName: string | null = null;
  needsScroll = false;

  ngOnInit(): void {
    this.subscriptions.add(
      this.authService.currentUser$.subscribe(user => this.currentUser = user)
    );

    this.subscriptions.add(
      this.chatService.getMyConversations().subscribe(convos => {
        this.conversations = convos;
        this.chatService.conversationToOpen$.pipe(take(1)).subscribe(convoId => {
          if (convoId && this.conversations.some(c => c.conversationId === convoId)) {
            this.selectConversation(convoId);
            this.chatService.clearConversationToOpen();
          }
        });
      })
    );

    this.subscriptions.add(
      this.presenceService.conversationUpdated$.subscribe(updatedConvo => {
        const index = this.conversations.findIndex(c => c.conversationId === updatedConvo.conversationId);
        if (index > -1) {
          this.conversations.splice(index, 1);
        }
        this.conversations.unshift(updatedConvo);
      })
    );

    this.subscriptions.add(
      this.chatService.messageThread$.subscribe(() => this.needsScroll = true)
    );
  }

  async selectConversation(id: number): Promise<void> {
    this.activeConversationId = id;
    this.activeParticipantName = this.conversations.find(c => c.conversationId === id)?.participantName || null;

    const selectedConvo = this.conversations.find(c => c.conversationId === id);
    if (selectedConvo) {
      selectedConvo.unreadCount = 0;
    }

    try {
      await this.chatService.createHubConnection(id);
      this.chatService.getMessages(id).subscribe();
      this.chatService.markMessagesAsRead(id);
      this.needsScroll = true;
    } catch (error) {
      console.error("Nie udało się połączyć z czatem:", error);
    }
  }

  sendMessage(): void {
    if (this.activeConversationId && this.messageContent.trim()) {
      this.chatService.sendMessage(this.activeConversationId, this.messageContent)
        .then(() => {
          this.messageContent = '';
          this.needsScroll = true;
        });
    }
  }

  ngAfterViewChecked(): void {
    if (this.needsScroll) {
      this.scrollToBottom();
      this.needsScroll = false;
    }
  }

  ngOnDestroy(): void {
    this.chatService.stopHubConnection();
    this.subscriptions.unsubscribe();
  }

  private scrollToBottom(): void {
    setTimeout(() => {
      if (this.messageContainer?.nativeElement) {
        const element = this.messageContainer.nativeElement;
        element.scrollTop = element.scrollHeight;
      }
    });
  }

  getInitials(name: string): string {
    if (!name) return '';
    const parts = name.trim().split(' ');
    return parts.length > 1
      ? (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
      : parts[0].substring(0, 2).toUpperCase();
  }
}
