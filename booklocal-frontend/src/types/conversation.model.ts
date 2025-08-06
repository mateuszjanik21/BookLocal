export interface Conversation {
  conversationId: number;
  participantId: string;
  participantName: string;
  participantPhotoUrl?: string;
  lastMessage: string;
  lastMessageAt: string;
  unreadCount: number;
}