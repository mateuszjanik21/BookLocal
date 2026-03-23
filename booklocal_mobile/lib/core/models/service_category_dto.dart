import 'service.dart';

class ServiceCategoryDto {
  final int serviceCategoryId;
  final String name;
  final String? photoUrl;
  final bool isArchived;
  final List<Service> services;

  ServiceCategoryDto({
    required this.serviceCategoryId,
    required this.name,
    this.photoUrl,
    required this.isArchived,
    required this.services,
  });

  factory ServiceCategoryDto.fromJson(Map<String, dynamic> json) {
    return ServiceCategoryDto(
      serviceCategoryId: json['serviceCategoryId'] ?? 0,
      name: json['name'] ?? '',
      photoUrl: json['photoUrl'],
      isArchived: json['isArchived'] ?? false,
      services: (json['services'] as List<dynamic>?)
              ?.map((s) => Service.fromJson(s))
              .toList() ??
          [],
    );
  }
}
