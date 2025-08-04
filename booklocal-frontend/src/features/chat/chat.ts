import { Component, OnDestroy, OnInit, inject, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';

import { ChatService } from '../../core/services/chat';
import { AuthService } from '../../core/services/auth-service';
import { Conversation } from '../../types/conversation.model';
import { UserDto } from '../../types/auth.models';
import { ToastrService } from 'ngx-toastr';
import { PresenceService } from '../../core/services/presence-service';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.html',
})
export class ChatComponent implements OnInit, OnDestroy, AfterViewChecked {
  @ViewChild('messageContainer') private messageContainer!: ElementRef;
  private toastr = inject(ToastrService);
  chatService = inject(ChatService);
  authService = inject(AuthService);
  presenceService = inject(PresenceService);

  private subscriptions: Subscription[] = [];
  private needsScroll = false;

  conversations: Conversation[] = [];
  currentUser: UserDto | null = null;
  messageContent = '';
  activeConversationId: number | null = null;
  activeParticipantName: string | null = null;
      
  ngOnInit(): void {
    const userSub = this.authService.currentUser$.subscribe(user => this.currentUser = user);
    const convoSub = this.chatService.getMyConversations().subscribe(convos => this.conversations = convos);
    const newMsgSub = this.chatService.newMessages$.subscribe(newMessage => {
      if (newMessage.conversationId !== this.activeConversationId) {
        this.toastr.info(newMessage.content, `Nowa wiadomość od: ${newMessage.senderFullName}`);
      }
      this.chatService.getMyConversations().subscribe(convos => this.conversations = convos);
      this.needsScroll = true;
    });

    const convoUpdateSub = this.presenceService.conversationUpdated$.subscribe(updatedConvo => {
      const index = this.conversations.findIndex(c => c.conversationId === updatedConvo.conversationId);
      if (index !== -1) {
        this.conversations[index] = updatedConvo;
      } else {
        this.conversations.unshift(updatedConvo);
      }
    });

    this.subscriptions.push(userSub, convoSub, newMsgSub, convoUpdateSub);
  }

  ngAfterViewChecked(): void {
    if (this.needsScroll) {
      this.scrollToBottom();
      this.needsScroll = false;
    }
  }

  getInitials(name: string): string {
    if (!name) return '';
    const nameParts = name.trim().split(' ');
    if (nameParts.length > 1) {
      return (nameParts[0][0] + nameParts[nameParts.length - 1][0]).toUpperCase();
    }
    return nameParts[0].substring(0, 2).toUpperCase();
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
      
      this.chatService.getMessages(id).subscribe(() => {
        this.needsScroll = true;
        this.chatService.markMessagesAsRead(id);
      });
    } catch (error) {
      console.error("Nie udało się nawiązać połączenia z czatem:", error);
    }
  }

  sendMessage(): void {
    if (this.activeConversationId && this.messageContent.trim() !== '') {
      this.chatService.sendMessage(this.activeConversationId, this.messageContent)
        .then(() => {
          this.messageContent = '';
          this.needsScroll = true;
        });
    }
  }

  ngOnDestroy(): void {
    this.chatService.stopHubConnection();
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  private scrollToBottom(): void {
    setTimeout(() => {
      if (this.messageContainer?.nativeElement) {
        const element = this.messageContainer.nativeElement;
        element.scrollTop = element.scrollHeight;
      }
    }, 0);
  }
}