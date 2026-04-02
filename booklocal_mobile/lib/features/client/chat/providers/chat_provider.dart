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

  /// Ustawiane z zewnątrz z poziomu UI (ConversationScreen), 
  /// aby powiązać z PresenceService do odświeżania badge'a.
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

  /// Wzór Angular: Otwieramy ChatHub, rejestrujemy listenery, 
  /// POTEM markujemy jako przeczytane.
  Future<void> startListening(int conversationId) async {
    await _chatService.startHubConnection(conversationId);

    // Rejestruj handlery PRZED markAsRead
    _chatService.onMessageReceived = (newMessage) async {
      // Szukaj tymczasowej wiadomości (id=0) o tej samej treści i nadawcy
      // — to echo naszej własnej wiadomości wracające z serwera
      final tempIndex = _currentMessages.indexWhere(
        (m) => m.id == 0 && m.senderId == newMessage.senderId && m.content == newMessage.content,
      );

      if (tempIndex != -1) {
        // Zastąp tymczasową prawdziwą wersją z serwera
        _currentMessages[tempIndex] = newMessage;
      } else if (newMessage.id == 0 || !_currentMessages.any((m) => m.id == newMessage.id)) {
        // Nowa wiadomość od drugiej strony
        _currentMessages.add(newMessage);
      } else {
        // Duplikat, ignoruj
        return;
      }

      notifyListeners();

      // Automatycznie oznacz jako przeczytane jeśli jesteśmy w tej konwersacji
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
      // Odśwież badge globalny
      _presenceService?.refreshUnreadCount();
    };

    // TERAZ oznacz jako przeczytane (Angular wzór: listenery zarejestrowane PRZED)
    await _chatService.markMessagesAsRead(conversationId);

    // Lokalnie zeruj w UI
    final idx = _conversations.indexWhere((c) => c.conversationId == conversationId);
    if (idx != -1) {
      _conversations[idx].unreadCount = 0;
      notifyListeners();
    }

    // Odśwież badge globalny
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
