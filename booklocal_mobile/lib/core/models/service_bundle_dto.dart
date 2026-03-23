class ServiceBundleItemDto {
  final int serviceBundleItemId;
  final int serviceVariantId;
  final String variantName;
  final String serviceName;
  final int durationMinutes;
  final int sequenceOrder;
  final double originalPrice;

  ServiceBundleItemDto({
    required this.serviceBundleItemId,
    required this.serviceVariantId,
    required this.variantName,
    required this.serviceName,
    required this.durationMinutes,
    required this.sequenceOrder,
    required this.originalPrice,
  });

  factory ServiceBundleItemDto.fromJson(Map<String, dynamic> json) {
    return ServiceBundleItemDto(
      serviceBundleItemId: json['serviceBundleItemId'] ?? 0,
      serviceVariantId: json['serviceVariantId'] ?? 0,
      variantName: json['variantName'] ?? '',
      serviceName: json['serviceName'] ?? '',
      durationMinutes: json['durationMinutes'] ?? 0,
      sequenceOrder: json['sequenceOrder'] ?? 0,
      originalPrice: (json['originalPrice'] ?? 0).toDouble(),
    );
  }
}

class ServiceBundleDto {
  final int serviceBundleId;
  final int businessId;
  final String name;
  final String? description;
  final double totalPrice;
  final String? photoUrl;
  final bool isActive;
  final List<ServiceBundleItemDto> items;

  ServiceBundleDto({
    required this.serviceBundleId,
    required this.businessId,
    required this.name,
    this.description,
    required this.totalPrice,
    this.photoUrl,
    required this.isActive,
    required this.items,
  });

  /// Suma oryginalnych cen wszystkich pozycji
  double get originalTotalPrice =>
      items.fold(0.0, (sum, item) => sum + item.originalPrice);

  /// Procent zniżki (np. 20%)
  int get discountPercent {
    final original = originalTotalPrice;
    if (original <= 0) return 0;
    return ((1 - totalPrice / original) * 100).round();
  }

  /// Łączny czas trwania (suma minut)
  int get totalDurationMinutes =>
      items.fold(0, (sum, item) => sum + item.durationMinutes);

  factory ServiceBundleDto.fromJson(Map<String, dynamic> json) {
    return ServiceBundleDto(
      serviceBundleId: json['serviceBundleId'] ?? 0,
      businessId: json['businessId'] ?? 0,
      name: json['name'] ?? '',
      description: json['description'],
      totalPrice: (json['totalPrice'] ?? 0).toDouble(),
      photoUrl: json['photoUrl'],
      isActive: json['isActive'] ?? true,
      items: (json['items'] as List<dynamic>?)
              ?.map((i) => ServiceBundleItemDto.fromJson(i))
              .toList() ??
          [],
    );
  }
}
