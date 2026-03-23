// ignore_for_file: avoid_print

import 'dart:convert';
import 'package:http/http.dart' as http;
import '../constants/api_config.dart';
import '../models/service_category_search_result.dart';
import '../models/rebook_suggestion.dart';
import '../models/main_category.dart';
import '../models/paged_result.dart';

class SearchService {
  Future<PagedResult<ServiceCategorySearchResult>> searchCategoryFeed({
    String? searchTerm,
    String? locationTerm,
    int? mainCategoryId,
    String? sortBy,
    required int pageNumber,
    required int pageSize,
  }) async {
    try {
      final queryParams = <String, String>{
        'pageNumber': pageNumber.toString(),
        'pageSize': pageSize.toString(),
      };

      if (searchTerm != null && searchTerm.isNotEmpty) {
        queryParams['searchTerm'] = searchTerm;
      }
      if (locationTerm != null && locationTerm.isNotEmpty) {
        queryParams['locationTerm'] = locationTerm;
      }
      if (mainCategoryId != null) {
        queryParams['mainCategoryId'] = mainCategoryId.toString();
      }
      if (sortBy != null && sortBy.isNotEmpty) {
        queryParams['sortBy'] = sortBy;
      }

      final uri = Uri.parse('${ApiConfig.baseUrl}/search/category-feed')
          .replace(queryParameters: queryParams);

      final response = await http.get(uri);

      if (response.statusCode == 200) {
        final Map<String, dynamic> data = jsonDecode(response.body);
        return PagedResult.fromJson(
          data,
          (json) => ServiceCategorySearchResult.fromJson(json),
        );
      } else {
        print('Błąd API searchCategoryFeed: ${response.statusCode}');
        return PagedResult(
          items: [],
          totalCount: 0,
          pageNumber: pageNumber,
          pageSize: pageSize,
          totalPages: 0,
        );
      }
    } catch (e) {
      print('Błąd searchCategoryFeed: $e');
      return PagedResult(
        items: [],
        totalCount: 0,
        pageNumber: pageNumber,
        pageSize: pageSize,
        totalPages: 0,
      );
    }
  }

  Future<List<RebookSuggestion>> getRebookSuggestions(String? token) async {
    if (token == null || token.isEmpty) {
      return [];
    }

    try {
      final uri = Uri.parse('${ApiConfig.baseUrl}/search/rebook-suggestions');
      final response = await http.get(
        uri,
        headers: {
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final List<dynamic> data = jsonDecode(response.body);
        return data.map((json) => RebookSuggestion.fromJson(json)).toList();
      } else {
        print('Błąd API getRebookSuggestions: ${response.statusCode}');
        return [];
      }
    } catch (e) {
      print('Błąd getRebookSuggestions: $e');
      return [];
    }
  }

  Future<List<MainCategory>> getMainCategories() async {
    try {
      final uri = Uri.parse('${ApiConfig.baseUrl}/maincategories');
      final response = await http.get(uri);

      if (response.statusCode == 200) {
        final List<dynamic> data = jsonDecode(response.body);
        return data.map((json) => MainCategory.fromJson(json)).toList();
      } else {
        print('Błąd API getMainCategories: ${response.statusCode}');
        return [];
      }
    } catch (e) {
      print('Błąd getMainCategories: $e');
      return [];
    }
  }
}
