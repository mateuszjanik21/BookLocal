import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/home_provider.dart';
import 'package:shimmer/shimmer.dart';

class CategoryButtons extends StatelessWidget {
  const CategoryButtons({super.key});

  IconData _getIconForCategory(String name) {
    final lowerName = name.toLowerCase();
    if (lowerName.contains('fryzjer') || lowerName.contains('włosy')) {
      return Icons.content_cut;
    } else if (lowerName.contains('masaż') || lowerName.contains('fizjo')) {
      return Icons.spa;
    } else if (lowerName.contains('kosmety') || lowerName.contains('paznok')) {
      return Icons.brush;
    } else if (lowerName.contains('tatu') || lowerName.contains('tattoo')) {
      return Icons.color_lens;
    } else if (lowerName.contains('barber')) {
      return Icons.face;
    }
    return Icons.star_border; // Domyślna ikona
  }

  @override
  Widget build(BuildContext context) {
    final homeProvider = Provider.of<HomeProvider>(context);
    final categories = homeProvider.mainCategories;

    return AnimatedSwitcher(
      duration: const Duration(milliseconds: 300),
      child: homeProvider.isCategoryLoading
          ? _buildSkeleton(context)
          : Container(
              key: const ValueKey('category_loaded'),
              height: 95,
              margin: const EdgeInsets.only(top: 16.0, bottom: 4.0),
              child: ListView.builder(
                scrollDirection: Axis.horizontal,
                padding: const EdgeInsets.symmetric(horizontal: 16.0),
                itemCount: categories.length + 1, // Zawsze dodajemy przycisk "Wszystkie"
                itemBuilder: (context, index) {
                  if (index == 0) {
                    final isSelected = homeProvider.activeMainCategoryId == null;
                    return _buildCategoryTile(
                      context: context,
                      label: 'Wszystkie',
                      iconData: Icons.apps,
                      isSelected: isSelected,
                      onTap: () => homeProvider.setMainCategory(null),
                    );
                  }

                  final category = categories[index - 1];
                  final isSelected =
                      homeProvider.activeMainCategoryId == category.mainCategoryId;
                  return _buildCategoryTile(
                    context: context,
                    label: category.name,
                    iconData: _getIconForCategory(category.name),
                    isSelected: isSelected,
                    onTap: () => homeProvider.setMainCategory(category.mainCategoryId),
                  );
                },
              ),
            ),
    );
  }

  Widget _buildCategoryTile({
    required BuildContext context,
    required String label,
    required IconData iconData,
    required bool isSelected,
    required VoidCallback onTap,
  }) {
    final primaryColor = const Color(0xFF16a34a);

    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: 80,
        margin: const EdgeInsets.only(right: 12.0),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.start,
          children: [
            AnimatedContainer(
              duration: const Duration(milliseconds: 200),
              width: 64,
              height: 64,
              decoration: BoxDecoration(
                color: isSelected ? primaryColor : Colors.white,
                borderRadius: BorderRadius.circular(16.0),
                border: Border.all(
                  color: isSelected ? primaryColor : Colors.grey.shade200,
                  width: 1.5,
                ),
                boxShadow: isSelected
                    ? [
                        BoxShadow(
                          color: primaryColor.withOpacity(0.3),
                          blurRadius: 10,
                          offset: const Offset(0, 4),
                        )
                      ]
                    : [
                        BoxShadow(
                          color: Colors.black.withOpacity(0.04),
                          blurRadius: 8,
                          offset: const Offset(0, 2),
                        )
                      ],
              ),
              child: Icon(
                iconData,
                color: isSelected ? Colors.white : Colors.black87,
                size: 28,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              label,
              textAlign: TextAlign.center,
              maxLines: 2,
              overflow: TextOverflow.ellipsis,
              style: TextStyle(
                color: isSelected ? primaryColor : Colors.black87,
                fontWeight: isSelected ? FontWeight.bold : FontWeight.w500,
                fontSize: 11,
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildSkeleton(BuildContext context) {
    return Container(
      height: 95,
      margin: const EdgeInsets.only(top: 16.0, bottom: 4.0),
      child: ListView.builder(
        scrollDirection: Axis.horizontal,
        padding: const EdgeInsets.symmetric(horizontal: 16.0),
        itemCount: 5,
        itemBuilder: (context, index) {
          return Container(
            width: 80,
            margin: const EdgeInsets.only(right: 12.0),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.start,
              children: [
                Shimmer.fromColors(
                  baseColor: Colors.grey.shade300,
                  highlightColor: Colors.grey.shade100,
                  child: Container(
                    width: 64,
                    height: 64,
                    decoration: BoxDecoration(
                      color: Colors.white,
                      borderRadius: BorderRadius.circular(16.0),
                    ),
                  ),
                ),
                const SizedBox(height: 8),
                Shimmer.fromColors(
                  baseColor: Colors.grey.shade300,
                  highlightColor: Colors.grey.shade100,
                  child: Container(
                    width: 48,
                    height: 10,
                    decoration: BoxDecoration(
                      color: Colors.white,
                      borderRadius: BorderRadius.circular(4.0),
                    ),
                  ),
                ),
              ],
            ),
          );
        },
      ),
    );
  }
}
