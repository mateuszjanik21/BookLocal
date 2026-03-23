import 'dart:convert';
import 'package:http/http.dart' as http;
import '../constants/api_config.dart';
import '../models/service_bundle_dto.dart';

class ServiceBundleService {
  Future<List<ServiceBundleDto>> getBundles(int businessId) async {
    final url = Uri.parse('${ApiConfig.baseUrl}/businesses/$businessId/bundles');
    try {
      final response = await http.get(url);
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
}
