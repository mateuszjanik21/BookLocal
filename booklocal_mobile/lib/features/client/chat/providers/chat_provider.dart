import 'package:flutter/material.dart';
import '../../../../core/models/chat_models.dart';
import '../../../../core/services/chat_services.dart';

class ChatProvider with ChangeNotifier {
  final ChatService _chatService;

  ChatProvider(this._chatService);

  List<ConversationDto> _conversations = [];
  bool _isLoadingConversations = false;

  List<ConversationDto> get conversations => _conversations;
  bool get isLoadingConversations => _isLoadingConversations;

  List<MessageDto> _currentMessages = [];
  bool _isLoadingMessages = false;

  List<MessageDto> get currentMessages => _currentMessages;
  bool get isLoadingMessages => _isLoadingMessages;

  Future<void> loadMyConversations() async {
    _isLoadingConversations = true;
    notifyListeners();
    try {
      _conversations = await _chatService.getMyConversations();
    } catch (e) {
      _conversations = [];
    } finally {
      _isLoadingConversations = false;
      notifyListeners();
    }
  }

  Future<void> loadMessageThread(int conversationId) async {
    _isLoadingMessages = true;
    _currentMessages = [];
    notifyListeners();
    try {
      _currentMessages = await _chatService.getMessageThread(conversationId);
    } catch (e) {
      _currentMessages = [];
    } finally {
      _isLoadingMessages = false;
      notifyListeners();
    }
  }

  Future<void> startListening(int conversationId) async {
    await _chatService.startHubConnection(conversationId);
    _chatService.onMessageReceived = (newMessage) {
      if (!_currentMessages.any((msg) => msg.id == newMessage.id && msg.id != 0)) {
         _currentMessages.add(newMessage);
         notifyListeners();
      }
    };
  }

  void stopListening() {
    _chatService.stopHubConnection();
    _chatService.onMessageReceived = null;
  }

  Future<void> sendMessage(int conversationId, String text, String currentUserId) async {
    final tempMessage = MessageDto(
      id: 0,
      senderId: currentUserId,
      content: text,
      messageSent: DateTime.now(),
      isRead: false,
    );

    _currentMessages.add(tempMessage);
    notifyListeners();

    try {
      await _chatService.sendMessage(conversationId, text);
    } catch (e) {
      _currentMessages.remove(tempMessage);
      notifyListeners();
      rethrow;
    }
  }
}
