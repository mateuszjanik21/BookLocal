import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import 'package:url_launcher/url_launcher.dart';
import '../../../../core/models/reservation_models.dart';
import '../../../../core/models/business_detail_dto.dart';
import '../../../../core/services/client_service.dart';
import '../chat/conversation_screen.dart';
import '../chat/providers/chat_provider.dart';
import '../../../../core/models/business_list_item_dto.dart';
import '../business_detail/business_details_screen.dart';
import 'providers/reservations_provider.dart';
import 'add_review_dialog.dart';

class ReservationDetailsScreen extends StatefulWidget {
  final ReservationDto reservation;

  const ReservationDetailsScreen({super.key, required this.reservation});

  @override
  State<ReservationDetailsScreen> createState() => _ReservationDetailsScreenState();
}

class _ReservationDetailsScreenState extends State<ReservationDetailsScreen> {
  BusinessDetailDto? _businessDetail;
  bool _isLoadingBusiness = true;

  @override
  void initState() {
    super.initState();
    _loadBusinessInfo();
  }

  Future<void> _loadBusinessInfo() async {
    try {
      final clientService = Provider.of<ClientService>(context, listen: false);
      final detail = await clientService.getBusinessById(widget.reservation.businessId);
      if (mounted) {
        setState(() {
          _businessDetail = detail;
          _isLoadingBusiness = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _isLoadingBusiness = false;
        });
      }
    }
  }

