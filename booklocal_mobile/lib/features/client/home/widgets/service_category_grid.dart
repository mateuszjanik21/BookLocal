import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/home_provider.dart';
import 'service_category_card.dart';
import 'package:shimmer/shimmer.dart';

class ServiceCategoryGrid extends StatelessWidget {
  const ServiceCategoryGrid({super.key});

  @override
  Widget build(BuildContext context) {
    final homeProvider = Provider.of<HomeProvider>(context);

    if (homeProvider.isSkeletonVisible) {
      return _buildSkeletonGrid(context);
    }

    if (!homeProvider.isLoading &&
        (homeProvider.pagedResult == null ||
            homeProvider.pagedResult!.items.isEmpty)) {
      return SliverToBoxAdapter(
        child: Padding(
          padding: const EdgeInsets.symmetric(vertical: 40.0),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const Icon(Icons.search_off_rounded,
                  size: 80, color: Colors.black26),
              const SizedBox(height: 16),
              const Text(
                'Brak ofert pasujących do Twoich kryteriów',
                style: TextStyle(
                    fontSize: 18,
                    fontWeight: FontWeight.bold,
                    color: Colors.black87),
              ),
              const SizedBox(height: 8),
              const Text(
                'Spróbuj wpisać inną frazę lub zmienić lokalizację.',
                style: TextStyle(color: Colors.black54),
              ),
              const SizedBox(height: 24),
              OutlinedButton(
                onPressed: () => homeProvider.clearSearch(),
                style: OutlinedButton.styleFrom(
                  padding:
                      const EdgeInsets.symmetric(horizontal: 32, vertical: 12),
                  shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12)),
                ),
                child: const Text('Wyczyść filtry'),
              )
            ],
          ),
        ),
      );
    }

    final items = homeProvider.pagedResult?.items ?? [];
    
    // Obliczamy odpowiednią liczbę kolumn w zależności od szerokości ekranu
    int crossAxisCount = 1;
    double width = MediaQuery.of(context).size.width;
    if (width >= 1024) {
      crossAxisCount = 4;
    } else if (width >= 768) {
      crossAxisCount = 2;
    }

    return SliverMainAxisGroup(
      slivers: [
        SliverPadding(
          padding: const EdgeInsets.only(bottom: 24.0),
          sliver: SliverGrid(
            gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
              crossAxisCount: crossAxisCount,
              mainAxisSpacing: 16,
              crossAxisSpacing: 16,
              childAspectRatio: crossAxisCount == 1 ? 2.2 : 0.75, // Zmiana proporcji dla mobile (pozioma karta)
            ),
            delegate: SliverChildBuilderDelegate(
              (context, index) {
                return ServiceCategoryCard(item: items[index]);
              },
              childCount: items.length,
            ),
          ),
        ),
        if (homeProvider.pagedResult != null &&
            homeProvider.pagedResult!.totalPages > 1)
          SliverToBoxAdapter(
            child: _buildPagination(context, homeProvider),
          ),
      ],
    );
  }

  Widget _buildPagination(BuildContext context, HomeProvider provider) {
    final pages = provider.getPaginationPages();
    final currentPage = provider.pagedResult!.pageNumber;
    final totalPages = provider.pagedResult!.totalPages;

    return Padding(
      padding: const EdgeInsets.only(bottom: 40.0),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          IconButton(
            icon: const Icon(Icons.chevron_left),
            onPressed: currentPage > 1
                ? () => provider.pageChanged(currentPage - 1)
                : null,
          ),
          ...pages.map((page) {
            if (page == '...') {
              return const Padding(
                padding: EdgeInsets.symmetric(horizontal: 8.0),
                child: Text('...'),
              );
            }
            final isCurrent = page == currentPage;
            return Container(
              margin: const EdgeInsets.symmetric(horizontal: 4),
              width: 36,
              height: 36,
              decoration: BoxDecoration(
                color: isCurrent ? const Color(0xFF16a34a) : Colors.transparent,
                borderRadius: BorderRadius.circular(8),
                border: Border.all(
                  color:
                      isCurrent ? const Color(0xFF16a34a) : Colors.grey.shade300,
                ),
              ),
              child: InkWell(
                onTap: () => provider.pageChanged(page as int),
                child: Center(
                  child: Text(
                    page.toString(),
                    style: TextStyle(
                      color: isCurrent ? Colors.white : Colors.black87,
                      fontWeight:
                          isCurrent ? FontWeight.bold : FontWeight.normal,
                    ),
                  ),
                ),
              ),
            );
          }),
          IconButton(
            icon: const Icon(Icons.chevron_right),
            onPressed: currentPage < totalPages
                ? () => provider.pageChanged(currentPage + 1)
                : null,
          ),
        ],
      ),
    );
  }

  // Uproszczony skeleton build na potrzeby widoku (może zostać ulepszony w Fazie 5)
  Widget _buildSkeletonGrid(BuildContext context) {
    int crossAxisCount = MediaQuery.of(context).size.width >= 768 ? 2 : 1;

    return SliverGrid(
      gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: crossAxisCount,
        mainAxisSpacing: 16,
        crossAxisSpacing: 16,
        childAspectRatio: crossAxisCount == 1 ? 2.2 : 0.75,
      ),
      delegate: SliverChildBuilderDelegate(
        (context, index) {
          return Shimmer.fromColors(
            baseColor: Colors.grey.shade300,
            highlightColor: Colors.grey.shade100,
            child: Container(
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(16),
              ),
            ),
          );
        },
        childCount: 8,
      ),
    );
  }
}
