// ignore_for_file: avoid_print

import 'dart:convert';
import 'package:booklocal_mobile/core/models/employee_models.dart';
import 'package:booklocal_mobile/core/models/service_models.dart';
import 'package:http/http.dart' as http;
import '../constants/api_config.dart';
import '../models/business_list_item_dto.dart';

class ClientService {
  Future<List<ServiceDto>> getBusinessServices(int businessId) async {
    final url = Uri.parse('${ApiConfig.baseUrl}/businesses/$businessId');

    try {
      final response = await http.get(url);

      if (response.statusCode == 200) {
        final Map<String, dynamic> data = jsonDecode(response.body);
        List<ServiceDto> allServices = [];

        // 1. Sprawdź listę główną 'services' (jeśli backend tam coś wrzuca)
        if (data['services'] != null) {
          final List<dynamic> servicesJson = data['services'];
          allServices.addAll(servicesJson.map((json) => ServiceDto.fromJson(json)));
        }

        // 2. Sprawdź listę 'categories' i wyciągnij usługi z każdej kategorii
        // To jest kluczowe, bo Angular korzysta właśnie z tego pola
        if (data['categories'] != null) {
          final List<dynamic> categoriesJson = data['categories'];
          
          for (var cat in categoriesJson) {
            // Sprawdź czy kategoria ma usługi
            if (cat['services'] != null) {
              final List<dynamic> catServices = cat['services'];
              // Dodaj usługi z tej kategorii do naszej głównej listy
              allServices.addAll(catServices.map((json) => ServiceDto.fromJson(json)));
            }
          }
        }
        
        // Opcjonalnie: Usuń duplikaty (jeśli usługa jest i w 'services' i w 'categories')
        final ids = <int>{};
        allServices.retainWhere((x) => ids.add(x.id));

        return allServices;
      }
      return [];
    } catch (e) {
      print("Błąd pobierania usług firmy: $e");
      return [];
    }
  }
  
  Future<List<EmployeeDto>> getEmployeesForService(int businessId, int serviceId) async {
    final url = Uri.parse('${ApiConfig.baseUrl}/businesses/$businessId/services/$serviceId/employees');
    
    try {
      final response = await http.get(url);
      if (response.statusCode == 200) {
        final List<dynamic> data = jsonDecode(response.body);
        return data.map((json) => EmployeeDto.fromJson(json)).toList();
      }
      return [];
    } catch (e) {
      print("Błąd pobierania pracowników: $e");
      return [];
    }
  }

  Future<List<BusinessListItemDto>> getBusinesses() async {
    // ZMIANA: Używamy endpointu do wyszukiwania, bo on zwraca kategorie!
    // Parametry pageNumber i pageSize są wymagane przez Twój backend
    final url = Uri.parse('${ApiConfig.baseUrl}/search/businesses?pageNumber=1&pageSize=100');

    try {
      final response = await http.get(url);

      if (response.statusCode == 200) {
        final Map<String, dynamic> jsonResponse = jsonDecode(response.body);
        
        // Twój endpoint zwraca PagedResult, więc lista firm jest w polu 'items'
        if (jsonResponse.containsKey('items') && jsonResponse['items'] is List) {
          final List<dynamic> items = jsonResponse['items'];
          return items.map((json) => BusinessListItemDto.fromJson(json)).toList();
        } else {
          return [];
        }
      } else {
        print('Błąd API: ${response.statusCode}');
        return [];
      }
    } catch (e) {
      print('Błąd połączenia: $e');
      return [];
    }
  }
}