import 'package:flutter/material.dart';
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
    if (isLoading) {
      return const Center(child: CircularProgressIndicator());
    }

    final description = fullBusiness?.description ??
        (fallbackCity.isNotEmpty
            ? "Zapraszamy do naszego salonu w mieście $fallbackCity. Oferujemy profesjonalne usługi najwyższej jakości."
            : "Witamy w naszym salonie!");

    final address = fullBusiness?.address ?? fallbackCity;
    final phoneNumber = fullBusiness?.phoneNumber;

    return LayoutBuilder(
      builder: (context, constraints) {
        return Padding(
          padding: const EdgeInsets.fromLTRB(16, 16, 16, 16),
          child: SectionCard(
            title: "O Nas",
            icon: Icons.info_outline,
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  description,
                  style: TextStyle(color: Colors.grey[700], height: 1.5, fontSize: 14),
                  textAlign: TextAlign.justify,
                ),
                const SizedBox(height: 20),
                if (address.isNotEmpty || (phoneNumber != null && phoneNumber.isNotEmpty))
                  Container(
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      color: Colors.grey[50],
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(color: Colors.grey[200]!),
                    ),
                    child: Column(
                      children: [
                        if (address.isNotEmpty) ...[
                          Row(
                            children: [
                              const Icon(Icons.location_on, color: Color(0xFF16a34a), size: 20),
                              const SizedBox(width: 12),
                              Expanded(child: Text(address, style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w500))),
                            ],
                          ),
                        ],
                        if (phoneNumber != null && phoneNumber.isNotEmpty) ...[
                          if (address.isNotEmpty) const Divider(height: 24),
                          Row(
                            children: [
                              const Icon(Icons.phone, color: Color(0xFF16a34a), size: 20),
                              const SizedBox(width: 12),
                              Expanded(child: Text(phoneNumber, style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w500))),
                            ],
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
                          ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text("Otwieranie mapy...")));
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
        );
      },
    );
  }
}
