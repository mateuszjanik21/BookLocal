import 'package:flutter/material.dart';
import '../../../../core/models/reservation_models.dart';
import '../../../../core/services/reservation_service.dart';

class ReservationsProvider extends ChangeNotifier {
  final ReservationService _reservationService;

  List<ReservationDto> upcomingReservations = [];
  List<ReservationDto> pastReservations = [];

  bool isLoadingUpcoming = true;
  bool isLoadingPast = true;
  bool isLoadingMorePast = false;

  int _pastPageNumber = 1;
  bool _hasMorePast = true;
  final int _pageSize = 10;

  bool get hasMorePast => _hasMorePast;

  ReservationsProvider(this._reservationService);

  Future<void> fetchUpcoming() => loadUpcomingReservations();
  Future<void> fetchPast() => loadPastReservations();

  Future<void> init() async {
    loadUpcomingReservations();
    loadPastReservations();
  }

  Future<void> loadUpcomingReservations() async {
    isLoadingUpcoming = true;
    notifyListeners();

    try {
      final results = await _reservationService.getMyReservations(scope: 'upcoming', pageNumber: 1, pageSize: 50);
      upcomingReservations = results;
    } catch (e) {
      upcomingReservations = [];
    } finally {
      isLoadingUpcoming = false;
      notifyListeners();
    }
  }

  Future<void> loadPastReservations() async {
    isLoadingPast = true;
    _pastPageNumber = 1;
    _hasMorePast = true;
    notifyListeners();

    try {
      final results = await _reservationService.getMyReservations(
        scope: 'past', 
        pageNumber: _pastPageNumber, 
        pageSize: _pageSize
      );
      pastReservations = results;
      if (results.length < _pageSize) {
        _hasMorePast = false;
      }
    } catch (e) {
      pastReservations = [];
      _hasMorePast = false;
    } finally {
      isLoadingPast = false;
      notifyListeners();
    }
  }

  Future<void> loadMorePastReservations() async {
    if (isLoadingMorePast || !_hasMorePast) return;

    isLoadingMorePast = true;
    notifyListeners();

    try {
      _pastPageNumber++;
      final results = await _reservationService.getMyReservations(
        scope: 'past', 
        pageNumber: _pastPageNumber, 
        pageSize: _pageSize
      );
      
      if (results.isEmpty) {
        _hasMorePast = false;
      } else {
        pastReservations.addAll(results);
        if (results.length < _pageSize) {
          _hasMorePast = false;
        }
      }
    } catch (e) {
      _hasMorePast = false;
    } finally {
      isLoadingMorePast = false;
      notifyListeners();
    }
  }

  Future<bool> cancelReservation(int reservationId) async {
    final success = await _reservationService.cancelReservation(reservationId);
    if (success) {
      await loadUpcomingReservations();
      await loadPastReservations();
    }
    return success;
  }

  Future<bool> submitReview({
    required int reservationId,
    required int rating,
    required String comment,
  }) async {
    final success = await _reservationService.submitReview(reservationId, rating, comment);
    if (success) {
      await loadPastReservations();
    }
    return success;
  }
}
