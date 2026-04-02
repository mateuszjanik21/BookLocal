import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:signalr_netcore/signalr_client.dart';
import '../constants/api_config.dart';
import '../models/chat_models.dart';
import 'auth_service.dart';

/// ChatService — obsługuje ChatHub (per-konwersacja) oraz REST API.
/// Jest singletonem — tworzony RAZ w main.dart.
class ChatService {
  final AuthService _authService;
  HubConnection? _hubConnection;

  // Callbacki do powiadamiania UI
  Function(MessageDto)? onMessageReceived;
  Function(int)? onMessagesRead;

  ChatService(this._authService);

  // ─── REST API ───────────────────────────────────────────────

  Future<List<ConversationDto>> getMyConversations() async {
    final token = _authService.token;
    if (token == null) return [];

    try {
      final response = await http.get(
        Uri.parse('${ApiConfig.baseUrl}/messages/my-conversations'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final List<dynamic> data = jsonDecode(response.body);
        return data.map((json) => ConversationDto.fromJson(json)).toList();
      }
      return [];
    } catch (e) {
      print('[ChatService] getMyConversations error: $e');
      return [];
    }
  }

  Future<List<MessageDto>> getMessageThread(int conversationId) async {
    final token = _authService.token;
    if (token == null) return [];

    try {
      final response = await http.get(
        Uri.parse('${ApiConfig.baseUrl}/messages/$conversationId'),
        headers: {'Authorization': 'Bearer $token'},
      );
      if (response.statusCode == 200) {
        final List<dynamic> data = jsonDecode(response.body);
        return data.map((json) => MessageDto.fromJson(json)).toList();
      }
      return [];
    } catch (e) {
      print('[ChatService] getMessageThread error: $e');
      return [];
    }
  }

  Future<int?> startConversation(int businessId) async {
    final token = _authService.token;
    if (token == null) return null;

    try {
      final response = await http.post(
        Uri.parse('${ApiConfig.baseUrl}/messages/start'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
        body: jsonEncode({'businessId': businessId}),
      );
      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return data['conversationId'];
      }
      return null;
    } catch (e) {
      print('[ChatService] startConversation error: $e');
      return null;
    }
  }

  // ─── SignalR ChatHub (per-konwersacja) ──────────────────────

  /// Wzorowane na Angular `createHubConnection(conversationId)`.
  /// Otwiera ChatHub, dołącza do grupy, rejestruje ReceiveMessage + MessagesRead.
  Future<void> startHubConnection(int conversationId) async {
    // Zamknij stare połączenie
    await stopHubConnection();

    final token = _authService.token;
    if (token == null) return;

    _hubConnection = HubConnectionBuilder()
        .withUrl('${ApiConfig.baseUrl}/chatHub', options: HttpConnectionOptions(
          accessTokenFactory: () async => token,
        ))
        .withAutomaticReconnect()
        .build();

    // Rejestruj PRZED start() — tak jak Angular
    _hubConnection!.on('ReceiveMessage', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final map = Map<String, dynamic>.from(arguments[0] as Map);
          final message = MessageDto.fromJson(map);
          onMessageReceived?.call(message);
        } catch (e) {
          print('[ChatService] Błąd parsowania ReceiveMessage: $e');
        }
      }
    });

    _hubConnection!.on('MessagesRead', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        final convId = int.tryParse(arguments[0].toString());
        if (convId != null) {
          onMessagesRead?.call(convId);
        }
      }
    });

    _hubConnection!.onclose(({Exception? error}) {
      print('[ChatService] ChatHub zamknięty: $error');
    });

    try {
      await _hubConnection!.start();
      await _hubConnection!.invoke('JoinConversation', args: [conversationId]);
    } catch (e) {
      print('[ChatService] Błąd ChatHub: $e');
    }
  }

  Future<void> sendMessage(int conversationId, String content) async {
    if (_hubConnection == null || _hubConnection!.state != HubConnectionState.Connected) {
      throw Exception('ChatHub nie jest połączony');
    }
    await _hubConnection!.invoke('SendMessage', args: [conversationId, content]);
  }

  Future<void> markMessagesAsRead(int conversationId) async {
    if (_hubConnection == null || _hubConnection!.state != HubConnectionState.Connected) {
      print('[ChatService] markMessagesAsRead pominięte — brak połączenia');
      return;
    }
    try {
      await _hubConnection!.invoke('MarkMessagesAsRead', args: [conversationId]);
      print('[ChatService] markMessagesAsRead wysłane dla konwersacji $conversationId');
    } catch (e) {
      print('[ChatService] Błąd markMessagesAsRead: $e');
    }
  }

  Future<void> stopHubConnection() async {
    if (_hubConnection != null) {
      onMessageReceived = null;
      onMessagesRead = null;
      try {
        await _hubConnection!.stop();
      } catch (e) {
        print('[ChatService] Błąd zamykania ChatHub: $e');
      }
      _hubConnection = null;
    }
  }
}