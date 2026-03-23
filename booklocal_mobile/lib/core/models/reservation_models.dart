class ReservationDto {
  final int reservationId;
  final String businessName;
  final String serviceName;
  final String employeeName; // Dodano (employeeFullName w TS)
  final DateTime date;
  final double price; // Backend tego nie zwraca wprost w modelu TS, damy 0.0 lub pobierzemy jeśli jest w DTO backendu
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

  // Helper do UI
  bool get isUpcoming {
    // Logika frontendowa, ale backend i tak filtruje przez 'scope'
    return status != 'cancelled' && status != 'completed'; 
  }

  factory ReservationDto.fromJson(Map<String, dynamic> json) {
    return ReservationDto(
      reservationId: json['reservationId'] ?? 0,
      businessName: json['businessName'] ?? 'Firma',
      serviceName: json['serviceName'] ?? 'Usługa',
      employeeName: json['employeeFullName'] ?? '', 
      hasReview: json['hasReview'] ?? false,
      
      // Backend zwraca 'startTime' w ISO
      date: DateTime.tryParse(json['startTime'] ?? '')?.toLocal() ?? DateTime.now(),
      
      // Angular Model nie ma pola price, więc pewnie jest w innej strukturze lub backend go nie śle.
      // Ustawiam 0, żeby kod się nie wywalał. Możemy to ukryć w widoku jeśli 0.
      price: (json['price'] ?? 0).toDouble(), 
      
      status: json['status']?.toString().toLowerCase() ?? 'pending',
    );
  }
}