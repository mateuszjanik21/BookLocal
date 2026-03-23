class RebookSuggestion {
  final int serviceCategoryId;
  final String categoryName;
  final String? categoryPhotoUrl;
  final int businessId;
  final String businessName;
  final String? businessCity;
  final String employeeName;
  final String? employeePhotoUrl;
  final String lastVisitDate;
  final int visitCount;

  RebookSuggestion({
    required this.serviceCategoryId,
    required this.categoryName,
    this.categoryPhotoUrl,
    required this.businessId,
    required this.businessName,
    this.businessCity,
    required this.employeeName,
    this.employeePhotoUrl,
    required this.lastVisitDate,
    required this.visitCount,
  });

  factory RebookSuggestion.fromJson(Map<String, dynamic> json) {
    return RebookSuggestion(
      serviceCategoryId: json['serviceCategoryId'] ?? 0,
      categoryName: json['categoryName'] ?? '',
      categoryPhotoUrl: json['categoryPhotoUrl'],
      businessId: json['businessId'] ?? 0,
      businessName: json['businessName'] ?? '',
      businessCity: json['businessCity'],
      employeeName: json['employeeName'] ?? '',
      employeePhotoUrl: json['employeePhotoUrl'],
      lastVisitDate: json['lastVisitDate'] ?? '',
      visitCount: json['visitCount'] ?? 0,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'serviceCategoryId': serviceCategoryId,
      'categoryName': categoryName,
      'categoryPhotoUrl': categoryPhotoUrl,
      'businessId': businessId,
      'businessName': businessName,
      'businessCity': businessCity,
      'employeeName': employeeName,
      'employeePhotoUrl': employeePhotoUrl,
      'lastVisitDate': lastVisitDate,
      'visitCount': visitCount,
    };
  }
}
