import 'package:flutter/material.dart';
import 'package:cached_network_image/cached_network_image.dart';
import '../../../../core/models/service_category_search_result.dart';

class ServiceCategoryCard extends StatelessWidget {
  final ServiceCategorySearchResult item;

  const ServiceCategoryCard({super.key, required this.item});

  @override
  Widget build(BuildContext context) {
    final bool isMobile = MediaQuery.of(context).size.width < 768;

    return GestureDetector(
      onTap: () {
        // Docelowo: powiązanie z widokiem biznesu. BusinessListItemDto czy Id? 
        // Konstruktor BusinessDetailsScreen oczekiwał BusinessListItemDto. 
        // Będzie wymagał refaktoryzacji, na razie wyłączamy nawigację rzucając komentarz
        // Navigator.push(context, MaterialPageRoute(builder: (_) => BusinessDetailsScreen(business: item.businessId)));
      },
      child: Container(
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(16),
          border: Border.all(color: Colors.black.withOpacity(0.05)),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withOpacity(0.03),
              blurRadius: 10,
              offset: const Offset(0, 4),
            ),
          ],
        ),
        clipBehavior: Clip.antiAlias,
        child: isMobile ? _buildHorizontalLayout() : _buildVerticalLayout(),
      ),
    );
  }

  Widget _buildHorizontalLayout() {
    return Row(
      children: [
        // ZDJĘCIE (lewa strona)
        SizedBox(
          width: 120,
          height: double.infinity,
          child: Stack(
            fit: StackFit.expand,
            children: [
              _buildImage(),
              Positioned(
                bottom: 8,
                left: 8,
                child: _buildCityBadge(),
              ),
            ],
          ),
        ),
        // TREŚĆ (prawa strona)
        Expanded(
          child: Padding(
            padding: const EdgeInsets.all(12.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                _buildHeaderInfo(showRatingBadge: true),
                const SizedBox(height: 4),
                _buildServicesList(maxCount: 2, isMobile: true),
              ],
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildVerticalLayout() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        // ZDJĘCIE (góra)
        SizedBox(
          height: 160,
          width: double.infinity,
          child: Stack(
            fit: StackFit.expand,
            children: [
              _buildImage(),
              Positioned(
                bottom: 12,
                left: 12,
                child: _buildCityBadge(),
              ),
            ],
          ),
        ),
        // TREŚĆ (dół)
        Expanded(
          child: Padding(
            padding: const EdgeInsets.all(16.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                _buildHeaderInfo(showRatingBadge: false),
                const Divider(color: Colors.black12, height: 24),
                _buildRatingAndLocationBar(),
                const SizedBox(height: 12),
                const Spacer(),
                _buildServicesList(maxCount: 2, isMobile: false),
              ],
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildImage() {
    return CachedNetworkImage(
      imageUrl: item.photoUrl ?? '',
      fit: BoxFit.cover,
      placeholder: (context, url) => Container(
        color: Colors.grey.shade100,
        child: const Center(child: Icon(Icons.image, color: Colors.black12, size: 40)),
      ),
      errorWidget: (context, url, error) => Container(
        color: Colors.grey.shade100,
        child: const Center(child: Icon(Icons.store, color: Colors.black12, size: 40)),
      ),
    );
  }

  Widget _buildCityBadge() {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
      decoration: BoxDecoration(
        color: Colors.white.withOpacity(0.9),
        borderRadius: BorderRadius.circular(6),
      ),
      child: Text(
        (item.businessCity ?? '').toUpperCase(),
        style: const TextStyle(fontSize: 9, fontWeight: FontWeight.bold),
      ),
    );
  }

  Widget _buildHeaderInfo({required bool showRatingBadge}) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                (item.mainCategoryName ?? 'USŁUGI').toUpperCase(),
                style: const TextStyle(
                  color: Color(0xFF16a34a),
                  fontSize: 10,
                  fontWeight: FontWeight.bold,
                  letterSpacing: 1.0,
                ),
              ),
              const SizedBox(height: 2),
              Text(
                item.name,
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
                style: const TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.bold,
                  color: Colors.black87,
                ),
              ),
              Text(
                item.businessName,
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
                style: const TextStyle(
                  fontSize: 11,
                  color: Colors.black54,
                ),
              ),
            ],
          ),
        ),
        if (showRatingBadge)
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 4),
            decoration: BoxDecoration(
              color: Colors.amber.shade50,
              borderRadius: BorderRadius.circular(8),
              border: Border.all(color: Colors.amber.shade200),
            ),
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                const Icon(Icons.star, color: Colors.amber, size: 12),
                const SizedBox(width: 4),
                Text(
                  item.averageRating.toStringAsFixed(1),
                  style: const TextStyle(
                    color: Colors.amber,
                    fontSize: 11,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ],
            ),
          ),
      ],
    );
  }

  Widget _buildRatingAndLocationBar() {
    return Row(
      children: [
        Container(
          padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
          decoration: BoxDecoration(
            color: Colors.amber.shade50,
            borderRadius: BorderRadius.circular(6),
          ),
          child: Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(Icons.star, color: Colors.amber, size: 12),
              const SizedBox(width: 4),
              Text(
                item.averageRating.toStringAsFixed(1),
                style: const TextStyle(
                  color: Colors.amber,
                  fontSize: 11,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ],
          ),
        ),
        const SizedBox(width: 8),
        Text(
          '${item.reviewCount} opinii',
          style: const TextStyle(fontSize: 11, color: Colors.black45),
        ),
        const SizedBox(width: 8),
        const Text('•', style: TextStyle(color: Colors.black26)),
        const SizedBox(width: 8),
        Expanded(
          child: Text(
            item.businessCity ?? '',
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: const TextStyle(fontSize: 11, color: Colors.black45),
          ),
        ),
      ],
    );
  }

  Widget _buildServicesList({required int maxCount, required bool isMobile}) {
    final services = item.services.take(maxCount).toList();

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: services.map((service) {
        final price = service.variants.isNotEmpty ? service.variants.first.price : 0.0;
        
        if (isMobile) {
          return Container(
            margin: const EdgeInsets.only(top: 4),
            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 6),
            decoration: BoxDecoration(
              color: Colors.grey.shade50,
              borderRadius: BorderRadius.circular(8),
              border: Border.all(color: Colors.grey.shade200),
            ),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Expanded(
                  child: Text(
                    service.name,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(fontSize: 10, color: Colors.black54),
                  ),
                ),
                Text(
                  'od $price zł',
                  style: const TextStyle(
                    fontSize: 10,
                    fontWeight: FontWeight.bold,
                    color: Color(0xFF16a34a),
                  ),
                ),
              ],
            ),
          );
        } else {
          return Padding(
            padding: const EdgeInsets.only(top: 6),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Expanded(
                  child: Text(
                    service.name,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(fontSize: 12, color: Colors.black87),
                  ),
                ),
                Text(
                  'od $price zł',
                  style: const TextStyle(
                    fontSize: 12,
                    fontWeight: FontWeight.bold,
                    color: Colors.black87,
                  ),
                ),
              ],
            ),
          );
        }
      }).toList(),
    );
  }
}
