import 'dart:convert';
import 'package:http/http.dart' as http;
import '../constants/api_config.dart';
import '../models/favorite_service_dto.dart';
import '../models/paged_result.dart';
import 'auth_service.dart';

class FavoritesService {
  final AuthService? _authService;

  FavoritesService([this._authService]); 

  Future<PagedResult<FavoriteServiceDto>?> getFavorites({int pageNumber = 1, int pageSize = 12}) async {
    final url = Uri.parse('${ApiConfig.baseUrl}/Favorites?pageNumber=$pageNumber&pageSize=$pageSize');
    final token = _authService?.token;

    try {
      final response = await http.get(
        url,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final Map<String, dynamic> jsonResponse = jsonDecode(response.body);
        return PagedResult<FavoriteServiceDto>.fromJson(
          jsonResponse,
          (json) => FavoriteServiceDto.fromJson(json),
        );
      } else {
        print("Błąd pobierania ulubionych: ${response.statusCode}");
        return null;
      }
    } catch (e) {
      print("Błąd sieci ulubionych: $e");
      return null;
    }
  }

  Future<bool> addFavorite(int serviceVariantId) async {
    final url = Uri.parse('${ApiConfig.baseUrl}/Favorites/$serviceVariantId');
    final token = _authService?.token;

    try {
      final response = await http.post(
        url,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200 || response.statusCode == 201) {
        return true;
      } else {
        print("Błąd dodawania do ulubionych: ${response.statusCode}");
        return false;
      }
    } catch (e) {
      print("Błąd sieci dodawania: $e");
      return false;
    }
  }

  Future<bool> removeFavorite(int serviceVariantId) async {
    final url = Uri.parse('${ApiConfig.baseUrl}/Favorites/$serviceVariantId');
    final token = _authService?.token;

    try {
      final response = await http.delete(
        url,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200 || response.statusCode == 204) {
        return true;
      } else {
        print("Błąd usuwania z ulubionych: ${response.statusCode}");
        return false;
      }
    } catch (e) {
      print("Błąd sieci usuwania: $e");
      return false;
    }
  }

  Future<bool> isFavorite(int serviceVariantId) async {
    final url = Uri.parse('${ApiConfig.baseUrl}/Favorites/check/$serviceVariantId');
    final token = _authService?.token;

    try {
      final response = await http.get(
        url,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        return jsonDecode(response.body) as bool;
      } else {
        return false;
      }
    } catch (e) {
      print("Błąd sieci sprawdzania ulubionych: $e");
      return false;
    }
  }
}
