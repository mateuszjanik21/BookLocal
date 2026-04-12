import 'dart:async';
import 'package:flutter/material.dart';
import '../../../../core/models/chat_models.dart';
import '../../../../core/services/chat_services.dart';
import '../../../../core/services/presence_service.dart';

class ChatProvider extends ChangeNotifier {
  final ChatService _chatService;
  PresenceService? _presenceService;

  List<ConversationDto> _conversations = [];
  bool _isLoadingConversations = false;
  int? _activeConversationId;

  List<MessageDto> _currentMessages = [];
  bool _isLoadingMessages = false;

  ChatProvider(this._chatService);

  void setPresenceService(PresenceService ps) {
    _presenceService = ps;
  }

  List<ConversationDto> get conversations => _conversations;
  bool get isLoadingConversations => _isLoadingConversations;
  List<MessageDto> get currentMessages => _currentMessages;
  bool get isLoadingMessages => _isLoadingMessages;

  void setActiveConversationId(int? id) {
    _activeConversationId = id;
  }

  Future<void> loadMyConversations() async {
    _isLoadingConversations = true;
    notifyListeners();
    try {
      _conversations = await _chatService.getMyConversations();
      for (var c in _conversations) {
        print('[ChatProvider] Konwersacja ${c.conversationId} (${c.participantName}): unreadCount=${c.unreadCount}');
      }
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

    _chatService.onMessageReceived = (newMessage) async {
      final tempIndex = _currentMessages.indexWhere(
        (m) => m.id == 0 && m.senderId == newMessage.senderId && m.content == newMessage.content,
      );

      if (tempIndex != -1) {
        _currentMessages[tempIndex] = newMessage;
      } else if (newMessage.id == 0 || !_currentMessages.any((m) => m.id == newMessage.id)) {
        _currentMessages.add(newMessage);
      } else {
        return;
      }

      notifyListeners();

      if (_activeConversationId == conversationId) {
        await _chatService.markMessagesAsRead(conversationId);
      }
    };

    _chatService.onMessagesRead = (convId) {
      print('[ChatProvider] onMessagesRead dla konwersacji $convId');
      if (convId == _activeConversationId) {
        for (var m in _currentMessages) {
          m.isRead = true;
        }
        notifyListeners();
      }
      _presenceService?.refreshUnreadCount();
    };

    await _chatService.markMessagesAsRead(conversationId);

    final idx = _conversations.indexWhere((c) => c.conversationId == conversationId);
    if (idx != -1) {
      _conversations[idx].unreadCount = 0;
      notifyListeners();
    }

    _presenceService?.refreshUnreadCount();
  }

  void stopListening() {
    _chatService.stopHubConnection();
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

  Future<int?> startConversation(int businessId) async {
    return await _chatService.startConversation(businessId);
  }
}
