class ReservationDto {
  final int reservationId;
  final String businessName;
  final String serviceName;
  final String employeeName;
  final DateTime date;
  final double price; 
  final String status;
  final bool hasReview;

  ReservationDto({
    required this.reservationId,
    required this.businessName,
    required this.serviceName,
    required this.employeeName,
    required this.date,
    required this.price,
    required this.status,
    required this.hasReview,
  });

  bool get isUpcoming {
    return status != 'cancelled' && status != 'completed'; 
  }

  factory ReservationDto.fromJson(Map<String, dynamic> json) {
    return ReservationDto(
      reservationId: json['reservationId'] ?? 0,
      businessName: json['businessName'] ?? 'Firma',
      serviceName: json['serviceName'] ?? 'Usługa',
      employeeName: json['employeeFullName'] ?? '', 
      hasReview: json['hasReview'] ?? false,
      
      date: DateTime.tryParse(json['startTime'] ?? '')?.toLocal() ?? DateTime.now(),
      
      price: (json['price'] ?? 0).toDouble(), 
      
      status: json['status']?.toString().toLowerCase() ?? 'pending',
    );
  }
}