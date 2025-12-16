class EmployeeDto {
  final int id;
  final String firstName;
  final String lastName;
  final String? position;
  final String? photoUrl;

  EmployeeDto({
    required this.id,
    required this.firstName,
    required this.lastName,
    this.position,
    this.photoUrl,
  });

  String get fullName => "$firstName $lastName";

  factory EmployeeDto.fromJson(Map<String, dynamic> json) {
    return EmployeeDto(
      id: json['id'] ?? 0,
      firstName: json['firstName'] ?? '',
      lastName: json['lastName'] ?? '',
      position: json['position'],
      photoUrl: json['photoUrl'],
    );
  }
}