class ServiceVariant {
  final int serviceVariantId;
  final String name;
  final double price;
  final int durationMinutes;
  final int cleanupTimeMinutes;
  final bool isDefault;
  final bool isActive;
  final int favoritesCount;

  ServiceVariant({
    required this.serviceVariantId,
    required this.name,
    required this.price,
    required this.durationMinutes,
    required this.cleanupTimeMinutes,
    required this.isDefault,
    required this.isActive,
    required this.favoritesCount,
  });

  factory ServiceVariant.fromJson(Map<String, dynamic> json) {
    return ServiceVariant(
      serviceVariantId: json['serviceVariantId'] ?? 0,
      name: json['name'] ?? '',
      price: (json['price'] ?? 0).toDouble(),
      durationMinutes: json['durationMinutes'] ?? 0,
      cleanupTimeMinutes: json['cleanupTimeMinutes'] ?? 0,
      isDefault: json['isDefault'] ?? false,
      isActive: json['isActive'] ?? true,
      favoritesCount: json['favoritesCount'] ?? 0,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'serviceVariantId': serviceVariantId,
      'name': name,
      'price': price,
      'durationMinutes': durationMinutes,
      'cleanupTimeMinutes': cleanupTimeMinutes,
      'isDefault': isDefault,
      'isActive': isActive,
      'favoritesCount': favoritesCount,
    };
  }
}
