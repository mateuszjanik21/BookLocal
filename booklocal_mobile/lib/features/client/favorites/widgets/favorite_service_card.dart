import 'package:booklocal_mobile/features/client/business_detail/business_details_screen.dart';
import 'package:flutter/material.dart';
import '../../../../core/models/favorite_service_dto.dart';
import '../../../../core/models/business_list_item_dto.dart';
import '../providers/favorites_provider.dart';
import 'package:provider/provider.dart';

class FavoriteServiceCard extends StatelessWidget {
  final FavoriteServiceDto favorite;

  const FavoriteServiceCard({super.key, required this.favorite});

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      color: Colors.white,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      elevation: 1,
      child: InkWell(
        borderRadius: BorderRadius.circular(16),
        onTap: () {
          final dummyBusiness = BusinessListItemDto(
            id: favorite.businessId,
            name: favorite.businessName,
            category: '', // Pusta kategoria dla makiety
            city: favorite.businessCity,
            photoUrl: favorite.businessPhotoUrl,
            rating: 0.0,
            reviewCount: 0,
          );
          Navigator.push(
            context,
            MaterialPageRoute(
              builder: (context) => BusinessDetailsScreen(business: dummyBusiness),
            ),
          );
        },
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Row(
            children: [
              // Image
              Container(
                width: 56,
                height: 56,
                decoration: BoxDecoration(
                  borderRadius: BorderRadius.circular(12),
                  color: Colors.grey[200],
                  border: Border.all(color: Colors.grey[200]!),
                ),
                child: ClipRRect(
                  borderRadius: BorderRadius.circular(12),
                  child: favorite.businessPhotoUrl != null
                      ? Image.network(
                          favorite.businessPhotoUrl!, 
                          fit: BoxFit.cover, 
                          errorBuilder: (context, error, stackTrace) => const Icon(Icons.store, color: Colors.grey)
                        )
                      : const Icon(Icons.store, color: Colors.grey),
                ),
              ),
              const SizedBox(width: 16),
              // Info
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      favorite.serviceName,
                      style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16, color: Color(0xFF1F2937)),
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
                    const SizedBox(height: 6),
                    Row(
                      children: [
                        Flexible(
                          child: Text(
                            favorite.businessName,
                            style: const TextStyle(
                              color: Color(0xFF16a34a), // Primary color
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
                        ]
                      ],
                    ),
                  ],
                ),
              ),
              const SizedBox(width: 8),
              // Price & Delete
              Column(
                crossAxisAlignment: CrossAxisAlignment.end,
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text(
                    "${favorite.price.toStringAsFixed(2)} zł",
                    style: const TextStyle(
                      fontWeight: FontWeight.bold,
                      color: Color(0xFF16a34a),
                      fontSize: 15,
                    ),
                  ),
                  const SizedBox(height: 12),
                  InkWell(
                    onTap: () async {
                      final provider = Provider.of<FavoritesProvider>(context, listen: false);
                      final scaffoldMessenger = ScaffoldMessenger.of(context);
                      final isSuccess = await provider.removeFavorite(favorite.serviceVariantId);
                      
                      if (isSuccess) {
                        scaffoldMessenger.clearSnackBars();
                        scaffoldMessenger.showSnackBar(
                          SnackBar(
                            content: Row(
                              mainAxisAlignment: MainAxisAlignment.spaceBetween,
                              children: [
                                const Text("Usunięto z ulubionych"),
                                GestureDetector(
                                  onTap: () {
                                    provider.addFavorite(favorite.serviceVariantId);
                                    scaffoldMessenger.hideCurrentSnackBar();
                                  },
                                  child: const Padding(
                                    padding: EdgeInsets.symmetric(horizontal: 8.0, vertical: 4.0),
                                    child: Text('COFNIJ', style: TextStyle(color: Colors.greenAccent, fontWeight: FontWeight.bold)),
                                  ),
                                ),
                              ],
                            ),
                            duration: const Duration(seconds: 3),
                            behavior: SnackBarBehavior.floating,
                          ),
                        );
                      }
                    },
                    borderRadius: BorderRadius.circular(20),
                    child: Container(
                      padding: const EdgeInsets.all(4),
                      child: const Icon(Icons.delete_outline, color: Colors.redAccent, size: 22),
                    ),
                  ),
                ],
              )
            ],
          ),
        ),
      ),
    );
  }
}
