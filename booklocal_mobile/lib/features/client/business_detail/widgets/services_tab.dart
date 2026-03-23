import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../../core/models/business_detail_dto.dart';
import '../../../../core/models/business_list_item_dto.dart';
import '../../../../core/models/service.dart';
import '../../../../core/models/service_variant.dart';
import '../../../../core/models/service_models.dart';
import '../../booking/booking_screen.dart';
import '../../favorites/providers/favorites_provider.dart';
import 'section_card.dart';

class ServicesTab extends StatelessWidget {
  final BusinessDetailDto? fullBusiness;
  final BusinessListItemDto business;
  final bool isLoading;

  const ServicesTab({
    super.key,
    required this.fullBusiness,
    required this.business,
    required this.isLoading,
  });

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      padding: const EdgeInsets.fromLTRB(16, 16, 16, 40),
      child: SectionCard(
        title: "Cennik Usług",
        icon: Icons.list_alt,
        child: isLoading
            ? const Center(child: CircularProgressIndicator())
            : fullBusiness == null || fullBusiness!.categories.isEmpty
                ? const Text("Brak usług.", style: TextStyle(color: Colors.grey))
                : ListView.builder(
                    shrinkWrap: true,
                    physics: const NeverScrollableScrollPhysics(),
                    itemCount: fullBusiness!.categories.length,
                    itemBuilder: (context, catIndex) {
                      final category = fullBusiness!.categories[catIndex];
                      if (category.services.isEmpty) return const SizedBox.shrink();
                      return Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Padding(
                            padding: const EdgeInsets.only(top: 8.0, bottom: 12.0),
                            child: Text(category.name, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 18, color: Color(0xFF16a34a))),
                          ),
                          ...category.services.expand((service) => service.variants.map((variant) => _buildServiceRow(context, service, variant))),
                          const SizedBox(height: 16),
                        ],
                      );
                    },
                  ),
      ),
    );
  }

  Widget _buildServiceRow(BuildContext context, Service service, ServiceVariant variant) {
    return Material(
      color: Colors.transparent,
      child: InkWell(
        onTap: () {
          final dummyDto = ServiceDto(
            id: service.id,
            name: variant.name.toLowerCase() == "domyślny" || variant.name.toLowerCase() == "default"
                ? service.name
                : "${service.name} - ${variant.name}",
            description: service.description ?? "",
            price: variant.price,
            durationMinutes: variant.durationMinutes,
          );
          Navigator.push(
            context,
            MaterialPageRoute(
              builder: (context) => BookingScreen(
                business: business,
                service: dummyDto,
              ),
            ),
          );
        },
        borderRadius: BorderRadius.circular(8),
        child: Padding(
          padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 8),
          child: Row(
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      variant.name.toLowerCase() == "domyślny" || variant.name.toLowerCase() == "default"
                          ? service.name
                          : "${service.name} - ${variant.name}",
                      style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 15, color: Color(0xFF1F2937)),
                    ),
                    const SizedBox(height: 6),
                    Row(
                      children: [
                        Icon(Icons.schedule, size: 14, color: Colors.grey[400]),
                        const SizedBox(width: 4),
                        Text(
                          "${variant.durationMinutes} min",
                          style: TextStyle(color: Colors.grey[500], fontSize: 13),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
              Row(
                children: [
                  Text(
                    "${variant.price.toInt()} zł",
                    style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16, color: Color(0xFF16a34a)),
                  ),
                  const SizedBox(width: 8),
                  Consumer<FavoritesProvider>(
                    builder: (context, provider, child) {
                      final isFav = provider.favorites.any((f) => f.serviceVariantId == variant.serviceVariantId);
                      return Padding(
                        padding: const EdgeInsets.only(right: 8.0),
                        child: InkWell(
                          onTap: () async {
                            if (isFav) {
                              await provider.removeFavorite(variant.serviceVariantId);
                            } else {
                              await provider.addFavorite(variant.serviceVariantId);
                            }
                          },
                          child: Icon(
                            isFav ? Icons.favorite : Icons.favorite_border,
                            size: 20,
                            color: isFav ? Colors.redAccent : Colors.grey[400],
                          ),
                        ),
                      );
                    },
                  ),
                  const SizedBox(width: 4),
                  Icon(Icons.arrow_forward_ios_rounded, size: 14, color: Colors.grey[300]),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}
