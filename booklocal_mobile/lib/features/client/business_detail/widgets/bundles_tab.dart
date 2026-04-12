import 'package:flutter/material.dart';
import '../../../../core/models/business_list_item_dto.dart';
import '../../../../core/models/service_bundle_dto.dart';
import '../../booking/bundle_booking_screen.dart';
import 'section_card.dart';

class BundlesTab extends StatelessWidget {
  final List<ServiceBundleDto> bundles;
  final bool isLoading;
  final BusinessListItemDto business;

  const BundlesTab({
    super.key,
    required this.bundles,
    required this.isLoading,
    required this.business,
  });

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      physics: const BouncingScrollPhysics(
        parent: AlwaysScrollableScrollPhysics(),
      ),
      padding: const EdgeInsets.fromLTRB(16, 16, 16, 40),
      child: SectionCard(
        title: "Pakiety",
        icon: Icons.card_giftcard,
        child: isLoading
            ? const Center(child: CircularProgressIndicator())
            : bundles.isEmpty
                ? const Center(
                    child: Padding(
                      padding: EdgeInsets.all(20.0),
                      child: Text(
                        "Brak dostępnych pakietów.",
                        style: TextStyle(color: Colors.grey),
                      ),
                    ),
                  )
                : ListView.separated(
                    shrinkWrap: true,
                    physics: const NeverScrollableScrollPhysics(),
                    itemCount: bundles.length,
                    separatorBuilder: (context, index) => const SizedBox(height: 16),
                    itemBuilder: (context, index) => _BundleCard(
                      bundle: bundles[index],
                      business: business,
                    ),
                  ),
      ),
    );
  }
}

class _BundleCard extends StatelessWidget {
  static const Color _primary = Color(0xFF16a34a);
  final ServiceBundleDto bundle;
  final BusinessListItemDto business;

  const _BundleCard({required this.bundle, required this.business});

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(20),
        color: Colors.white,
        border: Border.all(color: Colors.grey.shade200),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.04),
            blurRadius: 15,
            offset: const Offset(0, 5),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Header
          Container(
            padding: const EdgeInsets.all(18),
            decoration: BoxDecoration(
              gradient: LinearGradient(
                colors: [
                  _primary.withOpacity(0.08),
                  _primary.withOpacity(0.03),
                ],
                begin: Alignment.topLeft,
                end: Alignment.bottomRight,
              ),
              borderRadius: const BorderRadius.vertical(
                top: Radius.circular(20),
              ),
            ),
            child: Row(
              children: [
                Container(
                  padding: const EdgeInsets.all(10),
                  decoration: BoxDecoration(
                    color: _primary.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: const Icon(
                    Icons.card_giftcard,
                    color: _primary,
                    size: 20,
                  ),
                ),
                const SizedBox(width: 14),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        bundle.name,
                        style: const TextStyle(
                          fontWeight: FontWeight.bold,
                          fontSize: 16,
                          color: Color(0xFF1F2937),
                        ),
                      ),
                      if (bundle.description != null &&
                          bundle.description!.isNotEmpty)
                        Padding(
                          padding: const EdgeInsets.only(top: 3),
                          child: Text(
                            bundle.description!,
                            style: TextStyle(
                              color: Colors.grey[600],
                              fontSize: 12,
                            ),
                            maxLines: 2,
                            overflow: TextOverflow.ellipsis,
                          ),
                        ),
                    ],
                  ),
                ),
                if (bundle.discountPercent > 0)
                  Container(
                    padding: const EdgeInsets.symmetric(
                      horizontal: 10,
                      vertical: 5,
                    ),
                    decoration: BoxDecoration(
                      color: Colors.redAccent,
                      borderRadius: BorderRadius.circular(10),
                    ),
                    child: Text(
                      "-${bundle.discountPercent}%",
                      style: const TextStyle(
                        color: Colors.white,
                        fontWeight: FontWeight.w800,
                        fontSize: 12,
                      ),
                    ),
                  ),
              ],
            ),
          ),

          Padding(
            padding: const EdgeInsets.fromLTRB(18, 14, 18, 10),
            child: Column(
              children: bundle.items.asMap().entries.map((entry) {
                final item = entry.value;
                return Padding(
                  padding: const EdgeInsets.symmetric(vertical: 5),
                  child: Row(
                    children: [
                      Container(
                        width: 22,
                        height: 22,
                        decoration: BoxDecoration(
                          color: _primary.withOpacity(0.1),
                          borderRadius: BorderRadius.circular(6),
                        ),
                        child: const Icon(
                          Icons.check,
                          size: 13,
                          color: _primary,
                        ),
                      ),
                      const SizedBox(width: 10),
                      Expanded(
                        child: Text(
                          item.serviceName +
                              (item.variantName.isNotEmpty &&
                                      item.variantName.toLowerCase() !=
                                          "domyślny"
                                  ? " - ${item.variantName}"
                                  : ""),
                          style: const TextStyle(
                            fontSize: 13,
                            color: Color(0xFF374151),
                            fontWeight: FontWeight.w500,
                          ),
                        ),
                      ),
                      Text(
                        "${item.durationMinutes} min",
                        style: TextStyle(
                          color: Colors.grey[400],
                          fontSize: 11,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                    ],
                  ),
                );
              }).toList(),
            ),
          ),

          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 18),
            child: Divider(color: Colors.grey.shade200, height: 1),
          ),

          Padding(
            padding: const EdgeInsets.all(18),
            child: Row(
              children: [
                // Duration
                Icon(Icons.schedule, size: 15, color: Colors.grey[400]),
                const SizedBox(width: 4),
                Text(
                  "~${bundle.totalDurationMinutes} min",
                  style: TextStyle(
                    color: Colors.grey[500],
                    fontSize: 12,
                    fontWeight: FontWeight.w600,
                  ),
                ),
                const Spacer(),
                // Price
                Column(
                  crossAxisAlignment: CrossAxisAlignment.end,
                  children: [
                    if (bundle.discountPercent > 0)
                      Text(
                        "${bundle.originalTotalPrice.toInt()} zł",
                        style: TextStyle(
                          color: Colors.grey[400],
                          fontSize: 12,
                          decoration: TextDecoration.lineThrough,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                    Text(
                      "${bundle.totalPrice.toStringAsFixed(0)} zł",
                      style: const TextStyle(
                        fontWeight: FontWeight.w900,
                        fontSize: 20,
                        color: _primary,
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),

          Padding(
            padding: const EdgeInsets.fromLTRB(18, 0, 18, 18),
            child: SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                onPressed: () {
                  Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (_) => BundleBookingScreen(
                        business: business,
                        bundle: bundle,
                      ),
                    ),
                  );
                },
                icon: const Icon(Icons.calendar_today, size: 18),
                label: const Text(
                  "Zarezerwuj pakiet",
                  style: TextStyle(
                    fontWeight: FontWeight.bold,
                    fontSize: 14,
                  ),
                ),
                style: ElevatedButton.styleFrom(
                  backgroundColor: _primary,
                  foregroundColor: Colors.white,
                  padding: const EdgeInsets.symmetric(vertical: 14),
                  elevation: 0,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(14),
                  ),
                  shadowColor: _primary.withOpacity(0.3),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
