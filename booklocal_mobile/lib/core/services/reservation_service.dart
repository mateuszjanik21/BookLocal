import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:intl/intl.dart';
import '../constants/api_config.dart';
import '../models/reservation_models.dart';
import 'auth_service.dart';

class ReservationService {
  final AuthService? _authService;

  ReservationService([this._authService]); 

  Future<List<String>> getAvailableSlots(int employeeId, int serviceVariantId, DateTime date) async {
    final dateStr = DateFormat('yyyy-MM-dd').format(date);
    
    final url = Uri.parse('${ApiConfig.baseUrl}/employees/$employeeId/availability?serviceVariantId=$serviceVariantId&date=$dateStr');

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

  Future<bool> createReservation(
    int serviceVariantId,
    int employeeId,
    DateTime fullDate, {
    String? discountCode,
    String paymentMethod = 'Cash',
    int loyaltyPointsUsed = 0,
  }) async {
    final url = Uri.parse('${ApiConfig.baseUrl}/reservations');
    final token = _authService?.token;
    final startTimeStr = fullDate.toUtc().toIso8601String();

    final body = <String, dynamic>{
      'serviceVariantId': serviceVariantId,
      'employeeId': employeeId,
      'startTime': startTimeStr,
      'paymentMethod': paymentMethod,
    };
    if (discountCode != null && discountCode.isNotEmpty) {
      body['discountCode'] = discountCode;
    }
    if (loyaltyPointsUsed > 0) {
      body['loyaltyPointsUsed'] = loyaltyPointsUsed;
    }

    try {
      final response = await http.post(
        url,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
        body: jsonEncode(body),
      );

      if (response.statusCode == 200 || response.statusCode == 201) {
        return true;
      } else {
        print("Błąd rezerwacji: ${response.statusCode} - ${response.body}");
        return false;
      }
    } catch (e) {
      print("Błąd sieci rezerwacji: $e");
      return false;
    }
  }

  Future<Map<String, dynamic>?> verifyDiscount({
    required int businessId,
    required String code,
    required int serviceId,
    required String customerId,
    required double originalPrice,
  }) async {
    final url = Uri.parse('${ApiConfig.baseUrl}/businesses/$businessId/discounts/verify');
    final token = _authService?.token;

    try {
      final response = await http.post(
        url,
        headers: {
          'Content-Type': 'application/json',
          if (token != null) 'Authorization': 'Bearer $token',
        },
        body: jsonEncode({
          'code': code,
          'serviceId': serviceId,
          'customerId': customerId,
          'originalPrice': originalPrice,
        }),
      );

      if (response.statusCode == 200) {
        return jsonDecode(response.body) as Map<String, dynamic>;
      }
      return null;
    } catch (e) {
      print("Błąd weryfikacji kodu: $e");
      return null;
    }
  }

  Future<int> getLoyaltyBalance({
    required int businessId,
    required String customerId,
  }) async {
    final url = Uri.parse('${ApiConfig.baseUrl}/businesses/$businessId/loyalty/customer/$customerId');
    final token = _authService?.token;

    try {
      final response = await http.get(
        url,
        headers: {
          'Content-Type': 'application/json',
          if (token != null) 'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return (data['balance']?['pointsBalance'] ?? 0) as int;
      }
      return 0;
    } catch (e) {
      print("Błąd pobierania punktów: $e");
      return 0;
    }
  }

  Future<List<ReservationDto>> getMyReservations({
    String scope = 'upcoming', 
    int pageNumber = 1, 
    int pageSize = 10,
  }) async {
    final uri = Uri.parse('${ApiConfig.baseUrl}/reservations/my-reservations').replace(queryParameters: {
      'scope': scope, 
      'pageNumber': pageNumber.toString(),
      'pageSize': pageSize.toString(), 
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

  Future<bool> submitReview(int reservationId, int rating, String comment) async {
    final url = Uri.parse('${ApiConfig.baseUrl}/reservations/$reservationId/reviews');
    final token = _authService?.token;

    try {
      final response = await http.post(
        url,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
        body: jsonEncode({
          'rating': rating,
          'comment': comment,
        }),
      );

      if (response.statusCode == 200 || response.statusCode == 201) {
        return true;
      } else {
        print("Błąd wysyłania opinii: ${response.statusCode} - ${response.body}");
        return false;
      }
    } catch (e) {
      print("Błąd sieci (review): $e");
      return false;
    }
  }
}