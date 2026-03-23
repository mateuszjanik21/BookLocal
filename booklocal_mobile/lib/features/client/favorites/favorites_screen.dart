import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'providers/favorites_provider.dart';
import 'widgets/favorite_service_card.dart';

class FavoritesScreen extends StatefulWidget {
  const FavoritesScreen({super.key});

  @override
  State<FavoritesScreen> createState() => _FavoritesScreenState();
}

class _FavoritesScreenState extends State<FavoritesScreen> {
  final ScrollController _scrollController = ScrollController();
  final TextEditingController _searchController = TextEditingController();
  String _searchQuery = '';

  @override
  void initState() {
    super.initState();
    _scrollController.addListener(_onScroll);
    WidgetsBinding.instance.addPostFrameCallback((_) {
      Provider.of<FavoritesProvider>(context, listen: false).fetchFavorites(refresh: true);
    });
  }

  void _onScroll() {
    if (_scrollController.position.pixels >= _scrollController.position.maxScrollExtent - 200) {
      Provider.of<FavoritesProvider>(context, listen: false).fetchFavorites();
    }
  }

  @override
  void dispose() {
    _scrollController.dispose();
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF3F4F6),
      appBar: AppBar(
        title: const Text('Ulubione Usługi', style: TextStyle(fontWeight: FontWeight.bold)),
        backgroundColor: Colors.white,
        surfaceTintColor: Colors.transparent,
        elevation: 0,
        centerTitle: true,
      ),
      body: Consumer<FavoritesProvider>(
        builder: (context, provider, child) {
          if (provider.isLoading && provider.favorites.isEmpty) {
            return const Center(
              child: CircularProgressIndicator(color: Color(0xFF16a34a)),
            );
          }

          if (provider.favorites.isEmpty) {
            return Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(Icons.favorite_border, size: 80, color: Colors.grey[350]),
                  const SizedBox(height: 16),
                  const Text(
                    'Brak ulubionych usług',
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Color(0xFF1F2937)),
                  ),
                  const SizedBox(height: 8),
                  Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 32),
                    child: Text(
                      'Dodaj usługi do ulubionych przeglądając oferty firm, aby mieć do nich szybki dostęp.',
                      textAlign: TextAlign.center,
                      style: TextStyle(color: Colors.grey[600], height: 1.5),
                    ),
                  ),
                ],
              ),
            );
          }

          final filteredFavorites = provider.favorites.where((f) {
            final query = _searchQuery.toLowerCase();
            return f.serviceName.toLowerCase().contains(query) || 
                   f.businessName.toLowerCase().contains(query);
          }).toList();

          return RefreshIndicator(
            onRefresh: () => provider.fetchFavorites(refresh: true),
            color: const Color(0xFF16a34a),
            child: Column(
              children: [
                if (provider.favorites.isNotEmpty || _searchQuery.isNotEmpty)
                  Padding(
                    padding: const EdgeInsets.all(16.0),
                    child: TextField(
                      controller: _searchController,
                      decoration: InputDecoration(
                        hintText: 'Szukaj usługi lub salonu...',
                        prefixIcon: const Icon(Icons.search),
                        filled: true,
                        fillColor: Colors.white,
                        border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(12),
                          borderSide: BorderSide.none,
                        ),
                        contentPadding: const EdgeInsets.symmetric(vertical: 0),
                        suffixIcon: _searchQuery.isNotEmpty
                            ? IconButton(
                                icon: const Icon(Icons.clear, size: 20),
                                onPressed: () {
                                  _searchController.clear();
                                  setState(() => _searchQuery = '');
                                },
                              )
                            : null,
                      ),
                      onChanged: (val) {
                        setState(() {
                          _searchQuery = val;
                        });
                      },
                    ),
                  ),
                Expanded(
                  child: filteredFavorites.isEmpty && _searchQuery.isNotEmpty
                      ? const Center(child: Text("Brak wyników wyszukiwania"))
                      : ListView.builder(
                          controller: _scrollController,
                          padding: const EdgeInsets.only(bottom: 24),
                          itemCount: filteredFavorites.length + (provider.isLoadingMore ? 1 : 0),
                          physics: const AlwaysScrollableScrollPhysics(),
                          itemBuilder: (context, index) {
                            if (index == filteredFavorites.length) {
                              return const Padding(
                                padding: EdgeInsets.all(16.0),
                                child: Center(child: CircularProgressIndicator(color: Color(0xFF16a34a))),
                              );
                            }
                            final favorite = filteredFavorites[index];
                            return FavoriteServiceCard(favorite: favorite);
                          },
                        ),
                ),
              ],
            ),
          );
        },
      ),
    );
  }
}
