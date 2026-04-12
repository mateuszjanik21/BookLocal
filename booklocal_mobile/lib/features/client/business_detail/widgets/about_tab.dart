import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher_string.dart';
import 'package:shimmer/shimmer.dart';
import '../../../../core/models/business_detail_dto.dart';
import 'section_card.dart';

class AboutTab extends StatelessWidget {
  final BusinessDetailDto? fullBusiness;
  final String fallbackCity;
  final bool isLoading;
  final VoidCallback onStartChat;

  const AboutTab({
    super.key,
    required this.fullBusiness,
    required this.fallbackCity,
    required this.isLoading,
    required this.onStartChat,
  });

  @override
  Widget build(BuildContext context) {
    final description = fullBusiness?.description ??
        (fallbackCity.isNotEmpty
            ? "Zapraszamy do naszego salonu w mieście $fallbackCity. Oferujemy profesjonalne usługi najwyższej jakości."
            : "Witamy w naszym salonie!");

    final address = fullBusiness?.address ?? fallbackCity;
    final phoneNumber = fullBusiness?.phoneNumber;
    final ownerName = [fullBusiness?.ownerFirstName, fullBusiness?.ownerLastName]
        .where((s) => s != null && s.isNotEmpty)
        .join(' ');

    return SingleChildScrollView(
      physics: const BouncingScrollPhysics(parent: AlwaysScrollableScrollPhysics()),
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: SectionCard(
          title: "O Nas",
          icon: Icons.info_outline,
          child: AnimatedSwitcher(
            duration: const Duration(milliseconds: 300),
            child: isLoading
                ? _buildSkeletonAbout()
                : Column(
                    key: const ValueKey('about_loaded'),
                    mainAxisSize: MainAxisSize.min,
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        description,
                        style: TextStyle(color: Colors.grey[700], height: 1.5, fontSize: 14),
                        textAlign: TextAlign.justify,
                      ),
                      const SizedBox(height: 20),
                      if (ownerName.isNotEmpty || address.isNotEmpty || (phoneNumber != null && phoneNumber.isNotEmpty))
                        Container(
                          padding: const EdgeInsets.all(16),
                          decoration: BoxDecoration(
                            color: Colors.grey[50],
                            borderRadius: BorderRadius.circular(12),
                            border: Border.all(color: Colors.grey[200]!),
                          ),
                          child: Column(
                            children: [
                              if (ownerName.isNotEmpty) ...[
                                Row(
                                  children: [
                                    const Icon(Icons.person, color: Color(0xFF16a34a), size: 20),
                                    const SizedBox(width: 12),
                                    Expanded(
                                      child: Column(
                                        crossAxisAlignment: CrossAxisAlignment.start,
                                        children: [
                                          const Text("Właściciel", style: TextStyle(fontSize: 12, color: Colors.grey)),
                                          Text(ownerName, style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w600)),
                                        ],
                                      ),
                                    ),
                                  ],
                                ),
                                if (address.isNotEmpty || (phoneNumber != null && phoneNumber.isNotEmpty))
                                  const Divider(height: 24),
                              ],
                              if (address.isNotEmpty) ...[
                                InkWell(
                                  onTap: () {
                                    final query = Uri.encodeComponent(address);
                                    launchUrlString("https://www.google.com/maps/search/?api=1&query=$query");
                                  },
                                  child: Padding(
                                    padding: const EdgeInsets.symmetric(vertical: 4.0),
                                    child: Row(
                                      children: [
                                        const Icon(Icons.location_on, color: Color(0xFF16a34a), size: 20),
                                        const SizedBox(width: 12),
                                        Expanded(child: Text(address, style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w500))),
                                      ],
                                    ),
                                  ),
                                ),
                              ],
                              if (phoneNumber != null && phoneNumber.isNotEmpty) ...[
                                if (address.isNotEmpty) const Divider(height: 24),
                                InkWell(
                                  onTap: () {
                                    launchUrlString("tel:${phoneNumber.replaceAll(' ', '')}");
                                  },
                                  child: Padding(
                                    padding: const EdgeInsets.symmetric(vertical: 4.0),
                                    child: Row(
                                      children: [
                                        const Icon(Icons.phone, color: Color(0xFF16a34a), size: 20),
                                        const SizedBox(width: 12),
                                        Expanded(child: Text(phoneNumber, style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w500, color: Color(0xFF16a34a)))),
                                      ],
                                    ),
                                  ),
                                ),
                              ],
                            ],
                          ),
                        ),
                      const SizedBox(height: 20),
                      Row(
                        children: [
                          Expanded(
                            child: OutlinedButton.icon(
                              onPressed: onStartChat,
                              icon: const Icon(Icons.chat_bubble_outline, size: 18),
                              label: const Text("Napisz"),
                              style: OutlinedButton.styleFrom(
                                foregroundColor: const Color(0xFF16a34a),
                                side: const BorderSide(color: Color(0xFF16a34a)),
                                padding: const EdgeInsets.symmetric(vertical: 12),
                                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                              ),
                            ),
                          ),
                          const SizedBox(width: 12),
                          Expanded(
                            child: ElevatedButton.icon(
                              onPressed: () {
                                if (address.isNotEmpty) {
                                  final query = Uri.encodeComponent(address);
                                  launchUrlString("https://www.google.com/maps/search/?api=1&query=$query");
                                } else {
                                  ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text("Brak dokładnego adresu.")));
                                }
                              },
                              icon: const Icon(Icons.directions, size: 18),
                              label: const Text("Trasa"),
                              style: ElevatedButton.styleFrom(
                                backgroundColor: const Color(0xFF16a34a),
                                foregroundColor: Colors.white,
                                padding: const EdgeInsets.symmetric(vertical: 12),
                                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                                elevation: 0,
                              ),
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
          ),
        ),
      ),
    );
  }

  Widget _buildSkeletonAbout() {
    return Shimmer.fromColors(
      key: const ValueKey('about_skeleton'),
      baseColor: Colors.grey.shade300,
      highlightColor: Colors.grey.shade100,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(width: double.infinity, height: 14, color: Colors.white),
          const SizedBox(height: 6),
          Container(width: double.infinity, height: 14, color: Colors.white),
          const SizedBox(height: 6),
          Container(width: 200, height: 14, color: Colors.white),
          const SizedBox(height: 24),
          Container(
            width: double.infinity,
            height: 120,
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(12),
            ),
          ),
          const SizedBox(height: 24),
          Row(
            children: [
              Expanded(child: Container(height: 45, decoration: BoxDecoration(color: Colors.white, borderRadius: BorderRadius.circular(10)))),
              const SizedBox(width: 12),
              Expanded(child: Container(height: 45, decoration: BoxDecoration(color: Colors.white, borderRadius: BorderRadius.circular(10)))),
            ],
          )
        ],
      ),
    );
  }
}
