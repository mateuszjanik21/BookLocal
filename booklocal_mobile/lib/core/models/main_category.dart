class MainCategory {
  final int mainCategoryId;
  final String name;

  MainCategory({
    required this.mainCategoryId,
    required this.name,
  });

  factory MainCategory.fromJson(Map<String, dynamic> json) {
    return MainCategory(
      mainCategoryId: json['mainCategoryId'] ?? 0,
      name: json['name'] ?? '',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'mainCategoryId': mainCategoryId,
      'name': name,
    };
  }
}
