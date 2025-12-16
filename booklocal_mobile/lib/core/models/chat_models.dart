class ConversationDto {
  final int conversationId;
  final String participantName;
  final String? participantPhotoUrl;
  final String lastMessage;
  final DateTime lastMessageDate;
  final int unreadCount;

  ConversationDto({
    required this.conversationId,
    required this.participantName,
    this.participantPhotoUrl,
    required this.lastMessage,
    required this.lastMessageDate,
    this.unreadCount = 0,
  });

  factory ConversationDto.fromJson(Map<String, dynamic> json) {
    return ConversationDto(
      conversationId: json['conversationId'] ?? 0,
      participantName: json['participantName'] ?? 'Rozmówca',
      participantPhotoUrl: json['participantPhotoUrl'],
      lastMessage: json['lastMessage'] ?? '',
      lastMessageDate: DateTime.tryParse(json['lastMessageDate'] ?? '') ?? DateTime.now(),
      unreadCount: json['unreadCount'] ?? 0,
    );
  }
}

class MessageDto {
  final int id;
  final String senderId; // <--- ZMIANA: int -> String
  final String content;
  final DateTime messageSent;
  final bool isRead;

  MessageDto({
    required this.id,
    required this.senderId,
    required this.content,
    required this.messageSent,
    required this.isRead,
  });

  factory MessageDto.fromJson(Map<String, dynamic> json) {
    return MessageDto(
      id: json['id'] ?? 0,
      // Teraz bezpiecznie rzutujemy na String
      senderId: json['senderId'].toString(), 
      content: json['content'] ?? '',
      messageSent: DateTime.tryParse(json['messageSent'] ?? '') ?? DateTime.now(),
      isRead: json['isRead'] ?? false,
    );
  }
}