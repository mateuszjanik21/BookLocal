class BundleSubItemDto {
  final int reservationId;
  final String serviceName;
  final String variantName;

  BundleSubItemDto({
    required this.reservationId,
    required this.serviceName,
    required this.variantName,
  });

  factory BundleSubItemDto.fromJson(Map<String, dynamic> json) {
    return BundleSubItemDto(
      reservationId: json['reservationId'] ?? 0,
      serviceName: json['serviceName'] ?? '',
      variantName: json['variantName'] ?? '',
    );
  }
}

class ReservationDto {
  final int reservationId;
  final int businessId;
  final String businessName;
  final String serviceName;
  final String variantName;
  final int employeeId;
  final String employeeName;
  final String? employeePhotoUrl;
  final DateTime startTime;
  final DateTime endTime;
  final double agreedPrice;
  final String status;
  final bool hasReview;
  final int? serviceBundleId;
  final String? bundleName;
  final bool isBundle;
  final int loyaltyPointsUsed;
  final String paymentMethod;
  final List<BundleSubItemDto>? subItems;

  ReservationDto({
    required this.reservationId,
    required this.businessId,
    required this.businessName,
    required this.serviceName,
    required this.variantName,
    required this.employeeId,
    required this.employeeName,
    this.employeePhotoUrl,
    required this.startTime,
    required this.endTime,
    required this.agreedPrice,
    required this.status,
    required this.hasReview,
    this.serviceBundleId,
    this.bundleName,
    this.isBundle = false,
    this.loyaltyPointsUsed = 0,
    this.paymentMethod = 'Cash',
    this.subItems,
  });

  bool get isUpcoming {
    return status != 'cancelled' && status != 'completed'; 
  }

  factory ReservationDto.fromJson(Map<String, dynamic> json) {
    return ReservationDto(
      reservationId: json['reservationId'] ?? 0,
      businessId: json['businessId'] ?? 0,
      businessName: json['businessName'] ?? 'Firma',
      serviceName: json['serviceName'] ?? 'Usługa',
      variantName: json['variantName'] ?? '',
      employeeId: json['employeeId'] ?? 0,
      employeeName: json['employeeFullName'] ?? '',
      employeePhotoUrl: json['employeePhotoUrl'],
      startTime: DateTime.tryParse(json['startTime'] ?? '')?.toLocal() ?? DateTime.now(),
      endTime: DateTime.tryParse(json['endTime'] ?? '')?.toLocal() ?? DateTime.now(),
      agreedPrice: (json['agreedPrice'] ?? 0).toDouble(),
      status: json['status']?.toString().toLowerCase() ?? 'pending',
      hasReview: json['hasReview'] ?? false,
      serviceBundleId: json['serviceBundleId'],
      bundleName: json['bundleName'],
      isBundle: json['isBundle'] ?? false,
      loyaltyPointsUsed: json['loyaltyPointsUsed'] ?? 0,
      paymentMethod: json['paymentMethod'] ?? 'Cash',
      subItems: (json['subItems'] as List<dynamic>?)
          ?.map((e) => BundleSubItemDto.fromJson(e))
          .toList(),
    );
  }

  // Kompatybilność wsteczna
  DateTime get date => startTime;
  double get price => agreedPrice;
}