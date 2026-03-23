import 'dart:async';
import 'package:flutter/material.dart';
import '../../../../core/models/main_category.dart';
import '../../../../core/models/paged_result.dart';
import '../../../../core/models/rebook_suggestion.dart';
import '../../../../core/models/service_category_search_result.dart';
import '../../../../core/services/search_service.dart';
import '../../../../core/services/auth_service.dart';
import 'package:geolocator/geolocator.dart';
import 'package:permission_handler/permission_handler.dart';
import 'dart:convert';
import 'package:http/http.dart' as http;

class HomeProvider extends ChangeNotifier {
  final SearchService _searchService = SearchService();

  // Konfiguracja paginacji
  int pageNumber = 1;
  final int pageSize = 12;

  // Kontrolery wprowadzania tekstu
  final TextEditingController searchController = TextEditingController();
  final TextEditingController locationController = TextEditingController();

  // Filtry i parametry wyszukiwania
  int? activeMainCategoryId;
  String activeSortBy = '';

  // Stan
  PagedResult<ServiceCategorySearchResult>? pagedResult;
  List<MainCategory> mainCategories = [];
  List<RebookSuggestion> rebookSuggestions = [];

  bool isLoading = false;
  bool isSkeletonVisible = false;
  bool isLocationLoading = false;

  Timer? _debounceTimer;
  Timer? _skeletonTimer;

  HomeProvider() {
    searchController.addListener(_onSearchChanged);
    locationController.addListener(_onSearchChanged);
  }

  @override
  void dispose() {
    _debounceTimer?.cancel();
    _skeletonTimer?.cancel();
    searchController.dispose();
    locationController.dispose();
    super.dispose();
  }

  Future<void> init(AuthService authService) async {
    await Future.wait([
      _fetchMainCategories(),
      _fetchRebookSuggestions(authService),
    ]);
    fetchResults();
  }

  void _onSearchChanged() {
    if (_debounceTimer?.isActive ?? false) _debounceTimer!.cancel();
    _debounceTimer = Timer(const Duration(milliseconds: 300), () {
      pageNumber = 1;
      fetchResults();
    });
  }

  void onFilterChanged() {
    pageNumber = 1;
    fetchResults();
  }

  Future<void> fetchResults() async {
    isLoading = true;
    isSkeletonVisible = false;
    notifyListeners();

    _skeletonTimer?.cancel();
    _skeletonTimer = Timer(const Duration(milliseconds: 250), () {
      if (isLoading) {
        isSkeletonVisible = true;
        notifyListeners();
      }
    });

    try {
      final result = await _searchService.searchCategoryFeed(
        searchTerm: searchController.text,
        locationTerm: locationController.text,
        mainCategoryId: activeMainCategoryId,
        sortBy: activeSortBy,
        pageNumber: pageNumber,
        pageSize: pageSize,
      );
      
      pagedResult = result;
    } catch (e) {
      print('Error w HomeProvider fetchResults: $e');
      pagedResult = PagedResult(
        items: [],
        totalCount: 0,
        pageNumber: pageNumber,
        pageSize: pageSize,
        totalPages: 0,
      );
    } finally {
      _skeletonTimer?.cancel();
      isLoading = false;
      isSkeletonVisible = false;
      notifyListeners();
    }
  }

  Future<void> _fetchMainCategories() async {
    mainCategories = await _searchService.getMainCategories();
    notifyListeners();
  }

  Future<void> _fetchRebookSuggestions(AuthService authService) async {
    if (authService.isAuthenticated) {
      final token = authService.token;
      rebookSuggestions = await _searchService.getRebookSuggestions(token);
      notifyListeners();
    }
  }

  void setMainCategory(int? categoryId) {
    activeMainCategoryId = categoryId;
    onFilterChanged();
  }

  void setSortBy(String sortBy) {
    activeSortBy = sortBy;
    onFilterChanged();
  }

  void pageChanged(int newPage) {
    if (pageNumber == newPage) return;
    pageNumber = newPage;
    fetchResults();
  }

  void clearSearch() {
    searchController.clear();
    locationController.clear();
    activeMainCategoryId = null;
    activeSortBy = '';
    onFilterChanged();
  }

  Future<void> useCurrentLocation() async {
    if (isLocationLoading) return;

    final status = await Permission.locationWhenInUse.request();
    if (status.isGranted) {
      try {
        isLocationLoading = true;
        notifyListeners();

        final position = await Geolocator.getCurrentPosition(
          desiredAccuracy: LocationAccuracy.high,
          timeLimit: const Duration(seconds: 10),
        );

        final response = await http.get(Uri.parse(
            'https://api.bigdatacloud.net/data/reverse-geocode-client?latitude=${position.latitude}&longitude=${position.longitude}&localityLanguage=pl'));

        if (response.statusCode == 200) {
          final data = jsonDecode(response.body);
          final city = data['city'] ?? data['locality'] ?? 'Moja lokalizacja';
          locationController.text = city;
        } else {
          locationController.text = 'Moja lokalizacja';
        }
      } catch (e) {
        print('Błąd geolokalizacji: $e');
        locationController.text = 'Moja lokalizacja';
      } finally {
        isLocationLoading = false;
        notifyListeners();
        onFilterChanged();
      }
    } else {
      print('Brak uprawnień do lokalizacji');
    }
  }

  // Get pagination pages
  List<dynamic> getPaginationPages() {
    if (pagedResult == null) return [];

    final totalPages = pagedResult!.totalPages;
    final currentPage = pagedResult!.pageNumber;

    if (totalPages <= 7) {
      return List.generate(totalPages, (i) => i + 1);
    }

    final List<dynamic> pages = [];
    pages.add(1);

    if (currentPage > 4) {
      pages.add('...');
    }

    for (int i = currentPage - 2; i <= currentPage + 2; i++) {
      if (i > 1 && i < totalPages) {
        pages.add(i);
      }
    }

    if (currentPage < totalPages - 3) {
      pages.add('...');
    }

    pages.add(totalPages);

    return pages;
  }
}
