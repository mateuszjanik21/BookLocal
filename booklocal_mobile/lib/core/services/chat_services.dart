import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:signalr_netcore/signalr_client.dart';
import '../constants/api_config.dart';
import '../models/chat_models.dart';
import 'auth_service.dart';

class ChatService {
  final AuthService _authService;
  HubConnection? _hubConnection;

  Function(MessageDto)? onMessageReceived;

  ChatService(this._authService);

  Future<List<ConversationDto>> getMyConversations() async {
    final url = Uri.parse('${ApiConfig.baseUrl}/messages/my-conversations');
    final token = _authService.token;

    try {
      final response = await http.get(
        url,
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
      return [];
    }
  }

  Future<List<MessageDto>> getMessageThread(int conversationId) async {
    final url = Uri.parse('${ApiConfig.baseUrl}/messages/$conversationId');
    final token = _authService.token;

    try {
      final response = await http.get(
        url,
        headers: {
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final List<dynamic> data = jsonDecode(response.body);
        return data.map((json) => MessageDto.fromJson(json)).toList();
      }
      return [];
    } catch (e) {
      return [];
    }
  }

  Future<void> startHubConnection(int conversationId) async {
    final token = _authService.token;
    if (token == null) return;
    
    final hubUrl = '${ApiConfig.baseUrl}/chatHub'; 

    _hubConnection = HubConnectionBuilder()
        .withUrl(hubUrl, options: HttpConnectionOptions(
          accessTokenFactory: () async => token,
        ))
        .withAutomaticReconnect()
        .build();

    _hubConnection?.onclose(((error) => print("SignalR Closed: $error")) as ClosedCallback);

    _hubConnection?.on('ReceiveMessage', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        final map = arguments[0] as Map<String, dynamic>;
        final message = MessageDto.fromJson(map);
        
        if (onMessageReceived != null) {
          onMessageReceived!(message);
        }
      }
    });

    await _hubConnection?.start();
    print("Połączono z SignalR ID: ${_hubConnection?.connectionId}");

    await _hubConnection?.invoke('JoinConversation', args: [conversationId]);
  }

  Future<void> sendMessage(int conversationId, String content) async {
    if (_hubConnection == null) {
      return;
    }

    if (_hubConnection!.state != HubConnectionState.Connected) {
      try {
        await _hubConnection!.start();
      } catch (e) {
        throw Exception("Nie można połączyć z czatem.");
      }
    }

    try {
      
      await _hubConnection!.invoke('SendMessage', args: [conversationId, content]);
      
    } catch (e) {
      throw Exception("Błąd wysyłania: $e");
    }
  }

  Future<void> stopHubConnection() async {
    await _hubConnection?.stop();
  }

  Future<int?> startConversation(int businessId) async {
    final url = Uri.parse('${ApiConfig.baseUrl}/messages/start');
    final token = _authService.token;

    try {
      final response = await http.post(
        url,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
        body: jsonEncode({'businessId': businessId}),
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return data['conversationId'];
      } else {
        print("Błąd startConversation: ${response.body}");
        return null;
      }
    } catch (e) {
      print("Błąd sieci startConversation: $e");
      return null;
    }
  }
}