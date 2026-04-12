import 'package:booklocal_mobile/core/models/chat_models.dart';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'package:signalr_netcore/signalr_client.dart';
import '../constants/api_config.dart';
import 'auth_service.dart';
import '../../main.dart';
import '../../features/client/chat/conversation_screen.dart';

class PresenceService extends ChangeNotifier {
  final AuthService _authService;
  HubConnection? _hubConnection;
  bool _isConnected = false;

  int _totalUnreadCount = 0;
  int get totalUnreadCount => _totalUnreadCount;

  int? _activeConversationId;
  bool _isChatScreenActive = false;

  PresenceService(this._authService);

  void setActiveConversationId(int? id) {
    _activeConversationId = id;
  }

  void setChatScreenActive(bool active) {
    _isChatScreenActive = active;
  }

  Future<void> createHubConnection() async {
    if (_isConnected && _hubConnection != null) {
      return;
    }

    final token = _authService.token;
    if (token == null || token.isEmpty) return;

    await _stopInternal();

    final hubUrl = '${ApiConfig.baseUrl}/presenceHub';
    print('[PresenceService] Łączenie z: $hubUrl');

    _hubConnection = HubConnectionBuilder()
        .withUrl(hubUrl, options: HttpConnectionOptions(
          accessTokenFactory: () async => token,
        ))
        .withAutomaticReconnect()
        .build();

    _hubConnection!.on('UpdateConversation', _onUpdateConversation);
    _hubConnection!.on('GetOnlineUsers', (args) {
      print('[PresenceService] Online users: $args');
    });

    _hubConnection!.onclose(({Exception? error}) {
      print('[PresenceService] Połączenie zamknięte: $error');
      _isConnected = false;
    });

    _hubConnection!.onreconnected(({String? connectionId}) {
      print('[PresenceService] Ponownie połączono: $connectionId');
      _isConnected = true;
      refreshUnreadCount();
    });

    try {
      await _hubConnection!.start();
      _isConnected = true;
      print('[PresenceService] Połączono z PresenceHub! connectionId: ${_hubConnection!.connectionId}');
      await refreshUnreadCount();
    } catch (e) {
      print('[PresenceService] Błąd łączenia z PresenceHub: $e');
      _isConnected = false;
    }
  }

  void _onUpdateConversation(List<Object?>? arguments) {
    print('[PresenceService] UpdateConversation otrzymane! args: $arguments');
    if (arguments == null || arguments.isEmpty) return;

    try {
      final raw = arguments[0];
      Map<String, dynamic> map;
      if (raw is Map) {
        map = Map<String, dynamic>.from(raw);
      } else {
        print('[PresenceService] Nieznany typ argumentu: ${raw.runtimeType}');
        return;
      }

      final dto = ConversationDto.fromJson(map);
      print('[PresenceService] Konwersacja: ${dto.conversationId}, od: ${dto.participantName}, treść: ${dto.lastMessage}');

      refreshUnreadCount();

      if (_isChatScreenActive || _activeConversationId == dto.conversationId) {
        print('[PresenceService] Jesteśmy w czacie, pomijam toast');
        return;
      }

      _showToast(dto);
    } catch (e) {
      print('[PresenceService] Błąd parsowania UpdateConversation: $e');
    }
  }

  void _showToast(ConversationDto dto) {
    final messenger = globalMessengerKey.currentState;
    final navContext = globalNavigatorKey.currentContext;
    if (messenger == null || navContext == null) return;

    messenger.showSnackBar(
      SnackBar(
        content: InkWell(
          onTap: () {
            messenger.hideCurrentSnackBar();
            Navigator.push(
              navContext,
              MaterialPageRoute(
                builder: (_) => ConversationScreen(
                  conversationId: dto.conversationId,
                  participantName: dto.participantName,
                ),
              ),
            );
          },
          child: Row(
            children: [
              const Icon(Icons.mark_chat_unread, color: Colors.white),
              const SizedBox(width: 12),
              Expanded(
                child: Text(
                  "Nowa wiadomość od: ${dto.participantName}\n${dto.lastMessage}",
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
                ),
              ),
              IconButton(
                icon: const Icon(Icons.close, color: Colors.white70, size: 20),
                padding: EdgeInsets.zero,
                constraints: const BoxConstraints(),
                onPressed: () => messenger.hideCurrentSnackBar(),
              ),
            ],
          ),
        ),
        behavior: SnackBarBehavior.floating,
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
        margin: const EdgeInsets.only(bottom: 30, left: 16, right: 16),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        backgroundColor: const Color(0xFF16a34a).withOpacity(0.95),
        elevation: 6,
        duration: const Duration(seconds: 5),
      ),
    );
  }

  Future<void> stopHubConnection() async {
    await _stopInternal();
  }

  Future<void> _stopInternal() async {
    if (_hubConnection != null) {
      try {
        await _hubConnection!.stop();
      } catch (e) {
        print('[PresenceService] Błąd zamykania: $e');
      }
      _hubConnection = null;
      _isConnected = false;
    }
  }

  Future<void> refreshUnreadCount() async {
    final token = _authService.token;
    if (token == null) return;

    try {
      final response = await http.get(
        Uri.parse('${ApiConfig.baseUrl}/messages/unread-count'),
        headers: {'Authorization': 'Bearer $token'},
      );
      if (response.statusCode == 200) {
        final count = int.tryParse(response.body) ?? 0;
        if (_totalUnreadCount != count) {
          _totalUnreadCount = count;
          notifyListeners();
        }
      }
    } catch (e) {
      print('[PresenceService] Błąd pobierania unread-count: $e');
    }
  }
}
