class ServiceDto {
  final int id;
  final String name;
  final String description;
  final double price;
  final int durationMinutes;

  ServiceDto({
    required this.id,
    required this.name,
    required this.description,
    required this.price,
    required this.durationMinutes,
  });

  factory ServiceDto.fromJson(Map<String, dynamic> json) {
    return ServiceDto(
      id: json['id'] ?? 0,
      name: json['name'] ?? 'Usługa',
      description: json['description'] ?? '',
      price: (json['price'] ?? 0).toDouble(),
      durationMinutes: json['durationMinutes'] ?? 30,
    );
  }
}