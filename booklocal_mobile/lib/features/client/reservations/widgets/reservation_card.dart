import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../../../core/models/reservation_models.dart';
import 'package:provider/provider.dart';
import '../providers/reservations_provider.dart';
import '../reservation_details_screen.dart';

class ReservationCard extends StatelessWidget {
  final ReservationDto reservation;
  final bool isPast;
  final VoidCallback? onCancel;
  final VoidCallback? onReview;

  const ReservationCard({
    super.key,
    required this.reservation,
    required this.isPast,
    this.onCancel,
    this.onReview,
  });

  String _getRelativeDateText(DateTime date) {
    final now = DateTime.now();
    final today = DateTime(now.year, now.month, now.day);
    final target = DateTime(date.year, date.month, date.day);
    final difference = target.difference(today).inDays;

    if (difference == 0) return "Dziś";
    if (difference == 1) return "Jutro";
    if (difference == 2) return "Pojutrze";
    if (difference > 2 && difference <= 5) return "Za $difference dni";
    
    final dayFormatter = DateFormat('d', 'pl_PL');
    final monthFormatter = DateFormat('MMM', 'pl_PL');
    return "${dayFormatter.format(date)}\n${monthFormatter.format(date).toUpperCase().replaceAll('.', '')}";
  }

  @override
  Widget build(BuildContext context) {
    final timeFormatter = DateFormat('HH:mm', 'pl_PL');

    final bool isConfirmed = reservation.status == 'confirmed';
    final bool isCancelled = reservation.status == 'cancelled';
    final bool isCompleted = reservation.status == 'completed';

    const Color primaryColor = Color(0xFF16a34a);
    
    Color accentColor = primaryColor;
    if (isCancelled) accentColor = Colors.red;
    if (isCompleted || isPast) accentColor = Colors.grey.shade400;

    final String relativeDate = _getRelativeDateText(reservation.startTime);
    final time = timeFormatter.format(reservation.startTime);

    void goToDetails() {
      final provider = Provider.of<ReservationsProvider>(context, listen: false);
      Navigator.push(
        context,
        MaterialPageRoute(
          builder: (context) => ChangeNotifierProvider.value(
            value: provider,
            child: ReservationDetailsScreen(reservation: reservation),
          ),
        ),
      );
    }

    return Container(
      margin: const EdgeInsets.only(bottom: 16.0),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.04),
            blurRadius: 10,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: ClipRRect(
        borderRadius: BorderRadius.circular(16),
        child: IntrinsicHeight(
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Container(width: 5, color: accentColor),
              
              Expanded(
                child: Material(
                  color: Colors.transparent,
                  child: InkWell(
                    onTap: goToDetails,
                    child: Padding(
                      padding: const EdgeInsets.fromLTRB(12, 16, 8, 16),
                      child: Row(
                        children: [
                          // Sekcja daty z relatywnym tekstem
                          SizedBox(
                            width: 74,
                            child: Column(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                Text(
                                  relativeDate, 
                                  style: TextStyle(
                                    fontSize: relativeDate.contains("\n") ? 13 : 15, 
                                    fontWeight: relativeDate == "Dziś" || relativeDate == "Jutro" ? FontWeight.w900 : FontWeight.bold, 
                                    color: relativeDate == "Dziś" ? primaryColor : (isPast ? Colors.grey : Colors.black87),
                                    height: 1.2,
                                  ),
                                  textAlign: TextAlign.center,
                                ),
                                const SizedBox(height: 6),
                                Text(
                                  time, 
                                  style: const TextStyle(fontSize: 13, fontWeight: FontWeight.bold, color: Colors.black54)
                                ),
                              ],
                            ),
                          ),
                          
                          Container(
                            width: 1,
                            margin: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                            color: Colors.grey.shade100,
                          ),
                          
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                _buildStatusBadge(reservation.status, isConfirmed, isCancelled, isCompleted),
                                const SizedBox(height: 6),
                                Text(
                                  reservation.isBundle ? (reservation.bundleName ?? 'Pakiet') : reservation.serviceName, 
                                  maxLines: 1, 
                                  overflow: TextOverflow.ellipsis, 
                                  style: const TextStyle(fontSize: 15, fontWeight: FontWeight.bold, color: Colors.black87)
                                ),
                                const SizedBox(height: 2),
                                Text(
                                  reservation.businessName, 
                                  maxLines: 1, 
                                  overflow: TextOverflow.ellipsis, 
                                  style: TextStyle(fontSize: 13, color: Colors.grey[600])
                                ),
                                const SizedBox(height: 6),
                                Row(
                                  children: [
                                    Icon(Icons.person_outline, size: 14, color: Colors.grey[400]),
                                    const SizedBox(width: 4),
                                    Text(reservation.employeeName, style: const TextStyle(fontSize: 12, color: Colors.black54)),
                                  ],
                                ),
                              ],
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                ),
              ),
              
              if (isPast && isCompleted && !reservation.hasReview && onReview != null)
                Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 12),
                  child: Center(
                    child: GestureDetector(
                      onTap: () {
                        onReview?.call();
                      },
                      behavior: HitTestBehavior.opaque,
                      child: Container(
                        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
                        decoration: BoxDecoration(
                          color: primaryColor.withOpacity(0.1),
                          borderRadius: BorderRadius.circular(8),
                          border: Border.all(color: primaryColor.withOpacity(0.3)),
                        ),
                        child: const Text("OCEŃ", style: TextStyle(fontSize: 11, fontWeight: FontWeight.w900, color: primaryColor)),
                      ),
                    ),
                  ),
                )
              else if (!isPast && isConfirmed)
                Padding(
                  padding: const EdgeInsets.only(right: 16),
                  child: Center(
                    child: Material(
                      color: Colors.transparent,
                      child: IconButton(
                        onPressed: goToDetails,
                        icon: const Icon(Icons.arrow_forward_ios, size: 14, color: Colors.grey),
                      ),
                    ),
                  ),
                ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildStatusBadge(String status, bool isConfirmed, bool isCancelled, bool isCompleted) {
    Color color = Colors.grey;
    String label = status.toUpperCase();
    if (isConfirmed) {
      color = const Color(0xFF16a34a);
      label = "ZATWIERDZONA";
    } else if (isCancelled) {
      color = Colors.red;
      label = "ANULOWANA";
    } else if (isCompleted) {
      color = Colors.blueGrey;
      label = "ZAKOŃCZONA";
    }
    return Row(
      children: [
        Container(width: 6, height: 6, decoration: BoxDecoration(color: color, shape: BoxShape.circle)),
        const SizedBox(width: 6),
        Text(label, style: TextStyle(fontSize: 10, fontWeight: FontWeight.w800, color: color.withOpacity(0.8), letterSpacing: 0.5)),
      ],
    );
  }
}
