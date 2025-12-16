import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:signalr_netcore/signalr_client.dart'; // Import SignalR
import '../constants/api_config.dart';
import '../models/chat_models.dart';
import 'auth_service.dart';

class ChatService {
  final AuthService _authService;
  HubConnection? _hubConnection;

  // Callbacki do powiadamiania UI o nowych wiadomościach
  Function(MessageDto)? onMessageReceived;

  ChatService(this._authService);

  // 1. Pobieranie listy moich rozmów (REST API)
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

  // 2. Pobieranie historii wiadomości w konkretnej rozmowie (REST API)
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

  // 3. Połączenie z SignalR (WebSockets)
  Future<void> startHubConnection(int conversationId) async {
    final token = _authService.token;
    if (token == null) return;

    // Budujemy URL do Huba (ważne: używamy adresu z ApiConfig, ale bez /api na końcu, jeśli hub jest w root, 
    // ale w Program.cs masz: app.MapHub<ChatHub>("/api/chatHub"), więc jest OK)
    // Musimy tylko podmienić 'http' na URL bazowy.
    
    // Hack na URL: ApiConfig.baseUrl to np. http://10.0.2.2:5068/api
    // Hub jest pod: http://10.0.2.2:5068/api/chatHub
    // Więc wystarczy dokleić /chatHub do baseUrl (usuwając ewentualnie /api jeśli baseUrl je ma, ale u Ciebie ma)
    // W Program.cs: app.MapHub<ChatHub>("/api/chatHub");
    // W ApiConfig: baseUrl = '.../api'
    
    final hubUrl = '${ApiConfig.baseUrl}/chatHub'; 

    _hubConnection = HubConnectionBuilder()
        .withUrl(hubUrl, options: HttpConnectionOptions(
          accessTokenFactory: () async => token,
        ))
        .withAutomaticReconnect()
        .build();

    _hubConnection?.onclose(((error) => print("SignalR Closed: $error")) as ClosedCallback);

    // Nasłuchiwanie na nową wiadomość (nazwa metody z Angulara: 'ReceiveMessage')
    _hubConnection?.on('ReceiveMessage', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        final map = arguments[0] as Map<String, dynamic>;
        final message = MessageDto.fromJson(map);
        
        // Powiadom UI
        if (onMessageReceived != null) {
          onMessageReceived!(message);
        }
      }
    });

    await _hubConnection?.start();
    print("Połączono z SignalR ID: ${_hubConnection?.connectionId}");

    // Dołącz do grupy rozmowy (metoda z backendu: 'JoinConversation')
    await _hubConnection?.invoke('JoinConversation', args: [conversationId]);
  }

  // 4. Wysyłanie wiadomości (przez SignalR)
  Future<void> sendMessage(int conversationId, String content) async {
    // 1. Sprawdź czy obiekt połączenia istnieje
    if (_hubConnection == null) {
      return;
    }

    // 2. Sprawdź stan połączenia
    if (_hubConnection!.state != HubConnectionState.Connected) {
      try {
        await _hubConnection!.start();
      } catch (e) {
        throw Exception("Nie można połączyć z czatem.");
      }
    }

    // 3. Wyślij wiadomość
    try {
      
      // WAŻNE: Argumenty w liście. Backend oczekuje [int, string].
      await _hubConnection!.invoke('SendMessage', args: [conversationId, content]);
      
    } catch (e) {
      throw Exception("Błąd wysyłania: $e");
    }
  }

  // 5. Rozłączanie
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
        // Backend zwraca obiekt: { "conversationId": 123 }
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