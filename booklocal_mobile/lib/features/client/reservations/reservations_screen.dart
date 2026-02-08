import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../core/models/reservation_models.dart';
import '../../../core/services/reservation_service.dart';
import '../../../core/services/review_service.dart';
import 'add_review_dialog.dart';

class ReservationsScreen extends StatefulWidget {
  final int initialIndex;
  const ReservationsScreen({super.key, this.initialIndex = 0});

  @override
  State<ReservationsScreen> createState() => _ReservationsScreenState();
}

class _ReservationsScreenState extends State<ReservationsScreen> {
  late Future<List<ReservationDto>> _upcomingFuture;
  late Future<List<ReservationDto>> _historyFuture;

  @override
  void initState() {
    super.initState();
    _loadReservations();
  }

  void _loadReservations() {
    final service = Provider.of<ReservationService>(context, listen: false);
    setState(() {
      _upcomingFuture = service.getMyReservations(scope: 'upcoming');
      _historyFuture = service.getMyReservations(scope: 'past');
    });
  }

  Future<void> _showReviewDialog(int reservationId) async {
    final result = await showDialog<Map<String, dynamic>>(
      context: context,
      builder: (context) => const AddReviewDialog(),
    );

    if (result != null) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text("Wysyłanie opinii...")));

      final reviewService = Provider.of<ReviewService>(context, listen: false);
      final success = await reviewService.addReview(
        reservationId,
        result['rating'],
        result['comment'],
      );

      if (!mounted) return;

      if (success) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text("Dziękujemy za opinię! ⭐"), backgroundColor: Colors.green),
        );
        _loadReservations();
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text("Błąd wysyłania opinii."), backgroundColor: Colors.red),
        );
      }
    }
  }

  Future<void> _showCancelDialog(int reservationId) async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text("Anulowanie wizyty"),
        content: const Text("Czy na pewno chcesz anulować tę rezerwację?"),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text("Nie", style: TextStyle(color: Colors.grey)),
          ),
          TextButton(
            onPressed: () => Navigator.pop(context, true),
            style: TextButton.styleFrom(foregroundColor: Colors.red),
            child: const Text("Tak, anuluj"),
          ),
        ],
      ),
    );

    if (confirm == true) {
      if (!mounted) return;
      
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("Trwa anulowanie...")),
      );

      final service = Provider.of<ReservationService>(context, listen: false);
      final success = await service.cancelReservation(reservationId);

      if (!mounted) return;

      if (success) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text("Rezerwacja została anulowana"), backgroundColor: Colors.green),
        );
        _loadReservations();
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text("Błąd podczas anulowania"), backgroundColor: Colors.red),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return DefaultTabController(
      length: 2,
      initialIndex: 0, 
      child: Scaffold(
        appBar: AppBar(
          title: const Text("Moje Wizyty"),
          centerTitle: false,
          backgroundColor: Colors.white,
          elevation: 0,
          foregroundColor: Colors.black,
          bottom: const TabBar(
            labelColor: Color(0xFF16a34a),
            unselectedLabelColor: Colors.grey,
            indicatorColor: Color(0xFF16a34a),
            tabs: [
              Tab(text: "Nadchodzące"),
              Tab(text: "Historia"),
            ],
          ),
        ),
        body: TabBarView(
          children: [
            _buildTabContent(_upcomingFuture, isHistory: false),
            _buildTabContent(_historyFuture, isHistory: true),
          ],
        ),
      ),
    );
  }

  Widget _buildTabContent(Future<List<ReservationDto>> future, {required bool isHistory}) {
    return FutureBuilder<List<ReservationDto>>(
      future: future,
      builder: (context, snapshot) {
        if (snapshot.connectionState == ConnectionState.waiting) {
          return const Center(child: CircularProgressIndicator());
        } else if (snapshot.hasError) {
          return Center(child: Text("Błąd: ${snapshot.error}"));
        } else if (!snapshot.hasData || snapshot.data!.isEmpty) {
          return Center(
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Icon(isHistory ? Icons.history : Icons.calendar_today, size: 60, color: Colors.grey[300]),
                const SizedBox(height: 10),
                Text(
                  isHistory ? "Brak historii wizyt" : "Brak nadchodzących wizyt",
                  style: TextStyle(color: Colors.grey[500]),
                ),
              ],
            ),
          );
        }

        final reservations = snapshot.data!;
        return ListView.builder(
          padding: const EdgeInsets.all(16),
          itemCount: reservations.length,
          itemBuilder: (context, index) {
            return _buildReservationCard(reservations[index]);
          },
        );
      },
    );
  }

  Widget _buildReservationCard(ReservationDto res) {
    final timeStr = "${res.date.hour}:${res.date.minute.toString().padLeft(2, '0')}";

    Color statusColor;
    String statusText;

    switch (res.status.toLowerCase()) {
      case 'confirmed':
        statusColor = Colors.green;
        statusText = "Potwierdzona";
        break;
      case 'pending':
        statusColor = Colors.orange;
        statusText = "Oczekuje";
        break;
      case 'cancelled':
        statusColor = Colors.red;
        statusText = "Anulowana";
        break;
      case 'completed':
        statusColor = Colors.blue;
        statusText = "Zakończona";
        break;
      default:
        statusColor = Colors.grey;
        statusText = res.status;
    }

    return Container(
      margin: const EdgeInsets.only(bottom: 16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: Colors.grey.withOpacity(0.08),
            blurRadius: 8,
            offset: const Offset(0, 3),
          ),
        ],
      ),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            Row(
              children: [
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                  decoration: BoxDecoration(
                    color: Colors.grey[100],
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Column(
                    children: [
                      Text(
                        "${res.date.day}",
                        style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 20),
                      ),
                      Text(
                        "${res.date.month}",
                        style: TextStyle(color: Colors.grey[600], fontSize: 12),
                      ),
                    ],
                  ),
                ),
                const SizedBox(width: 16),
                
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(res.businessName, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                      const SizedBox(height: 4),
                      Text(res.serviceName, style: TextStyle(color: Colors.grey[600], fontSize: 14)),
                      const SizedBox(height: 4),
                      Row(
                        children: [
                          Icon(Icons.access_time, size: 14, color: Colors.grey[500]),
                          const SizedBox(width: 4),
                          Text(timeStr, style: TextStyle(color: Colors.grey[600], fontSize: 13)),
                        ],
                      ),
                    ],
                  ),
                ),

                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                  decoration: BoxDecoration(
                    color: statusColor.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Text(
                    statusText,
                    style: TextStyle(color: statusColor, fontSize: 11, fontWeight: FontWeight.bold),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 12),
            const Divider(),

            Row(
              mainAxisAlignment: MainAxisAlignment.end,
              children: [
                if (res.isUpcoming)
                  TextButton(
                    onPressed: () => _showCancelDialog(res.reservationId),
                    style: TextButton.styleFrom(foregroundColor: Colors.red),
                    child: const Text("Anuluj"),
                  )
                else
                  Row(
                    children: [
                      if (!res.hasReview && res.status.toLowerCase() == 'completed')
                        TextButton.icon(
                          onPressed: () => _showReviewDialog(res.reservationId),
                          icon: const Icon(Icons.star_outline, size: 18),
                          label: const Text("Oceń"),
                          style: TextButton.styleFrom(foregroundColor: Colors.amber[800]),
                        ),
                      
                      TextButton(
                        onPressed: () {
                          ScaffoldMessenger.of(context).showSnackBar(
                            const SnackBar(content: Text("Funkcja ponownego umawiania wkrótce")),
                          );
                        },
                        child: const Text("Umów ponownie"),
                      ),
                    ],
                  ),
              ],
            )
          ],
        ),
      ),
    );
  }
}