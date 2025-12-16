class ReviewDto {
  final int reviewId;
  final String reviewerName; // Zmiana z userName
  final String? reviewerPhotoUrl; // Zmiana z userPhotoUrl
  final int rating;
  final String comment; // Zmiana z content
  final DateTime createdAt;

  ReviewDto({
    required this.reviewId,
    required this.reviewerName,
    this.reviewerPhotoUrl,
    required this.rating,
    required this.comment,
    required this.createdAt,
  });

  factory ReviewDto.fromJson(Map<String, dynamic> json) {
    return ReviewDto(
      reviewId: json['reviewId'] ?? 0,
      // Mapowanie nazw z backendu (Angular: reviewerName)
      reviewerName: json['reviewerName'] ?? 'Anonim',
      reviewerPhotoUrl: json['reviewerPhotoUrl'],
      rating: json['rating'] ?? 5,
      // Mapowanie nazw z backendu (Angular: comment)
      comment: json['comment'] ?? '',
      createdAt: DateTime.tryParse(json['createdAt'] ?? '') ?? DateTime.now(),
    );
  }
}

// Model do stronicowania (zostaje bez zmian logicznych, tylko typ generyczny)
class PagedReviewsResult {
  final List<ReviewDto> items;
  final int totalCount;

  PagedReviewsResult({required this.items, required this.totalCount});

  factory PagedReviewsResult.fromJson(Map<String, dynamic> json) {
    final itemsJson = json['items'] as List<dynamic>? ?? [];
    final items = itemsJson.map((e) => ReviewDto.fromJson(e)).toList();
    return PagedReviewsResult(
      items: items,
      totalCount: json['totalCount'] ?? 0,
    );
  }
}