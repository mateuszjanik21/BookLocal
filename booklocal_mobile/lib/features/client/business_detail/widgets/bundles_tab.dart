import 'package:flutter/material.dart';
import '../../../../core/models/service_bundle_dto.dart';
import 'section_card.dart';

class BundlesTab extends StatelessWidget {
  final List<ServiceBundleDto> bundles;
  final bool isLoading;

  const BundlesTab({
    super.key,
    required this.bundles,
    required this.isLoading,
  });

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
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
                      child: Text("Brak dostępnych pakietów.", style: TextStyle(color: Colors.grey)),
                    ),
                  )
                : ListView.separated(
                    shrinkWrap: true,
                    physics: const NeverScrollableScrollPhysics(),
                    itemCount: bundles.length,
                    separatorBuilder: (_, _) => const SizedBox(height: 16),
                    itemBuilder: (context, index) => _BundleCard(bundle: bundles[index]),
                  ),
      ),
    );
  }
}

class _BundleCard extends StatelessWidget {
  final ServiceBundleDto bundle;

  const _BundleCard({required this.bundle});

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: Colors.grey.shade200),
        color: Colors.white,
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Nagłówek pakietu
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: const Color(0xFF16a34a).withOpacity(0.05),
              borderRadius: const BorderRadius.vertical(top: Radius.circular(12)),
            ),
            child: Row(
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(bundle.name, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16, color: Color(0xFF1F2937))),
                      if (bundle.description != null && bundle.description!.isNotEmpty)
                        Padding(
                          padding: const EdgeInsets.only(top: 4.0),
                          child: Text(bundle.description!, style: TextStyle(color: Colors.grey[600], fontSize: 13)),
                        ),
                    ],
                  ),
                ),
                if (bundle.discountPercent > 0)
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                    decoration: BoxDecoration(
                      color: Colors.redAccent,
                      borderRadius: BorderRadius.circular(20),
                    ),
                    child: Text("-${bundle.discountPercent}%", style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 12)),
                  ),
              ],
            ),
          ),
          // Lista elementów pakietu
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
            child: Column(
              children: bundle.items
                  .map((item) => Padding(
                        padding: const EdgeInsets.symmetric(vertical: 6.0),
                        child: Row(
                          children: [
                            Icon(Icons.check_circle_outline, size: 16, color: const Color(0xFF16a34a).withOpacity(0.7)),
                            const SizedBox(width: 10),
                            Expanded(
                              child: Text(
                                item.serviceName + (item.variantName.isNotEmpty && item.variantName.toLowerCase() != "domyślny" ? " - ${item.variantName}" : ""),
                                style: const TextStyle(fontSize: 13, color: Color(0xFF374151)),
                              ),
                            ),
                            Text("${item.durationMinutes} min", style: TextStyle(color: Colors.grey[500], fontSize: 12)),
                            const SizedBox(width: 8),
                            if (bundle.discountPercent > 0)
                              Text("${item.originalPrice.toInt()} zł",
                                  style: TextStyle(color: Colors.grey[400], fontSize: 12, decoration: TextDecoration.lineThrough)),
                          ],
                        ),
                      ))
                  .toList(),
            ),
          ),
          const Divider(height: 1),
          // Cena całkowita
          Padding(
            padding: const EdgeInsets.all(16),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Row(
                  children: [
                    Icon(Icons.schedule, size: 16, color: Colors.grey[400]),
                    const SizedBox(width: 4),
                    Text("Łącznie ~${bundle.totalDurationMinutes} min", style: TextStyle(color: Colors.grey[500], fontSize: 13)),
                  ],
                ),
                Row(
                  children: [
                    if (bundle.discountPercent > 0)
                      Padding(
                        padding: const EdgeInsets.only(right: 8),
                        child: Text("${bundle.originalTotalPrice.toInt()} zł",
                            style: TextStyle(color: Colors.grey[400], fontSize: 14, decoration: TextDecoration.lineThrough)),
                      ),
                    Text("${bundle.totalPrice.toInt()} zł", style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 18, color: Color(0xFF16a34a))),
                  ],
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
