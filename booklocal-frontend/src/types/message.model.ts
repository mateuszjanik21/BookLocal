export interface Message {
  messageId: number;
  conversationId: number;
  content: string;
  sentAt: string;
  senderId: string;
  senderFullName: string;
  senderPhotoUrl?: string;
  isRead: boolean;
}