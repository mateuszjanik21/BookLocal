import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import '../../../../../core/constants/api_config.dart';
import '../../../../../core/services/auth_service.dart';

class ProfileProvider extends ChangeNotifier {
  final AuthService _authService;

  Map<String, dynamic>? _stats;
  bool _isLoadingStats = true;

  ProfileProvider(this._authService);

  Map<String, dynamic>? get stats => _stats;
  bool get isLoadingStats => _isLoadingStats;

  Future<void> loadStats() async {
    final token = _authService.token;
    if (token == null) {
      _isLoadingStats = false;
      notifyListeners();
      return;
    }

    _isLoadingStats = true;
    notifyListeners();

    try {
      final response = await http.get(
        Uri.parse('${ApiConfig.baseUrl}/reservations/my-stats'),
        headers: {'Authorization': 'Bearer $token'},
      );

      if (response.statusCode == 200) {
        _stats = jsonDecode(response.body);
      }
    } catch (e) {
      print('[ProfileProvider] Błąd pobierania statystyk: $e');
    } finally {
      _isLoadingStats = false;
      notifyListeners();
    }
  }
}
