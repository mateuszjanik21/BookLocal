import 'package:flutter/foundation.dart';
import '../../../../core/models/favorite_service_dto.dart';
import '../../../../core/services/favorites_service.dart';

class FavoritesProvider with ChangeNotifier {
  FavoritesService _favoritesService;

  List<FavoriteServiceDto> _favorites = [];
  bool _isLoading = false;
  
  int _currentPage = 1;
  final int _pageSize = 12;
  bool _hasMore = true;
  bool _isLoadingMore = false;

  FavoritesProvider(this._favoritesService);

  void updateService(FavoritesService service) {
    _favoritesService = service;
  }

  List<FavoriteServiceDto> get favorites => _favorites;
  bool get isLoading => _isLoading;
  bool get hasMore => _hasMore;
  bool get isLoadingMore => _isLoadingMore;

  Future<void> fetchFavorites({bool refresh = false}) async {
    if (refresh) {
      _currentPage = 1;
      _hasMore = true;
      _favorites.clear();
      _isLoading = true;
    } else {
      if (!_hasMore || _isLoading || _isLoadingMore) return;
      _isLoadingMore = true;
    }
    notifyListeners();

    try {
      final result = await _favoritesService.getFavorites(pageNumber: _currentPage, pageSize: _pageSize);
      if (result != null) {
        if (refresh) {
          _favorites = result.items;
        } else {
          _favorites.addAll(result.items);
        }
        _hasMore = _currentPage < result.totalPages;
        if (_hasMore) {
          _currentPage++;
        }
      } else {
        _hasMore = false;
      }
    } catch (e) {
      print("Error fetching favorites: $e");
      _hasMore = false;
    } finally {
      _isLoading = false;
      _isLoadingMore = false;
      notifyListeners();
    }
  }

  Future<bool> addFavorite(int serviceVariantId) async {
    final success = await _favoritesService.addFavorite(serviceVariantId);
    if (success) {
      await fetchFavorites(refresh: true);
    }
    return success;
  }

  Future<bool> removeFavorite(int serviceVariantId) async {
    final success = await _favoritesService.removeFavorite(serviceVariantId);
    if (success) {
      _favorites.removeWhere((f) => f.serviceVariantId == serviceVariantId);
      notifyListeners();
    }
    return success;
  }

  Future<bool> isFavorite(int serviceVariantId) async {
    return await _favoritesService.isFavorite(serviceVariantId);
  }
}
