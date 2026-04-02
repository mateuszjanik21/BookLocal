import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/home_provider.dart';

class SortingHeader extends StatelessWidget {
  const SortingHeader({super.key});

  @override
  Widget build(BuildContext context) {
    final homeProvider = Provider.of<HomeProvider>(context);
    final hasCategory = homeProvider.activeMainCategoryId != null;

    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16.0, vertical: 8.0),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          Expanded(
            child: RichText(
              text: TextSpan(
                style: const TextStyle(
                    fontSize: 22,
                    fontWeight: FontWeight.bold,
                    color: Colors.black87),
                children: [
                  TextSpan(
                    text: hasCategory ? 'Oto ' : 'Wszystkie ',
                    style: const TextStyle(color: Color(0xFF16a34a)),
                  ),
                  TextSpan(
                    text: hasCategory ? 'polecane salony' : 'usługi',
                  ),
                ],
              ),
            ),
          ),
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 12),
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(24),
              border: Border.all(color: Colors.grey.shade300),
            ),
            child: DropdownButtonHideUnderline(
              child: DropdownButton<String>(
                value: homeProvider.activeSortBy.isEmpty
                    ? ''
                    : homeProvider.activeSortBy,
                icon: const Icon(Icons.keyboard_arrow_down,
                    color: Colors.black54),
                style: const TextStyle(
                    fontSize: 13,
                    fontWeight: FontWeight.w500,
                    color: Colors.black87),
                onChanged: (String? newValue) {
                  if (newValue != null) {
                    homeProvider.setSortBy(newValue);
                  }
                },
                items: const [
                  DropdownMenuItem(value: '', child: Text('Polecane')),
                  DropdownMenuItem(
                      value: 'rating_desc', child: Text('Najwyżej oceniane')),
                  DropdownMenuItem(
                      value: 'reviews_desc',
                      child: Text('Najbardziej popularne')),
                  DropdownMenuItem(
                      value: 'newest_desc', child: Text('Nowości na BookLocal')),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}
