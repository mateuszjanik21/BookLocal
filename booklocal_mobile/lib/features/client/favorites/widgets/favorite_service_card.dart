import 'package:booklocal_mobile/features/client/business_detail/business_details_screen.dart';
import 'package:flutter/material.dart';
import '../../../../core/models/favorite_service_dto.dart';
import '../../../../core/models/business_list_item_dto.dart';
import '../providers/favorites_provider.dart';
import 'package:provider/provider.dart';

class FavoriteServiceCard extends StatelessWidget {
  final FavoriteServiceDto favorite;

  const FavoriteServiceCard({super.key, required this.favorite});

  String _formatDuration(int minutes) {
    if (minutes < 60) return '${minutes}min';
    final h = minutes ~/ 60;
    final m = minutes % 60;
    return m > 0 ? '${h}h ${m}min' : '${h}h';
  }

  @override
  Widget build(BuildContext context) {
    return Dismissible(
      key: ValueKey(favorite.serviceVariantId),
      direction: DismissDirection.endToStart,
      background: Container(
        margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 6),
        decoration: BoxDecoration(
          color: Colors.red[400],
          borderRadius: BorderRadius.circular(16),
        ),
        alignment: Alignment.centerRight,
        padding: const EdgeInsets.only(right: 24),
        child: const Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.delete_rounded, color: Colors.white, size: 28),
            SizedBox(height: 4),
            Text('Usuń', style: TextStyle(color: Colors.white, fontWeight: FontWeight.w600, fontSize: 12)),
          ],
        ),
      ),
      confirmDismiss: (direction) async {
        return true;
      },
      onDismissed: (direction) {
        final provider = Provider.of<FavoritesProvider>(context, listen: false);
        final scaffoldMessenger = ScaffoldMessenger.of(context);
        provider.removeFavorite(favorite.serviceVariantId);
        scaffoldMessenger.clearSnackBars();
        scaffoldMessenger.showSnackBar(
          SnackBar(
            content: const Text("Usunięto z ulubionych"),
            action: SnackBarAction(
              label: 'COFNIJ',
              textColor: const Color(0xFF22c55e),
              onPressed: () {
                provider.addFavorite(favorite.serviceVariantId);
              },
            ),
            duration: const Duration(seconds: 4),
            behavior: SnackBarBehavior.floating,
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
          ),
        );
      },
      child: Container(
        margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 6),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(16),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withOpacity(0.04),
              blurRadius: 12,
              offset: const Offset(0, 4),
            ),
          ],
        ),
        child: Material(
          color: Colors.transparent,
          child: InkWell(
            borderRadius: BorderRadius.circular(16),
            onTap: () {
              final dummyBusiness = BusinessListItemDto(
                id: favorite.businessId,
                name: favorite.businessName,
                category: '',
                city: favorite.businessCity,
                photoUrl: favorite.businessPhotoUrl,
                rating: 0.0,
                reviewCount: 0,
              );
              Navigator.push(
                context,
                MaterialPageRoute(
                  builder: (context) => BusinessDetailsScreen(
                    business: dummyBusiness,
                    highlightVariantId: favorite.serviceVariantId,
                  ),
                ),
              );
            },
            child: Padding(
              padding: const EdgeInsets.all(14),
              child: Row(
                children: [
                  // Zdjęcie salonu
                  Container(
                    width: 64,
                    height: 64,
                    decoration: BoxDecoration(
                      borderRadius: BorderRadius.circular(14),
                      color: const Color(0xFFF3F4F6),
                    ),
                    child: ClipRRect(
                      borderRadius: BorderRadius.circular(14),
                      child: favorite.businessPhotoUrl != null
                          ? Image.network(
                              favorite.businessPhotoUrl!,
                              fit: BoxFit.cover,
                              errorBuilder: (context, error, stackTrace) => Container(
                                color: const Color(0xFFF3F4F6),
                                child: const Icon(Icons.store_rounded, color: Colors.grey, size: 28),
                              ),
                            )
                          : const Icon(Icons.store_rounded, color: Colors.grey, size: 28),
                    ),
                  ),
                  const SizedBox(width: 14),
                  // Informacje o usłudze
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          favorite.serviceName,
                          style: const TextStyle(
                            fontWeight: FontWeight.w700,
                            fontSize: 15,
                            color: Color(0xFF1F2937),
                          ),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                        const SizedBox(height: 2),
                        Text(
                          favorite.variantName,
                          style: TextStyle(color: Colors.grey[500], fontSize: 13),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                        const SizedBox(height: 8),
                        // Salon + Miasto
                        Row(
                          children: [
                            Icon(Icons.store_outlined, size: 14, color: Colors.grey[400]),
                            const SizedBox(width: 4),
                            Flexible(
                              child: Text(
                                favorite.businessName,
                                style: const TextStyle(
                                  color: Color(0xFF16a34a),
                                  fontWeight: FontWeight.w600,
                                  fontSize: 13,
                                ),
                                maxLines: 1,
                                overflow: TextOverflow.ellipsis,
                              ),
                            ),
                            if (favorite.businessCity.isNotEmpty) ...[
                              Text(
                                " · ${favorite.businessCity}",
                                style: TextStyle(color: Colors.grey[400], fontSize: 12),
                              ),
                            ],
                          ],
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(width: 10),
                  // Cena + czas
                  Column(
                    crossAxisAlignment: CrossAxisAlignment.end,
                    children: [
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
                        decoration: BoxDecoration(
                          color: const Color(0xFF16a34a).withOpacity(0.08),
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: Text(
                          "${favorite.price.toStringAsFixed(0)} zł",
                          style: const TextStyle(
                            fontWeight: FontWeight.w800,
                            color: Color(0xFF16a34a),
                            fontSize: 14,
                          ),
                        ),
                      ),
                      const SizedBox(height: 6),
                      Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Icon(Icons.schedule_rounded, size: 13, color: Colors.grey[400]),
                          const SizedBox(width: 3),
                          Text(
                            _formatDuration(favorite.durationMinutes),
                            style: TextStyle(
                              color: Colors.grey[500],
                              fontSize: 12,
                              fontWeight: FontWeight.w500,
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
