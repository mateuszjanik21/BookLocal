import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../../core/models/business_detail_dto.dart';
import '../../../../core/models/business_list_item_dto.dart';
import '../../../../core/models/employee_models.dart';
import '../../../../core/models/service.dart';
import '../../../../core/models/service_variant.dart';
import '../../../../core/models/service_models.dart';
import '../../booking/booking_screen.dart';
import '../../favorites/providers/favorites_provider.dart';
import 'section_card.dart';
import 'package:shimmer/shimmer.dart';

class ServicesTab extends StatefulWidget {
  final BusinessDetailDto? fullBusiness;
  final BusinessListItemDto business;
  final bool isLoading;
  final EmployeeDto? preselectedEmployee;
  final int? highlightVariantId;

  const ServicesTab({
    super.key,
    required this.fullBusiness,
    required this.business,
    required this.isLoading,
    this.preselectedEmployee,
    this.highlightVariantId,
  });

  @override
  State<ServicesTab> createState() => _ServicesTabState();
}

class _ServicesTabState extends State<ServicesTab> {
  // ID usługi (rodzica), która ma być auto-rozwinięta
  int? _expandedServiceId;
  bool _didAutoExpand = false;

  @override
  void didUpdateWidget(covariant ServicesTab oldWidget) {
    super.didUpdateWidget(oldWidget);
    // Gdy dane się załadują (fullBusiness zmieni się z null na wartość),
    // znajdź usługę zawierającą highlightVariantId i rozwiń ją
    if (widget.highlightVariantId != null &&
        !_didAutoExpand &&
        widget.fullBusiness != null &&
        widget.fullBusiness!.categories.isNotEmpty) {
      _autoExpandForVariant(widget.highlightVariantId!);
    }
  }

  void _autoExpandForVariant(int variantId) {
    for (var category in widget.fullBusiness!.categories) {
      for (var service in category.services) {
        for (var variant in service.variants) {
          if (variant.serviceVariantId == variantId) {
            setState(() {
              _expandedServiceId = service.id;
              _didAutoExpand = true;
            });
            return;
          }
        }
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      physics: const BouncingScrollPhysics(parent: AlwaysScrollableScrollPhysics()),
      padding: const EdgeInsets.fromLTRB(16, 16, 16, 40),
      child: SectionCard(
        title: "Cennik Usług",
        icon: Icons.list_alt,
        child: widget.isLoading
            ? _buildSkeletonLoader()
            : widget.fullBusiness == null || widget.fullBusiness!.categories.isEmpty
                ? const Text("Brak usług.", style: TextStyle(color: Colors.grey))
                : ListView.builder(
                    shrinkWrap: true,
                    physics: const NeverScrollableScrollPhysics(),
                    itemCount: widget.fullBusiness!.categories.length,
                    itemBuilder: (context, catIndex) {
                      final category = widget.fullBusiness!.categories[catIndex];
                      if (category.services.isEmpty) return const SizedBox.shrink();
                      return Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Padding(
                            padding: const EdgeInsets.only(top: 8.0, bottom: 12.0),
                            child: Text(category.name, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 18, color: Color(0xFF16a34a))),
                          ),
                          ...category.services.map((service) => _buildServiceAccordion(context, service)),
                          const SizedBox(height: 16),
                        ],
                      );
                    },
                  ),
      ),
    );
  }

  Widget _buildServiceAccordion(BuildContext context, Service service) {
    if (service.variants.isEmpty) return const SizedBox.shrink();

    final isAutoExpanded = _expandedServiceId == service.id;

    return Theme(
      data: Theme.of(context).copyWith(dividerColor: Colors.transparent),
      child: ExpansionTile(
        key: PageStorageKey<String>('service_${service.id}'),
        initiallyExpanded: isAutoExpanded,
        title: Text(
          service.name,
          style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 16, color: Color(0xFF1F2937)),
        ),
        subtitle: service.description != null && service.description!.trim().isNotEmpty
            ? Text(
                service.description!,
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
                style: TextStyle(color: Colors.grey[600], fontSize: 13),
              )
            : null,
        childrenPadding: EdgeInsets.zero,
        tilePadding: const EdgeInsets.symmetric(horizontal: 0, vertical: 0),
        children: service.variants.map((variant) => _buildServiceRow(context, service, variant)).toList(),
      ),
    );
  }

  Widget _buildServiceRow(BuildContext context, Service service, ServiceVariant variant) {
    final isHighlighted = widget.highlightVariantId == variant.serviceVariantId;

    return AnimatedContainer(
      duration: const Duration(milliseconds: 600),
      curve: Curves.easeOut,
      decoration: BoxDecoration(
        color: isHighlighted ? const Color(0xFF16a34a).withOpacity(0.08) : Colors.transparent,
        borderRadius: BorderRadius.circular(10),
        border: isHighlighted
            ? Border.all(color: const Color(0xFF16a34a).withOpacity(0.3), width: 1.5)
            : null,
      ),
      child: Material(
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
                  business: widget.business,
                  service: dummyDto,
                  originalServiceId: service.id,
                  serviceVariantId: variant.serviceVariantId,
                  preselectedEmployee: widget.preselectedEmployee,
                ),
              ),
            );
          },
          borderRadius: BorderRadius.circular(10),
          child: Padding(
            padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 8),
            child: Row(
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          if (isHighlighted) ...[
                            const Icon(Icons.favorite, size: 14, color: Colors.redAccent),
                            const SizedBox(width: 6),
                          ],
                          Expanded(
                            child: Text(
                              variant.name.toLowerCase() == "domyślny" || variant.name.toLowerCase() == "default"
                                  ? "Wariant domyślny"
                                  : variant.name,
                              style: TextStyle(
                                fontWeight: isHighlighted ? FontWeight.w700 : FontWeight.w500,
                                fontSize: 15,
                                color: const Color(0xFF374151),
                              ),
                            ),
                          ),
                        ],
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
                    const SizedBox(width: 4),
                    Consumer<FavoritesProvider>(
                      builder: (context, provider, child) {
                        final isFav = provider.favorites.any((f) => f.serviceVariantId == variant.serviceVariantId);
                        return IconButton(
                          icon: Icon(
                            isFav ? Icons.favorite : Icons.favorite_border,
                            size: 24,
                            color: isFav ? Colors.redAccent : Colors.grey[400],
                          ),
                          onPressed: () async {
                            if (isFav) {
                              await provider.removeFavorite(variant.serviceVariantId);
                            } else {
                              await provider.addFavorite(variant.serviceVariantId);
                            }
                          },
                          constraints: const BoxConstraints(minWidth: 40, minHeight: 40),
                          padding: EdgeInsets.zero,
                          splashRadius: 24,
                        );
                      },
                    ),
                    Icon(Icons.arrow_forward_ios_rounded, size: 14, color: Colors.grey[300]),
                  ],
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildSkeletonLoader() {
    return Shimmer.fromColors(
      baseColor: Colors.grey.shade300,
      highlightColor: Colors.grey.shade100,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: List.generate(
          3,
          (index) => Padding(
            padding: const EdgeInsets.only(bottom: 24.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Container(
                  width: 140,
                  height: 20,
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(4),
                  ),
                ),
                const SizedBox(height: 16),
                Container(
                  width: double.infinity,
                  height: 60,
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(8),
                  ),
                ),
                const SizedBox(height: 8),
                Container(
                  width: double.infinity,
                  height: 60,
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(8),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
