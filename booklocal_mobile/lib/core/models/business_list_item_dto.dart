class BusinessListItemDto {
  final int id;
  final String name;
  final String category;
  final String city;
  final String? photoUrl;
  final double rating;
  final int reviewCount;

  BusinessListItemDto({
    required this.id,
    required this.name,
    required this.category,
    required this.city,
    this.photoUrl,
    required this.rating,
    required this.reviewCount,
  });

  factory BusinessListItemDto.fromJson(Map<String, dynamic> json) {
    String categoryName = 'Inne';
    
    if (json['mainCategories'] != null && json['mainCategories'] is List) {
      final list = json['mainCategories'] as List;
      if (list.isNotEmpty) {
        categoryName = list.first.toString();
      }
    }

    return BusinessListItemDto(
      id: json['businessId'] ?? json['id'] ?? 0, 
      name: json['name'] ?? 'Bez nazwy',
      category: categoryName,
      city: json['city'] ?? '',
      photoUrl: json['photoUrl'],
      rating: (json['averageRating'] ?? 0).toDouble(),
      reviewCount: json['reviewCount'] ?? 0
    );
  }
}