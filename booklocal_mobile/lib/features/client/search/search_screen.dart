import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../core/models/business_list_item_dto.dart';
import '../../../core/services/client_service.dart';
import '../home/business_details_screen.dart'; // Żeby móc wejść w firmę z wyników

class SearchScreen extends StatefulWidget {
  const SearchScreen({super.key});

  @override
  State<SearchScreen> createState() => _SearchScreenState();
}

class _SearchScreenState extends State<SearchScreen> {
  final TextEditingController _searchController = TextEditingController();
  
  // Wszystkie firmy pobrane z API
  List<BusinessListItemDto> _allBusinesses = [];
  // Firmy aktualnie wyświetlane (przefiltrowane)
  List<BusinessListItemDto> _filteredBusinesses = [];
  
  bool _isLoading = true;
  String _selectedCategory = 'Wszystkie';

  // Lista dostępnych kategorii
  final List<String> _categories = [
    'Wszystkie',
    'Fryzjer',
    'Barber',
    'Kosmetyczka',
    'Masaż',
    'Fizjoterapia'
  ];

  @override
  void initState() {
    super.initState();
    _loadData();
  }

  Future<void> _loadData() async {
    // Pobieramy dane z serwisu
    final service = Provider.of<ClientService>(context, listen: false);
    final data = await service.getBusinesses();

    if (mounted) {
      setState(() {
        _allBusinesses = data;
        _filteredBusinesses = data; // Na początku pokazujemy wszystko
        _isLoading = false;
      });
    }
  }

  // Główna logika filtrowania
  void _filterResults() {
    final query = _searchController.text.toLowerCase();
    
    setState(() {
      _filteredBusinesses = _allBusinesses.where((business) {
        // 1. Sprawdź czy pasuje do kategorii (jeśli wybrana inna niż Wszystkie)
        final matchesCategory = _selectedCategory == 'Wszystkie' || 
                                business.category == _selectedCategory;
        
        // 2. Sprawdź czy pasuje do wpisanego tekstu (Nazwa LUB Miasto)
        final matchesQuery = business.name.toLowerCase().contains(query) || 
                             business.city.toLowerCase().contains(query);

        return matchesCategory && matchesQuery;
      }).toList();
    });
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final primaryColor = const Color(0xFF16a34a);

    return Scaffold(
      backgroundColor: Colors.white,
      body: SafeArea(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // 1. Pasek wyszukiwania
            Padding(
              padding: const EdgeInsets.all(16.0),
              child: TextField(
                controller: _searchController,
                onChanged: (value) => _filterResults(), // Filtruj przy każdej literce
                decoration: InputDecoration(
                  hintText: 'Szukaj (np. Barber, Wrocław)...',
                  prefixIcon: const Icon(Icons.search),
                  suffixIcon: _searchController.text.isNotEmpty
                      ? IconButton(
                          icon: const Icon(Icons.clear),
                          onPressed: () {
                            _searchController.clear();
                            _filterResults();
                          },
                        )
                      : null,
                  filled: true,
                  fillColor: Colors.grey[100],
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                    borderSide: BorderSide.none,
                  ),
                ),
              ),
            ),

            // 2. Kategorie (Filtry poziome)
            SizedBox(
              height: 50,
              child: ListView.builder(
                scrollDirection: Axis.horizontal,
                padding: const EdgeInsets.symmetric(horizontal: 16),
                itemCount: _categories.length,
                itemBuilder: (context, index) {
                  final cat = _categories[index];
                  final isSelected = cat == _selectedCategory;
                  
                  return Padding(
                    padding: const EdgeInsets.only(right: 8),
                    child: FilterChip(
                      label: Text(cat),
                      selected: isSelected,
                      onSelected: (bool selected) {
                        setState(() {
                          _selectedCategory = cat; // Zawsze wybieramy klikniętą (proste zachowanie)
                          _filterResults();
                        });
                      },
                      backgroundColor: Colors.white,
                      selectedColor: primaryColor.withOpacity(0.2),
                      labelStyle: TextStyle(
                        color: isSelected ? primaryColor : Colors.black87,
                        fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
                      ),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(20),
                        side: BorderSide(
                          color: isSelected ? primaryColor : Colors.grey.shade300,
                        ),
                      ),
                      showCheckmark: false, // Ukrywamy "ptaszka", zmieniamy tylko kolor
                    ),
                  );
                },
              ),
            ),
            
            const Divider(height: 30),

            // 3. Lista Wyników
            Expanded(
              child: _isLoading
                  ? const Center(child: CircularProgressIndicator())
                  : _filteredBusinesses.isEmpty
                      ? Center(
                          child: Column(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: [
                              Icon(Icons.search_off, size: 60, color: Colors.grey[300]),
                              const SizedBox(height: 10),
                              Text("Nie znaleziono firm.", style: TextStyle(color: Colors.grey[500])),
                            ],
                          ),
                        )
                      : ListView.builder(
                          padding: const EdgeInsets.symmetric(horizontal: 16),
                          itemCount: _filteredBusinesses.length,
                          itemBuilder: (context, index) {
                            return _buildSearchResultItem(_filteredBusinesses[index]);
                          },
                        ),
            ),
          ],
        ),
      ),
    );
  }

  // Karta pojedynczego wyniku (mniejsza niż na Home)
  Widget _buildSearchResultItem(BusinessListItemDto business) {
    return GestureDetector(
      onTap: () {
        // Przejście do szczegółów (tak samo jak z Home)
        Navigator.push(
          context,
          MaterialPageRoute(
            builder: (context) => BusinessDetailsScreen(business: business),
          ),
        );
      },
      child: Container(
        margin: const EdgeInsets.only(bottom: 16),
        padding: const EdgeInsets.all(12),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(color: Colors.grey.shade200),
          boxShadow: [
            BoxShadow(
              color: Colors.grey.withOpacity(0.05),
              blurRadius: 5,
              offset: const Offset(0, 2),
            ),
          ],
        ),
        child: Row(
          children: [
            // Zdjęcie (Miniatura)
            Container(
              width: 80,
              height: 80,
              decoration: BoxDecoration(
                borderRadius: BorderRadius.circular(8),
                color: Colors.grey[200],
                image: business.photoUrl != null
                    ? DecorationImage(image: NetworkImage(business.photoUrl!), fit: BoxFit.cover)
                    : null,
              ),
              child: business.photoUrl == null 
                  ? const Icon(Icons.store, color: Colors.grey) 
                  : null,
            ),
            const SizedBox(width: 16),
            // Opis
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    business.name,
                    style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    "${business.category} • ${business.city}",
                    style: TextStyle(color: Colors.grey[600], fontSize: 13),
                  ),
                  const SizedBox(height: 8),
                  Row(
                    children: [
                      const Icon(Icons.star, size: 14, color: Colors.amber),
                      const SizedBox(width: 4),
                      Text(
                        business.rating.toString(),
                        style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 12),
                      ),
                    ],
                  )
                ],
              ),
            ),
            const Icon(Icons.chevron_right, color: Colors.grey),
          ],
        ),
      ),
    );
  }
}