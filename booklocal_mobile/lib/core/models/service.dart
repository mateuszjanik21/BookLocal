import 'service_variant.dart';

class Service {
  final int id;
  final String name;
  final String? description;
  final int serviceCategoryId;
  final int businessId;
  final bool isArchived;
  final List<ServiceVariant> variants;

  Service({
    required this.id,
    required this.name,
    this.description,
    required this.serviceCategoryId,
    required this.businessId,
    required this.isArchived,
    required this.variants,
  });

  factory Service.fromJson(Map<String, dynamic> json) {
    return Service(
      id: json['id'] ?? 0,
      name: json['name'] ?? '',
      description: json['description'],
      serviceCategoryId: json['serviceCategoryId'] ?? 0,
      businessId: json['businessId'] ?? 0,
      isArchived: json['isArchived'] ?? false,
      variants: (json['variants'] as List<dynamic>?)
              ?.map((v) => ServiceVariant.fromJson(v))
              .toList() ??
          [],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'description': description,
      'serviceCategoryId': serviceCategoryId,
      'businessId': businessId,
      'isArchived': isArchived,
      'variants': variants.map((v) => v.toJson()).toList(),
    };
  }
}
