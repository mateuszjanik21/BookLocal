import 'package:flutter/foundation.dart';
import '../services/notification_service.dart';
import '../services/auth_service.dart';

class NotificationPayload {
  final String message;
  final int? reservationId;
  final String? status;
  final String? businessName;
  final DateTime receivedAt;

  NotificationPayload({
    required this.message,
    this.reservationId,
    this.status,
    this.businessName,
    DateTime? receivedAt,
  }) : receivedAt = receivedAt ?? DateTime.now();

  factory NotificationPayload.fromMap(Map<String, dynamic> map) {
    return NotificationPayload(
      message: map['message'] ?? map['Message'] ?? '',
      reservationId: map['reservationId'] ?? map['ReservationId'],
      status: map['status'] ?? map['Status'],
      businessName: map['businessName'] ?? map['BusinessName'],
    );
  }
}

class NotificationProvider with ChangeNotifier {
  final NotificationService _notificationService;
  final List<NotificationPayload> _notifications = [];
  int _unreadCount = 0;

  List<NotificationPayload> get notifications => _notifications;
  int get unreadCount => _unreadCount;
  bool get hasUnread => _unreadCount > 0;

  NotificationProvider(AuthService authService)
      : _notificationService = NotificationService(authService) {
    _notificationService.onNotificationReceived = _handleNotification;
  }

  Future<void> connect() async {
    await _notificationService.startConnection();
  }

  Future<void> disconnect() async {
    await _notificationService.stopConnection();
  }

  void _handleNotification(Map<String, dynamic> data) {
    final payload = NotificationPayload.fromMap(data);
    _notifications.insert(0, payload); // Najnowsze na górze
    _unreadCount++;
    notifyListeners();
  }

  void markAllAsRead() {
    _unreadCount = 0;
    notifyListeners();
  }

  void clearAll() {
    _notifications.clear();
    _unreadCount = 0;
    notifyListeners();
  }
}
