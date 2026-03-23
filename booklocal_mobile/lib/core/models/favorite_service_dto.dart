class FavoriteServiceDto {
  final int serviceVariantId;
  final String serviceName;
  final String variantName;
  final double price;
  final int durationMinutes;
  final int businessId;
  final String businessName;
  final String businessCity;
  final String? businessPhotoUrl;
  final bool isActive;
  final bool isServiceArchived;

  FavoriteServiceDto({
    required this.serviceVariantId,
    required this.serviceName,
    required this.variantName,
    required this.price,
    required this.durationMinutes,
    required this.businessId,
    required this.businessName,
    required this.businessCity,
    this.businessPhotoUrl,
    required this.isActive,
    required this.isServiceArchived,
  });

  factory FavoriteServiceDto.fromJson(Map<String, dynamic> json) {
    return FavoriteServiceDto(
      serviceVariantId: json['serviceVariantId'],
      serviceName: json['serviceName'],
      variantName: json['variantName'],
      price: json['price']?.toDouble() ?? 0.0,
      durationMinutes: json['durationMinutes'],
      businessId: json['businessId'],
      businessName: json['businessName'],
      businessCity: json['businessCity'],
      businessPhotoUrl: json['businessPhotoUrl'],
      isActive: json['isActive'] ?? true,
      isServiceArchived: json['isServiceArchived'] ?? false,
    );
  }
}
