import 'service.dart';

class ServiceCategorySearchResult {
  final int serviceCategoryId;
  final String name;
  final String? photoUrl;
  final int businessId;
  final String businessName;
  final String? businessCity;
  final String? mainCategoryName;
  final double averageRating;
  final int reviewCount;
  final String businessCreatedAt;
  final List<Service> services;

  ServiceCategorySearchResult({
    required this.serviceCategoryId,
    required this.name,
    this.photoUrl,
    required this.businessId,
    required this.businessName,
    this.businessCity,
    this.mainCategoryName,
    required this.averageRating,
    required this.reviewCount,
    required this.businessCreatedAt,
    required this.services,
  });

  factory ServiceCategorySearchResult.fromJson(Map<String, dynamic> json) {
    return ServiceCategorySearchResult(
      serviceCategoryId: json['serviceCategoryId'] ?? 0,
      name: json['name'] ?? '',
      photoUrl: json['photoUrl'],
      businessId: json['businessId'] ?? 0,
      businessName: json['businessName'] ?? '',
      businessCity: json['businessCity'],
      mainCategoryName: json['mainCategoryName'],
      averageRating: (json['averageRating'] ?? 0).toDouble(),
      reviewCount: json['reviewCount'] ?? 0,
      businessCreatedAt: json['businessCreatedAt'] ?? '',
      services: (json['services'] as List<dynamic>?)
              ?.map((s) => Service.fromJson(s))
              .toList() ??
          [],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'serviceCategoryId': serviceCategoryId,
      'name': name,
      'photoUrl': photoUrl,
      'businessId': businessId,
      'businessName': businessName,
      'businessCity': businessCity,
      'mainCategoryName': mainCategoryName,
      'averageRating': averageRating,
      'reviewCount': reviewCount,
      'businessCreatedAt': businessCreatedAt,
      'services': services.map((s) => s.toJson()).toList(),
    };
  }
}
