import 'package:flutter/material.dart';
import '../../../../core/models/reservation_models.dart';
import '../../../../core/services/reservation_service.dart';

class ReservationsProvider extends ChangeNotifier {
  final ReservationService _reservationService;

  // Nasze schowki na dane z serwera (zamiast trzymać je w UI)
  List<ReservationDto> upcomingReservations = [];
  List<ReservationDto> pastReservations = [];

  // Flagi ładowania do kręciołków
  bool isLoadingUpcoming = true;
  bool isLoadingPast = true;

  // Wstrzykiwanie serwisu po to, by Provider umiał "rozmawiać z serwerem"
  ReservationsProvider(this._reservationService);

  // Pierwsze odpalenie wywołuje ściąganie obu list
  Future<void> init() async {
    loadUpcomingReservations();
    loadPastReservations();
  }

  Future<void> loadUpcomingReservations() async {
    isLoadingUpcoming = true;
    notifyListeners(); // Każe ekranowi pokazać loading

    try {
      final results = await _reservationService.getMyReservations(scope: 'upcoming');
      upcomingReservations = results;
    } catch (e) {
      upcomingReservations = [];
    } finally {
      isLoadingUpcoming = false;
      notifyListeners(); // Zakończyliśmy - ekran przerysuje powłokę bez animacji kręcenia
    }
  }

  Future<void> loadPastReservations() async {
    isLoadingPast = true;
    notifyListeners();

    try {
      final results = await _reservationService.getMyReservations(scope: 'past');
      pastReservations = results;
    } catch (e) {
      pastReservations = [];
    } finally {
      isLoadingPast = false;
      notifyListeners();
    }
  }

  Future<bool> cancelReservation(int reservationId) async {
    final success = await _reservationService.cancelReservation(reservationId);
    if (success) {
      // Jeśli sie udało – po prostu odświeżamy tablice rezerwacji!
      await loadUpcomingReservations();
      await loadPastReservations();
    }
    return success;
  }
}
