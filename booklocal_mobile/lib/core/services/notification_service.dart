import 'package:signalr_netcore/signalr_client.dart';
import '../constants/api_config.dart';
import 'auth_service.dart';

class NotificationService {
  final AuthService _authService;
  HubConnection? _hubConnection;

  // Callback do powiadamiania UI o nowym powiadomieniu
  Function(Map<String, dynamic>)? onNotificationReceived;

  NotificationService(this._authService);

  Future<void> startConnection() async {
    final token = _authService.token;
    if (token == null || _hubConnection != null) return;

    final hubUrl = '${ApiConfig.baseUrl}/notificationHub';

    _hubConnection = HubConnectionBuilder()
        .withUrl(hubUrl, options: HttpConnectionOptions(
          accessTokenFactory: () async => token,
        ))
        .withAutomaticReconnect()
        .build();

    _hubConnection?.onclose(((error) => print("NotificationHub Closed: $error")) as ClosedCallback);

    // Nasłuchiwanie na powiadomienia od backendu (nazwa metody z ReservationsController)
    _hubConnection?.on('ReceiveClientNotification', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        final map = arguments[0] as Map<String, dynamic>;
        if (onNotificationReceived != null) {
          onNotificationReceived!(map);
        }
      }
    });

    try {
      await _hubConnection?.start();
      print("NotificationHub connected: ${_hubConnection?.connectionId}");
    } catch (e) {
      print("NotificationHub connection error: $e");
    }
  }

  Future<void> stopConnection() async {
    await _hubConnection?.stop();
    _hubConnection = null;
  }
}
