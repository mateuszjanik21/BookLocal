import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import '../constants/api_config.dart';
import '../models/auth_models.dart';

class AuthService with ChangeNotifier {
  final _storage = const FlutterSecureStorage();
  
  UserDto? _currentUser;
  String? _token;
  bool _isAuthenticated = false;

  bool get isAuthenticated => _isAuthenticated;
  UserDto? get currentUser => _currentUser;
  String? get token => _token;
  
  Future<bool> tryAutoLogin() async {
    try {
      final storedToken = await _storage.read(key: 'jwt_token');
      final storedUserData = await _storage.read(key: 'user_data');

      if (storedToken == null || storedUserData == null) {
        return false;
      }

      _token = storedToken;
      _currentUser = UserDto.fromJson(jsonDecode(storedUserData));
      _isAuthenticated = true;
      
      notifyListeners();
      return true;
    } catch (e) {
      await logout();
      return false;
    }
  }

  Future<bool> login(String email, String password) async {
    final url = Uri.parse('${ApiConfig.baseUrl}/auth/login');

    try {
      final response = await http.post(
        url,
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode(LoginDto(email: email, password: password).toJson()),
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        final authResponse = AuthResponseDto.fromJson(data);

        _token = authResponse.token;
        _currentUser = authResponse.user;
        _isAuthenticated = true;

        await _storage.write(key: 'jwt_token', value: _token);
        await _storage.write(key: 'user_data', value: jsonEncode(data['user']));

        notifyListeners();
        return true;
      } else {
        return false;
      }
    } catch (e) { 
      return false;
    }
  }

  Future<void> logout() async {
    _token = null;
    _currentUser = null;
    _isAuthenticated = false;
    await _storage.deleteAll();
    notifyListeners();
  }
}