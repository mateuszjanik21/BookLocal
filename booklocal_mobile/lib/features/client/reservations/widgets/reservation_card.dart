import 'package:flutter/material.dart';
import '../../../../core/models/reservation_models.dart';
import '../../../../core/models/business_list_item_dto.dart';
import '../../business_detail/business_details_screen.dart';
import 'package:intl/intl.dart';

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

  @override
  Widget build(BuildContext context) {
    // Formatowanie daty a'la Angularowa klasa Pipes!
    final dayFormatter = DateFormat('d', 'pl_PL');
    final monthYearFormatter = DateFormat('MMMM yyyy', 'pl_PL');
    final timeFormatter = DateFormat('EEEE, HH:mm', 'pl_PL');

    final day = dayFormatter.format(reservation.date);
    final monthYear = monthYearFormatter.format(reservation.date);
    final time = timeFormatter.format(reservation.date);

    final bool isConfirmed = reservation.status == 'confirmed';
    final bool isCancelled = reservation.status == 'cancelled';
    final bool isCompleted = reservation.status == 'completed';

    Color cardColor = isPast ? Colors.white.withOpacity(0.8) : Colors.white;

    return Card(
      elevation: 2,
      margin: const EdgeInsets.only(bottom: 16.0),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      color: cardColor,
      child: InkWell(
        borderRadius: BorderRadius.circular(16),
        onTap: () {
          final businessItem = BusinessListItemDto(
            id: reservation.businessId,
            name: reservation.businessName,
            category: reservation.serviceName,
            city: '',
            photoUrl: null,
            rating: 0,
            reviewCount: 0,
          );

          Navigator.push(
            context,
            MaterialPageRoute(
              builder: (context) => BusinessDetailsScreen(business: businessItem),
            ),
          );
        },
        child: IntrinsicHeight(
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // Lewa kolumna: DATA
            Container(
              width: 110,
              padding: const EdgeInsets.symmetric(vertical: 16, horizontal: 8),
              decoration: BoxDecoration(
                border: Border(right: BorderSide(color: Colors.grey.shade200)),
              ),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text(
                    day,
                    style: const TextStyle(fontSize: 28, fontWeight: FontWeight.bold, color: Colors.black87),
                  ),
                  Text(
                    monthYear,
                    textAlign: TextAlign.center,
                    style: const TextStyle(fontSize: 12, color: Colors.black54),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    time,
                    textAlign: TextAlign.center,
                    style: const TextStyle(fontSize: 11, color: Colors.black45),
                  ),
                ],
              ),
            ),
            // Prawa kolumna: DETALE LOGICZNE
            Expanded(
              child: Padding(
                padding: const EdgeInsets.all(16.0),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                reservation.serviceName,
                                maxLines: 3,
                                overflow: TextOverflow.ellipsis,
                                style: const TextStyle(
                                  fontSize: 16,
                                  fontWeight: FontWeight.bold,
                                  color: Color(0xFF16a34a),
                                ),
                              ),
                              const SizedBox(height: 4),
                              Text(
                                '${reservation.businessName} (${reservation.employeeName})',
                                maxLines: 2,
                                overflow: TextOverflow.ellipsis,
                                style: const TextStyle(fontSize: 13, color: Colors.black87),
                              ),
                            ],
                          ),
                        ),
                        const SizedBox(width: 12),
                        // Odznaka Statusu (Badge)
                        _buildStatusBadge(reservation.status, isConfirmed, isCancelled, isCompleted),
                      ],
                    ),
                    const Spacer(),
                    
                    // Renderowanie Opcjonalnych Guzików Akcji (Zależnie czy jesteśmy w dacie nadchodzącej, czy np. zakończono rezerwacje)
                    if (!isPast && isConfirmed && onCancel != null)
                      Align(
                        alignment: Alignment.centerRight,
                        child: OutlinedButton(
                          onPressed: onCancel,
                          style: OutlinedButton.styleFrom(
                            foregroundColor: Colors.red,
                            side: const BorderSide(color: Colors.red),
                            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                          ),
                          child: const Text('Anuluj'),
                        ),
                      ),
                    if (isPast && onReview != null)
                      Align(
                        alignment: Alignment.centerRight,
                        child: ElevatedButton(
                          onPressed: onReview,
                          style: ElevatedButton.styleFrom(
                            backgroundColor: const Color(0xFF16a34a),
                            foregroundColor: Colors.white,
                            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                          ),
                          child: const Text('Oceń wizytę'),
                        ),
                      ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
      ),
    );
  }

  // Użycie instrukcji funkcyjnej jako osobny mały pod-komponent, który sam renderuje kontener barwiący zamiast wyciągania go do nowego pliku
  Widget _buildStatusBadge(String status, bool isConfirmed, bool isCancelled, bool isCompleted) {
    Color bgColor = Colors.grey.shade200;
    Color textColor = Colors.black54;
    String label = status;

    if (isConfirmed) {
      bgColor = Colors.green.shade100;
      textColor = Colors.green.shade800;
      label = 'Potwierdzona';
    } else if (isCancelled) {
      bgColor = Colors.red.shade100;
      textColor = Colors.red.shade800;
      label = 'Anulowana';
    } else if (isCompleted) {
      bgColor = Colors.grey.shade300;
      textColor = Colors.black54;
      label = 'Zakończona';
    }

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: bgColor,
        borderRadius: BorderRadius.circular(8),
      ),
      child: Text(
        label,
        style: TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: textColor),
      ),
    );
  }
}