  Future<void> _launchUrl(String url) async {
    final uri = Uri.parse(url);
    try {
      await launchUrl(uri, mode: LaunchMode.externalApplication);
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text("Nie można otworzyć: $url")),
        );
      }
    }
  }

  void _openChat() async {
    final chatProvider = Provider.of<ChatProvider>(context, listen: false);
    final messenger = ScaffoldMessenger.of(context);
    final navigator = Navigator.of(context);

    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (ctx) => const Center(child: CircularProgressIndicator()),
    );

    try {
      final conversationId = await chatProvider.startConversation(widget.reservation.businessId);
      if (mounted) Navigator.pop(context); // Close loading

      if (conversationId != null && mounted) {
        navigator.push(
          MaterialPageRoute(
            builder: (context) => ConversationScreen(
              conversationId: conversationId,
              participantName: widget.reservation.businessName,
            ),
          ),
        );
      } else if (mounted) {
        messenger.showSnackBar(const SnackBar(content: Text("Nie udało się otworzyć czatu.")));
      }
    } catch (e) {
      if (mounted) {
        Navigator.pop(context);
        messenger.showSnackBar(const SnackBar(content: Text("Wystąpił błąd podczas otwierania czatu.")));
      }
    }
  }

  Future<void> _cancelReservation() async {
    final provider = Provider.of<ReservationsProvider>(context, listen: false);
    final messenger = ScaffoldMessenger.of(context);
    final navigator = Navigator.of(context);

    final confirm = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Anulować wizytę?'),
        content: Text(widget.reservation.isBundle 
          ? 'Czy na pewno chcesz anulować cały pakiet usług?' 
          : 'Czy na pewno chcesz anulować tę rezerwację?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('Nie', style: TextStyle(color: Colors.black54)),
          ),
          TextButton(
            onPressed: () => Navigator.pop(ctx, true),
            child: const Text('Tak, anuluj', style: TextStyle(color: Colors.red, fontWeight: FontWeight.bold)),
          ),
        ],
      ),
    );

    if (confirm == true && mounted) {
      final success = await provider.cancelReservation(widget.reservation.reservationId);
      if (success && mounted) {
        messenger.showSnackBar(
          const SnackBar(content: Text('Rezerwacja została anulowana.')),
        );
        navigator.pop(); // Wróć do listy
      }
    }
  }

  void _showReviewFlow() async {
    final res = widget.reservation;
    final provider = Provider.of<ReservationsProvider>(context, listen: false);

    int targetId = res.reservationId;
    String targetName = res.serviceName;

    if (res.isBundle && res.subItems != null && res.subItems!.isNotEmpty) {
      final selected = await showModalBottomSheet<BundleSubItemDto>(
        context: context,
        backgroundColor: Colors.white,
        shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(24))),
        builder: (ctx) => _BundleServicePickerDetails(items: res.subItems!),
      );

      if (selected == null) return;
      targetId = selected.reservationId;
      targetName = selected.serviceName;
    }

    if (!mounted) return;

    final result = await showModalBottomSheet<Map<String, dynamic>>(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (ctx) => AddReviewDialog(
        serviceName: targetName,
        businessName: res.businessName,
      ),
    );

    if (result != null && mounted) {
      final success = await provider.submitReview(
        reservationId: targetId,
        rating: result['rating'],
        comment: result['comment'],
      );

      if (success && mounted) {
        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text("Dziękujemy za Twoją opinię!")));
        Navigator.pop(context);
      }
    }
  }

  void _navigateToBusiness() {
    final res = widget.reservation;
    final businessData = BusinessListItemDto(
      id: res.businessId,
      name: res.businessName,
      category: 'Usługi', 
      city: _businessDetail?.city ?? '',
      photoUrl: _businessDetail?.photoUrl,
      rating: _businessDetail?.averageRating ?? 0.0,
      reviewCount: _businessDetail?.reviewCount ?? 0,
    );
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (ctx) => BusinessDetailsScreen(business: businessData),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final primaryColor = const Color(0xFF16a34a);
    final res = widget.reservation;
    
    final dayFormatter = DateFormat('EEEE, d MMMM yyyy', 'pl_PL');
    final timeFormatter = DateFormat('HH:mm', 'pl_PL');

    final bool isConfirmed = res.status == 'confirmed';
    final bool isUpcoming = res.isUpcoming;
    final bool isPast = !isUpcoming || res.status == 'completed' || res.status == 'cancelled';

    return Scaffold(
      backgroundColor: const Color(0xFFF3F4F6),
      appBar: AppBar(
        title: const Text("Szczegóły Wizyty", style: TextStyle(fontWeight: FontWeight.bold)),
        backgroundColor: Colors.white,
        foregroundColor: Colors.black,
        elevation: 0,
        centerTitle: true,
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(20),
        child: Column(
          children: [
            // 1. Status Section
            _buildStatusHeader(res, primaryColor),
            const SizedBox(height: 20),

            // 2. Service Details Card
            _buildCard(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                   Row(
                    children: [
                      Container(
                        padding: const EdgeInsets.all(10),
                        decoration: BoxDecoration(color: primaryColor.withOpacity(0.1), borderRadius: BorderRadius.circular(12)),
                        child: Icon(Icons.event_available, color: primaryColor),
                      ),
                      const SizedBox(width: 16),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              dayFormatter.format(res.startTime),
                              style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
                            ),
                            Text(
                              "Godzina ${timeFormatter.format(res.startTime)} - ${timeFormatter.format(res.endTime)}",
                              style: TextStyle(color: Colors.grey[600], fontSize: 14),
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),
                  const Padding(
                    padding: EdgeInsets.symmetric(vertical: 20),
                    child: Divider(height: 1),
                  ),
                  _buildDetailRow(Icons.spa_outlined, "Usługa", res.isBundle ? (res.bundleName ?? "Pakiet") : res.serviceName),
                  if (!res.isBundle) _buildDetailRow(Icons.category_outlined, "Wariant", res.variantName),
                  _buildDetailRow(Icons.person_outline, "Pracownik", res.employeeName),
                  _buildDetailRow(Icons.payments_outlined, "Cena", "${res.agreedPrice.toStringAsFixed(2)} zł", isLast: true),
                ],
              ),
            ),
            const SizedBox(height: 20),

            // 3. Business Info Card
            _buildCard(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Material(
                    color: Colors.transparent,
                    child: InkWell(
                      onTap: _navigateToBusiness,
                      borderRadius: BorderRadius.circular(12),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              const Text("Salon", style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                              Icon(Icons.arrow_forward_ios, size: 14, color: Colors.grey[400]),
                            ],
                          ),
                          const SizedBox(height: 16),
                          Row(
                            children: [
                              CircleAvatar(
                                radius: 25,
                                backgroundColor: Colors.grey[200],
                                backgroundImage: _businessDetail?.photoUrl != null 
                                  ? NetworkImage(_businessDetail!.photoUrl!) 
                                  : null,
                                child: _businessDetail?.photoUrl == null 
                                  ? const Icon(Icons.store, color: Colors.grey)
                                  : null,
                              ),
                              const SizedBox(width: 16),
                              Expanded(
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    Text(res.businessName, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                                    if (_isLoadingBusiness)
                                      const Text("Ładowanie adresu...", style: TextStyle(fontSize: 12, color: Colors.grey))
                                    else if (_businessDetail != null)
                                      Text("${_businessDetail!.address}, ${_businessDetail!.city}", style: TextStyle(fontSize: 14, color: Colors.grey[600]))
                                    else 
                                      Text("Adres niedostępny", style: TextStyle(fontSize: 14, color: Colors.grey[400])),
                                  ],
                                ),
                              ),
                            ],
                          ),
                        ],
                      ),
                    ),
                  ),
                  const SizedBox(height: 20),
                  Row(
                    children: [
                      if (!isPast) ...[
                        Expanded(
                          child: _buildActionButton(
                            Icons.location_on_outlined, 
                            "Nawiguj", 
                            () {
                              final addressSearch = _businessDetail != null 
                                ? "${_businessDetail!.address}, ${_businessDetail!.city}" 
                                : res.businessName;
                              _launchUrl("https://www.google.com/maps/search/?api=1&query=${Uri.encodeComponent(addressSearch)}");
                            },
                          ),
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          child: _buildActionButton(
                            Icons.phone_outlined, 
                            "Zadzwoń", 
                            _businessDetail != null && _businessDetail!.phoneNumber != null 
                                ? () => _launchUrl("tel:${_businessDetail!.phoneNumber}") 
                                : null,
                          ),
                        ),
                        const SizedBox(width: 12),
                      ],
                      Expanded(
                        child: _buildActionButton(
                          Icons.chat_bubble_outline, 
                          "Wiadomość", 
                          () => _openChat(),
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),
            
            const SizedBox(height: 40),

            // 4. Rate Button or Reviewed Status
            if (res.status == 'completed')
              if (!res.hasReview)
                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton(
                    onPressed: () => _showReviewFlow(),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: primaryColor,
                      foregroundColor: Colors.white,
                      padding: const EdgeInsets.symmetric(vertical: 16),
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                      elevation: 0,
                    ),
                    child: const Text("OCEŃ TĘ WIZYTĘ", style: TextStyle(fontWeight: FontWeight.bold, letterSpacing: 1)),
                  ),
                )
              else
                _buildCard(
                  child: Row(
                    children: [
                      Container(
                        padding: const EdgeInsets.all(12),
                        decoration: BoxDecoration(color: Colors.amber.withOpacity(0.1), shape: BoxShape.circle),
                        child: const Icon(Icons.star, color: Colors.amber, size: 28),
                      ),
                      const SizedBox(width: 16),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            const Text("Wizyta oceniona", style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                            Text("Dziękujemy za Twoją opinię!", style: TextStyle(color: Colors.grey[600], fontSize: 14)),
                          ],
                        ),
                      ),
                    ],
                  ),
                ),

            // 5. Cancel Button
            if (isUpcoming && isConfirmed)
              SizedBox(
                width: double.infinity,
                child: TextButton(
                  onPressed: () => _cancelReservation(),
                  style: TextButton.styleFrom(
                    padding: const EdgeInsets.symmetric(vertical: 16),
                    foregroundColor: Colors.red,
                    backgroundColor: Colors.red.withOpacity(0.05),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                  ),
                  child: const Text("ANULUJ REZERWACJĘ", style: TextStyle(fontWeight: FontWeight.bold, letterSpacing: 1)),
                ),
              ),
            
            const SizedBox(height: 20),
          ],
        ),
      ),
    );
  }

  Widget _buildStatusHeader(ReservationDto res, Color primaryColor) {
    Color statusColor = Colors.grey;
    String statusLabel = res.status.toUpperCase();
    IconData statusIcon = Icons.help_outline;

    if (res.status == 'confirmed') {
      statusColor = primaryColor;
      statusLabel = "POTWIERDZONA";
      statusIcon = Icons.check_circle_outline;
    } else if (res.status == 'cancelled') {
      statusColor = Colors.red;
      statusLabel = "ANULOWANA";
      statusIcon = Icons.cancel_outlined;
    } else if (res.status == 'completed') {
      statusColor = Colors.blueGrey;
      statusLabel = "ZAKOŃCZONA";
      statusIcon = Icons.task_alt;
    }

    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(24),
      decoration: BoxDecoration(
        color: statusColor,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(color: statusColor.withOpacity(0.3), blurRadius: 10, offset: const Offset(0, 4)),
        ],
      ),
      child: Column(
        children: [
          Icon(statusIcon, color: Colors.white, size: 48),
          const SizedBox(height: 12),
          Text(
            statusLabel,
            style: const TextStyle(color: Colors.white, fontWeight: FontWeight.w900, fontSize: 18, letterSpacing: 2),
          ),
          const SizedBox(height: 4),
          Text(
            res.isBundle ? "Rezerwacja pakietowa" : "Rezerwacja indywidualna",
            style: TextStyle(color: Colors.white.withOpacity(0.8), fontSize: 13),
          ),
        ],
      ),
    );
  }

  Widget _buildCard({required Widget child}) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(color: Colors.black.withOpacity(0.04), blurRadius: 10, offset: const Offset(0, 5)),
        ],
      ),
      child: child,
    );
  }

  Widget _buildDetailRow(IconData icon, String label, String value, {bool isLast = false}) {
    return Padding(
      padding: EdgeInsets.only(bottom: isLast ? 0 : 16),
      child: Row(
        children: [
          Icon(icon, size: 18, color: Colors.grey[400]),
          const SizedBox(width: 12),
          Text(label, style: TextStyle(color: Colors.grey[600], fontSize: 14)),
          const Spacer(),
          Text(value, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14)),
        ],
      ),
    );
  }

  Widget _buildActionButton(IconData icon, String label, VoidCallback? onTap) {
    return OutlinedButton.icon(
      onPressed: onTap,
      icon: Icon(icon, size: 18),
      label: Text(label, style: const TextStyle(fontSize: 12, fontWeight: FontWeight.bold)),
      style: OutlinedButton.styleFrom(
        padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 8),
        foregroundColor: Colors.black87,
        side: BorderSide(color: Colors.grey[300]!),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      ),
    );
  }
}

class _BundleServicePickerDetails extends StatelessWidget {
  final List<BundleSubItemDto> items;

  const _BundleServicePickerDetails({required this.items});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(vertical: 20),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Padding(
            padding: EdgeInsets.symmetric(horizontal: 24, vertical: 8),
            child: Text(
              "Wybierz usługę do oceny",
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
          ),
          const SizedBox(height: 8),
          ...items.map((item) => ListTile(
                leading: const Icon(Icons.check_circle_outline, color: Color(0xFF16a34a)),
                title: Text(item.serviceName, style: const TextStyle(fontWeight: FontWeight.bold)),
                subtitle: Text(item.variantName),
                onTap: () => Navigator.pop(context, item),
              )),
          const SizedBox(height: 20),
        ],
      ),
    );
  }
}
