import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:intl/intl.dart';
import '../constants/api_config.dart';
import '../models/reservation_models.dart';
import 'auth_service.dart';

class ReservationService {
  final AuthService? _authService;

  ReservationService([this._authService]); 

  Future<List<String>> getAvailableSlots(int employeeId, int serviceId, DateTime date) async {
    final dateStr = DateFormat('yyyy-MM-dd').format(date);
    
    final url = Uri.parse('${ApiConfig.baseUrl}/employees/$employeeId/availability?serviceId=$serviceId&date=$dateStr');

    try {
      final response = await http.get(url);
      if (response.statusCode == 200) {
        final List<dynamic> data = jsonDecode(response.body);
        return data.map((e) => e.toString()).toList();
      }
      print("Błąd slotów: ${response.statusCode} - ${response.body}");
      return [];
    } catch (e) {
      print("Błąd sieci slotów: $e");
      return [];
    }
  }

  Future<bool> createReservation(int serviceId, int employeeId, DateTime fullDate) async {
    final url = Uri.parse('${ApiConfig.baseUrl}/reservations');
    final token = _authService?.token;
    final startTimeStr = fullDate.toUtc().toIso8601String();

    try {
      final response = await http.post(
        url,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
        body: jsonEncode({
          'serviceId': serviceId,
          'employeeId': employeeId,
          'startTime': startTimeStr,
        }),
      );

      if (response.statusCode == 200 || response.statusCode == 201) {
        return true;
      } else {
        return false;
      }
    } catch (e) {
      print("❌ Błąd sieci rezerwacji: $e");
      return false;
    }
  }

  Future<List<ReservationDto>> getMyReservations({String scope = 'upcoming'}) async {
    final uri = Uri.parse('${ApiConfig.baseUrl}/reservations/my-reservations').replace(queryParameters: {
      'scope': scope,
      'pageNumber': '1',
      'pageSize': '50',
    });

    final token = _authService?.token;

    try {
      final response = await http.get(
        uri,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final Map<String, dynamic> jsonResponse = jsonDecode(response.body);
        
        if (jsonResponse.containsKey('items') && jsonResponse['items'] is List) {
          final List<dynamic> items = jsonResponse['items'];
          return items.map((json) => ReservationDto.fromJson(json)).toList();
        }
        return [];
      } else {
        print("Błąd pobierania wizyt ($scope): ${response.statusCode}");
        return [];
      }
    } catch (e) {
      print("Błąd sieci ($scope): $e");
      return [];
    }
  }

  Future<bool> cancelReservation(int reservationId) async {
    final url = Uri.parse('${ApiConfig.baseUrl}/reservations/my-reservations/$reservationId/cancel');
    final token = _authService?.token;

    try {
      final response = await http.patch(
        url,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
        body: jsonEncode({}), 
      );

      if (response.statusCode == 200 || response.statusCode == 204) {
        return true;
      } else {
        print("Błąd anulowania: ${response.statusCode} - ${response.body}");
        return false;
      }
    } catch (e) {
      print("Błąd sieci (cancel): $e");
      return false;
    }
  }
}