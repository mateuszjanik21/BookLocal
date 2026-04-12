import 'dart:convert';
import 'package:http/http.dart' as http;
import '../constants/api_config.dart';
import '../models/service_bundle_dto.dart';
import 'auth_service.dart';

class ServiceBundleService {
  final AuthService? _authService;

  ServiceBundleService([this._authService]);

  Map<String, String> get _headers => {
        'Content-Type': 'application/json',
        if (_authService?.token != null)
          'Authorization': 'Bearer ${_authService!.token}',
      };

  Future<List<ServiceBundleDto>> getBundles(int businessId) async {
    final url = Uri.parse(
      '${ApiConfig.baseUrl}/businesses/$businessId/bundles',
    );
    try {
      final response = await http.get(url, headers: _headers);
      if (response.statusCode == 200) {
        final List<dynamic> data = jsonDecode(response.body);
        return data.map((json) => ServiceBundleDto.fromJson(json)).toList();
      }
      return [];
    } catch (e) {
      print("Błąd pobierania pakietów: $e");
      return [];
    }
  }

  Future<List<String>> getBundleAvailableSlots(
    int employeeId,
    String date,
    int bundleId,
  ) async {
    final url = Uri.parse(
      '${ApiConfig.baseUrl}/employees/$employeeId/availability/bundle',
    ).replace(queryParameters: {
      'date': date,
      'bundleId': bundleId.toString(),
    });
    try {
      final response = await http.get(url, headers: _headers);
      if (response.statusCode == 200) {
        final List<dynamic> data = jsonDecode(response.body);
        return data.map((e) => e.toString()).toList();
      }
      print("Błąd slotów pakietu: ${response.statusCode} - ${response.body}");
      return [];
    } catch (e) {
      print("Błąd pobierania slotów pakietu: $e");
      return [];
    }
  }

  Future<bool> createBundleReservation({
    required int serviceBundleId,
    required int employeeId,
    required DateTime startTime,
    required String paymentMethod,
    int loyaltyPointsUsed = 0,
  }) async {
    final url = Uri.parse('${ApiConfig.baseUrl}/reservations/bundle');
    final startTimeStr = startTime.toUtc().toIso8601String();
    final payload = {
      'serviceBundleId': serviceBundleId,
      'employeeId': employeeId,
      'startTime': startTimeStr,
      'paymentMethod': paymentMethod,
      if (loyaltyPointsUsed > 0) 'loyaltyPointsUsed': loyaltyPointsUsed,
    };
    print("Bundle reservation payload: $payload");
    try {
      final response = await http.post(
        url,
        headers: _headers,
        body: jsonEncode(payload),
      );
      print("Bundle reservation response: ${response.statusCode} - ${response.body}");
      if (response.statusCode == 200 || response.statusCode == 201) {
        return true;
      }
      return false;
    } catch (e) {
      print("Błąd tworzenia rezerwacji pakietu: $e");
      return false;
    }
  }
}
