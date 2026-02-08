import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../core/services/auth_service.dart';
import '../../../core/services/client_service.dart';
import '../../../core/models/business_list_item_dto.dart';
import 'business_details_screen.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  List<BusinessListItemDto> _allBusinesses = [];
  List<BusinessListItemDto> _filteredBusinesses = [];
  
  bool _isLoading = true;
  String _searchQuery = "";
  String? _selectedCategory;

  final List<Map<String, dynamic>> _categories = [
    {'name': 'Fryzjer', 'icon': Icons.content_cut},
    {'name': 'Barber', 'icon': Icons.face},
    {'name': 'Masaż', 'icon': Icons.spa},
    {'name': 'Fizjoterapia', 'icon': Icons.accessibility_new},
    {'name': 'Psycholog', 'icon': Icons.psychology},
    {'name': 'Pilates', 'icon': Icons.self_improvement},
    {'name': 'Joga', 'icon': Icons.fitness_center},
    {'name': 'Kosmetyczka', 'icon': Icons.brush},
    {'name': 'Groomer', 'icon': Icons.pets},
    {'name': 'Tatuaż', 'icon': Icons.draw},
    {'name': 'Medycyna Est.', 'icon': Icons.medical_services},
    {'name': 'Trening', 'icon': Icons.directions_run},
  ];

  @override
  void initState() {
    super.initState();
    _loadBusinesses();
  }

  Future<void> _loadBusinesses() async {
    try {
      final clientService = Provider.of<ClientService>(context, listen: false);
      final businesses = await clientService.getBusinesses();
      
      if (mounted) {
        setState(() {
          _allBusinesses = businesses;
          _filteredBusinesses = businesses;
          _isLoading = false;
        });
      }
    } catch (e) {
      print("Błąd pobierania firm: $e");
      if (mounted) {
        setState(() {
          _isLoading = false;
        });
      }
    }
  }

  void _filterBusinesses() {
    setState(() {
      _filteredBusinesses = _allBusinesses.where((business) {
        final matchesSearch = business.name.toLowerCase().contains(_searchQuery.toLowerCase()) ||
                              business.city.toLowerCase().contains(_searchQuery.toLowerCase());
        
        final matchesCategory = _selectedCategory == null || 
                                business.category.toLowerCase().contains(_selectedCategory!.toLowerCase()) || 
                                (business.category == "Medycyna Estetyczna" && _selectedCategory == "Medycyna Est.");

        return matchesSearch && matchesCategory;
      }).toList();
    });
  }

  void _onSearchChanged(String value) {
    _searchQuery = value;
    _filterBusinesses();
  }

  void _onCategoryTap(String category) {
    setState(() {
      if (_selectedCategory == category) {
        _selectedCategory = null;
      } else {
        _selectedCategory = category;
      }
      _filterBusinesses();
    });
  }

  @override
  Widget build(BuildContext context) {
    final user = Provider.of<AuthService>(context).currentUser;
    final primaryColor = const Color(0xFF16a34a);

    return Scaffold(
      backgroundColor: const Color(0xFFF8F9FA),
      body: GestureDetector(
        onTap: () => FocusScope.of(context).unfocus(),
        child: SafeArea(
          child: Column(
            children: [
              Padding(
                padding: const EdgeInsets.fromLTRB(24, 24, 24, 10),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              "Cześć, ${user?.firstName ?? 'Gościu'}! 👋",
                              style: const TextStyle(
                                fontSize: 26,
                                fontWeight: FontWeight.w800,
                                color: Color(0xFF1F2937),
                              ),
                            ),
                            const SizedBox(height: 6),
                            Text(
                              "Znajdź najlepsze usługi w okolicy",
                              style: TextStyle(color: Colors.grey[500], fontSize: 15),
                            ),
                          ],
                        ),
                        Container(
                          padding: const EdgeInsets.all(2),
                          decoration: BoxDecoration(
                            shape: BoxShape.circle,
                            border: Border.all(color: primaryColor.withOpacity(0.2), width: 2),
                          ),
                          child: CircleAvatar(
                            radius: 24,
                            backgroundColor: primaryColor.withOpacity(0.1),
                            backgroundImage: user?.photoUrl != null ? NetworkImage(user!.photoUrl!) : null,
                            child: user?.photoUrl == null
                                ? Text(user?.firstName[0] ?? "G", style: TextStyle(color: primaryColor, fontWeight: FontWeight.bold))
                                : null,
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 24),

                    Container(
                      decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius: BorderRadius.circular(16),
                        boxShadow: [
                          BoxShadow(
                            color: Colors.black.withOpacity(0.04),
                            blurRadius: 15,
                            offset: const Offset(0, 5),
                          ),
                        ],
                      ),
                      child: TextField(
                        onChanged: _onSearchChanged,
                        decoration: InputDecoration(
                          hintText: "Szukaj (np. Barber, Wrocław)",
                          hintStyle: TextStyle(color: Colors.grey[400], fontSize: 15),
                          prefixIcon: Icon(Icons.search_rounded, color: Colors.grey[400], size: 26),
                          border: InputBorder.none,
                          contentPadding: const EdgeInsets.symmetric(horizontal: 20, vertical: 18),
                        ),
                      ),
                    ),
                  ],
                ),
              ),

              SizedBox(
                height: 110,
                child: ListView.separated(
                  padding: const EdgeInsets.symmetric(horizontal: 24),
                  scrollDirection: Axis.horizontal,
                  itemCount: _categories.length,
                  separatorBuilder: (context, index) => const SizedBox(width: 16),
                  itemBuilder: (context, index) {
                    final cat = _categories[index];
                    return _buildCategoryItem(
                      cat['name'],
                      cat['icon'],
                      isSelected: _selectedCategory == cat['name'],
                    );
                  },
                ),
              ),

              Expanded(
                child: _isLoading
                    ? Center(child: CircularProgressIndicator(color: primaryColor))
                    : _filteredBusinesses.isEmpty
                        ? Center(
                            child: Column(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                Icon(Icons.search_off_rounded, size: 80, color: Colors.grey[300]),
                                const SizedBox(height: 15),
                                Text(
                                  "Brak wyników",
                                  style: TextStyle(color: Colors.grey[500], fontSize: 16, fontWeight: FontWeight.w500),
                                ),
                              ],
                            ),
                          )
                        : ListView.separated(
                            padding: const EdgeInsets.fromLTRB(24, 10, 24, 30),
                            itemCount: _filteredBusinesses.length,
                            separatorBuilder: (context, index) => const SizedBox(height: 20),
                            itemBuilder: (context, index) {
                              final business = _filteredBusinesses[index];
                              return _buildBusinessCard(business);
                            },
                          ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildCategoryItem(String name, IconData icon, {required bool isSelected}) {
    final primaryColor = const Color(0xFF16a34a);

    return GestureDetector(
      onTap: () => _onCategoryTap(name),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          AnimatedContainer(
            duration: const Duration(milliseconds: 200),
            width: 65,
            height: 65,
            decoration: BoxDecoration(
              color: isSelected ? primaryColor : Colors.white,
              borderRadius: BorderRadius.circular(20),
              border: isSelected ? null : Border.all(color: Colors.grey.shade200),
              boxShadow: isSelected
                  ? [BoxShadow(color: primaryColor.withOpacity(0.4), blurRadius: 12, offset: const Offset(0, 6))]
                  : [BoxShadow(color: Colors.black.withOpacity(0.03), blurRadius: 8, offset: const Offset(0, 3))],
            ),
            child: Icon(
              icon,
              color: isSelected ? Colors.white : Colors.grey[600],
              size: 28,
            ),
          ),
          const SizedBox(height: 10),
          SizedBox(
            width: 70,
            child: Text(
              name,
              textAlign: TextAlign.center,
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
              style: TextStyle(
                fontSize: 12,
                fontWeight: isSelected ? FontWeight.bold : FontWeight.w500,
                color: isSelected ? primaryColor : Colors.grey[600],
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildBusinessCard(BusinessListItemDto business) {
    return GestureDetector(
      onTap: () {
        Navigator.push(
          context,
          MaterialPageRoute(
            builder: (context) => BusinessDetailsScreen(business: business),
          ),
        );
      },
      child: Container(
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(20),
          boxShadow: [
            BoxShadow(
              color: const Color(0xFF1F2937).withOpacity(0.08),
              blurRadius: 20,
              offset: const Offset(0, 8),
              spreadRadius: -4,
            ),
          ],
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Stack(
              children: [
                Container(
                  height: 180,
                  width: double.infinity,
                  decoration: BoxDecoration(
                    color: Colors.grey[100],
                    borderRadius: const BorderRadius.vertical(top: Radius.circular(20)),
                    image: business.photoUrl != null
                        ? DecorationImage(image: NetworkImage(business.photoUrl!), fit: BoxFit.cover)
                        : null,
                  ),
                  child: business.photoUrl == null
                      ? Center(child: Icon(Icons.store_mall_directory_outlined, size: 50, color: Colors.grey[300]))
                      : null,
                ),
                Positioned(
                  top: 15,
                  right: 15,
                  child: Container(
                    padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
                    decoration: BoxDecoration(
                      color: Colors.white.withOpacity(0.95),
                      borderRadius: BorderRadius.circular(12),
                      boxShadow: [
                        BoxShadow(color: Colors.black.withOpacity(0.1), blurRadius: 10),
                      ],
                    ),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        const Icon(Icons.star_rounded, size: 16, color: Color(0xFFEAB308)),
                        const SizedBox(width: 4),
                        Text(
                          business.rating.toStringAsFixed(1),
                          style: const TextStyle(fontSize: 13, fontWeight: FontWeight.bold, color: Colors.black87),
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
            
            Padding(
              padding: const EdgeInsets.all(20),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Expanded(
                        child: Text(
                          business.name,
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                          style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Color(0xFF111827)),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 8),
                  
                  Row(
                    children: [
                      _buildTag(business.category, Icons.category_outlined),
                      const SizedBox(width: 10),
                      _buildTag(business.city, Icons.location_on_outlined),
                    ],
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildTag(String text, IconData icon) {
    return Row(
      children: [
        Icon(icon, size: 14, color: Colors.grey[500]),
        const SizedBox(width: 4),
        Text(
          text,
          style: TextStyle(color: Colors.grey[600], fontSize: 13, fontWeight: FontWeight.w500),
        ),
      ],
    );
  }
}