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

  int pageNumber = 1;
  final int pageSize = 12;

  final TextEditingController searchController = TextEditingController();
  final TextEditingController locationController = TextEditingController();

  int? activeMainCategoryId;
  String activeSortBy = '';

  PagedResult<ServiceCategorySearchResult>? pagedResult;
  List<MainCategory> mainCategories = [];
  List<RebookSuggestion> rebookSuggestions = [];

  bool isLoading = true;
  bool isSkeletonVisible = true;
  bool isLocationLoading = false;
  bool isRebookLoading = true;
  bool isCategoryLoading = true;

  final ScrollController mainScrollController = ScrollController();

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
    mainScrollController.dispose();
    super.dispose();
  }

  Future<void> init(AuthService authService) async {
    _fetchMainCategories();
    _fetchRebookSuggestions(authService);
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
    isSkeletonVisible = true;
    notifyListeners();

    try {
      final results = await Future.wait([
        _searchService.searchCategoryFeed(
          searchTerm: searchController.text,
          locationTerm: locationController.text,
          mainCategoryId: activeMainCategoryId,
          sortBy: activeSortBy,
          pageNumber: pageNumber,
          pageSize: pageSize,
        ),
        Future.delayed(const Duration(milliseconds: 400)),
      ]);
      
      pagedResult = results[0] as PagedResult<ServiceCategorySearchResult>;
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
      isLoading = false;
      isSkeletonVisible = false;
      notifyListeners();
    }
  }

  Future<void> _fetchMainCategories() async {
    isCategoryLoading = true;
    notifyListeners();
    try {
      final results = await Future.wait([
        _searchService.getMainCategories(),
        Future.delayed(const Duration(milliseconds: 400)),
      ]);
      mainCategories = results[0] as List<MainCategory>;
    } catch (e) {
      print('Błąd ładowania kategorii na Home: $e');
    } finally {
      isCategoryLoading = false;
      notifyListeners();
    }
  }

  Future<void> _fetchRebookSuggestions(AuthService authService) async {
    isRebookLoading = true;
    notifyListeners();
    try {
      if (authService.isAuthenticated) {
        final token = authService.token;
        final results = await Future.wait([
          _searchService.getRebookSuggestions(token),
          Future.delayed(const Duration(milliseconds: 400)),
        ]);
        rebookSuggestions = results[0] as List<RebookSuggestion>;
      } else {
        await Future.delayed(const Duration(milliseconds: 400));
      }
    } catch (e) {
      print('Błąd rebook: $e');
    } finally {
      isRebookLoading = false;
      notifyListeners();
    }
  }

  void setMainCategory(int? categoryId) {
    activeMainCategoryId = categoryId;
    onFilterChanged();
  }

  void setSortBy(String sortBy) {
    if (activeSortBy == sortBy) return;
    activeSortBy = sortBy;
    onFilterChanged();
  }

  void pageChanged(int newPage) {
    if (pageNumber == newPage) return;
    pageNumber = newPage;
    fetchResults();
    if (mainScrollController.hasClients) {
      mainScrollController.animateTo(
        0,
        duration: const Duration(milliseconds: 400),
        curve: Curves.easeInOut,
      );
    }
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
