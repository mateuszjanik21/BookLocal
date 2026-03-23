import 'employee_models.dart';
import 'service_category_dto.dart';

class BusinessDetailDto {
  final int id;
  final String name;
  final String? nip;
  final String? city;
  final String? address;
  final String? phoneNumber;
  final String? description;
  final String? photoUrl;
  final bool isVerified;
  final double averageRating;
  final int reviewCount;
  final String? ownerFirstName;
  final String? ownerLastName;
  final List<ServiceCategoryDto> categories;
  final List<EmployeeDto> employees;

  BusinessDetailDto({
    required this.id,
    required this.name,
    this.nip,
    this.city,
    this.address,
    this.phoneNumber,
    this.description,
    this.photoUrl,
    required this.isVerified,
    required this.averageRating,
    required this.reviewCount,
    this.ownerFirstName,
    this.ownerLastName,
    required this.categories,
    required this.employees,
  });

  factory BusinessDetailDto.fromJson(Map<String, dynamic> json) {
    return BusinessDetailDto(
      id: json['id'] ?? 0,
      name: json['name'] ?? '',
      nip: json['nip'],
      city: json['city'],
      address: json['address'],
      phoneNumber: json['phoneNumber'],
      description: json['description'],
      photoUrl: json['photoUrl'],
      isVerified: json['isVerified'] ?? false,
      averageRating: (json['averageRating'] ?? 0).toDouble(),
      reviewCount: json['reviewCount'] ?? 0,
      ownerFirstName: json['owner']?['firstName'],
      ownerLastName: json['owner']?['lastName'],
      categories: (json['categories'] as List<dynamic>?)
              ?.map((c) => ServiceCategoryDto.fromJson(c))
              .toList() ??
          [],
      employees: (json['employees'] as List<dynamic>?)
              ?.map((e) => EmployeeDto.fromJson(e))
              .toList() ??
          [],
    );
  }
}
