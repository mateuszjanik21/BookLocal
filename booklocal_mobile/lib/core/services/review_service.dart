import 'dart:convert';
import 'package:booklocal_mobile/core/services/auth_service.dart';
import 'package:http/http.dart' as http;
import '../constants/api_config.dart';
import '../models/review_models.dart';

class ReviewService {
  final AuthService? _authService;
  ReviewService([this._authService]);
  
  Future<PagedReviewsResult> getReviews(int businessId, {int pageNumber = 1, int pageSize = 5}) async {
    final uri = Uri.parse('${ApiConfig.baseUrl}/businesses/$businessId/reviews').replace(queryParameters: {
      'pageNumber': pageNumber.toString(),
      'pageSize': pageSize.toString(),
    });

    try {
      final response = await http.get(uri);

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return PagedReviewsResult.fromJson(data);
      } else {
        print("Błąd pobierania opinii: ${response.statusCode}");
        return PagedReviewsResult(items: [], totalCount: 0);
      }
    } catch (e) {
      print("Błąd sieci (opinie): $e");
      return PagedReviewsResult(items: [], totalCount: 0);
    }
  }

  Future<bool> addReview(int reservationId, int rating, String comment) async {
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
        print("Błąd dodawania opinii: ${response.body}");
        return false;
      }
    } catch (e) {
      print("Błąd sieci (addReview): $e");
      return false;
    }
  }
}