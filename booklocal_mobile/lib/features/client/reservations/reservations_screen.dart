import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../core/services/reservation_service.dart';
import 'providers/reservations_provider.dart';
import 'widgets/reservation_card.dart';


class ReservationsScreen extends StatelessWidget {
  const ReservationsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    // 1. Owijamy ekran w naszego Providera zbudowanego w Kroku 1
    return ChangeNotifierProvider(
      create: (context) {
        final service = Provider.of<ReservationService>(context, listen: false);
        return ReservationsProvider(service)..init();
      },
      // 2. DefaultTabController zajmuje się matematyką przełączania między kartami!
      child: DefaultTabController(
        length: 2,
        child: Scaffold(
          backgroundColor: const Color(0xFFF8FAF9),
          appBar: AppBar(
            backgroundColor: Colors.white,
            elevation: 0,
            title: const Text(
              'Moje Rezerwacje',
              style: TextStyle(color: Colors.black87, fontWeight: FontWeight.bold),
            ),
            // Pasek zakładek
            bottom: const TabBar(
              labelColor: Color(0xFF16a34a),
              unselectedLabelColor: Colors.black54,
              indicatorColor: Color(0xFF16a34a),
              tabs: [
                Tab(text: 'Nadchodzące wizyty'),
                Tab(text: 'Historia wizyt'),
              ],
            ),
          ),
          // 3. To ten element zaciąga widoki do swoich zakładek
          body: const TabBarView(
            children: [
              _UpcomingTab(),
              _PastTab(),
            ],
          ),
        ),
      ),
    );
  }
}

// ------ ZAKŁADKA 1 ------
class _UpcomingTab extends StatelessWidget {
  const _UpcomingTab();

  @override
  Widget build(BuildContext context) {
    // Nasłuchujemy zmian od Providera (czyli wywołań komendy notifyListeners).
    final provider = Provider.of<ReservationsProvider>(context);

    if (provider.isLoadingUpcoming) {
      return const Center(child: CircularProgressIndicator(color: Color(0xFF16a34a)));
    }

    if (provider.upcomingReservations.isEmpty) {
      return const Center(child: Text('Nie masz żadnych nadchodzących rezerwacji.'));
    }

    // Dynamiczna lista 
    return ListView.builder(
      padding: const EdgeInsets.all(16.0),
      itemCount: provider.upcomingReservations.length,
      itemBuilder: (context, index) {
        final res = provider.upcomingReservations[index];
        return ReservationCard(
          reservation: res,
          isPast: false,
          onCancel: () async {
            final confirm = await showDialog<bool>(
              context: context,
              builder: (ctx) => AlertDialog(
                title: const Text('Anulować?'),
                content: const Text('Czy na pewno chcesz anulować tę rezerwację?'),
                actions: [
                  TextButton(
                    onPressed: () => Navigator.pop(ctx, false),
                    child: const Text('Nie', style: TextStyle(color: Colors.black54)),
                  ),
                  TextButton(
                    onPressed: () => Navigator.pop(ctx, true),
                    child: const Text('Tak, anuluj', style: TextStyle(color: Colors.red)),
                  ),
                ],
              ),
            );

            // Gdy ktoś potwierdzi AlertDialog - odsyłamy polecenie do Providera!
            if (confirm == true) {
              final success = await provider.cancelReservation(res.reservationId);
              if (success && context.mounted) {
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text('Twoja rezerwacja została anulowana.')),
                );
              } else if (context.mounted) {
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text('Nie udało się anulować rezerwacji.')),
                );
              }
            }
          },
        );
      },
    );
  }
}

// ------ ZAKŁADKA 2 ------
class _PastTab extends StatelessWidget {
  const _PastTab();

  @override
  Widget build(BuildContext context) {
    // Kolejny nasłuchiwacz
    final provider = Provider.of<ReservationsProvider>(context);

    if (provider.isLoadingPast) {
      return const Center(child: CircularProgressIndicator(color: Color(0xFF16a34a)));
    }

    if (provider.pastReservations.isEmpty) {
      return const Center(child: Text('Nie masz żadnych historycznych rezerwacji.'));
    }

    return ListView.builder(
      padding: const EdgeInsets.all(16.0),
      itemCount: provider.pastReservations.length,
      itemBuilder: (context, index) {
        final res = provider.pastReservations[index];
        return ReservationCard(
          reservation: res,
          isPast: true,
          onReview: res.status == 'completed' && !res.hasReview
              ? () {
                  // TODO: W 8 fazie modal do oceniania!
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text('Ocenianie pojawi się w kolejnym etapie!')),
                  );
                }
              : null,
        );
      },
    );
  }
}