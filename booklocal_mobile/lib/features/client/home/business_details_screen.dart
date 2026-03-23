import 'dart:ui'; // Potrzebne do ImageFilter
import 'package:booklocal_mobile/core/services/chat_services.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../core/models/business_detail_dto.dart';
import '../../../core/models/service.dart';
import '../../../core/models/service_variant.dart';
import '../../../core/models/business_list_item_dto.dart';
import '../../../core/models/employee_models.dart';
import '../../../core/models/service_models.dart';
import '../../../core/models/service_bundle_dto.dart';
import '../../../core/services/client_service.dart';
import '../../../core/services/service_bundle_service.dart';
import '../booking/booking_screen.dart';
import '../chat/conversation_screen.dart';
import '../../../core/models/review_models.dart';
import '../../../core/services/review_service.dart';
import '../favorites/providers/favorites_provider.dart';

class BusinessDetailsScreen extends StatefulWidget {
  final BusinessListItemDto business;
  
  const BusinessDetailsScreen({super.key, required this.business});

  @override
  State<BusinessDetailsScreen> createState() => _BusinessDetailsScreenState();
}

class _BusinessDetailsScreenState extends State<BusinessDetailsScreen> {
  BusinessDetailDto? _fullBusiness;
  bool _isLoadingBusiness = true;
  List<ReviewDto> _reviews = [];
  bool _isLoadingReviews = true;
  bool _isLoadingMoreReviews = false;
  int _reviewPage = 1;
  bool _hasMoreReviews = true;
  List<ServiceBundleDto> _bundles = [];
  bool _isLoadingBundles = true;

  @override
  void initState() {
    super.initState();
    _loadBusinessDetails();
    _loadReviews();
    _loadBundles();
  }

  Future<void> _loadReviews({bool loadMore = false}) async {
    if (loadMore) {
      setState(() => _isLoadingMoreReviews = true);
    }
    final reviewService = Provider.of<ReviewService>(context, listen: false);
    final page = loadMore ? _reviewPage + 1 : 1;
    final result = await reviewService.getReviews(widget.business.id, pageNumber: page, pageSize: 5);
    
    if (mounted) {
      setState(() {
        if (loadMore) {
          _reviews.addAll(result.items);
          _isLoadingMoreReviews = false;
        } else {
          _reviews = result.items;
          _isLoadingReviews = false;
        }
        _reviewPage = page;
        _hasMoreReviews = result.items.length >= 5;
      });
    }
  }

  Future<void> _loadBundles() async {
    final bundleService = Provider.of<ServiceBundleService>(context, listen: false);
    final bundles = await bundleService.getBundles(widget.business.id);
    if (mounted) {
      setState(() {
        _bundles = bundles;
        _isLoadingBundles = false;
      });
    }
  }

  Future<void> _loadBusinessDetails() async {
    final clientService = Provider.of<ClientService>(context, listen: false);
    final details = await clientService.getBusinessById(widget.business.id);

    if (mounted) {
      setState(() {
        _fullBusiness = details;
        _isLoadingBusiness = false;
      });
    }
  }

  void _startChat() async {
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text("Otwieranie czatu..."), duration: Duration(seconds: 1)),
    );

    try {
      final chatService = Provider.of<ChatService>(context, listen: false);
      final conversationId = await chatService.startConversation(widget.business.id);

      if (conversationId != null) {
        if (!mounted) return;
        Navigator.push(
          context,
          MaterialPageRoute(
            builder: (context) => ConversationScreen(
              conversationId: conversationId,
              participantName: widget.business.name,
            ),
          ),
        );
      } else {
        if (!mounted) return;
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text("Nie udało się rozpocząć rozmowy.")),
        );
      }
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text("Błąd: $e")));
    }
  }

  @override
  Widget build(BuildContext context) {
    // Definiujemy kolory
    const primaryColor = Color(0xFF16a34a);
    const backgroundColor = Color(0xFFF3F4F6); // Jasnoszary, jak w Angularze (bg-base-200)

    return DefaultTabController(
      length: 5,
      child: Scaffold(
        backgroundColor: backgroundColor,
        body: NestedScrollView(
          headerSliverBuilder: (BuildContext context, bool innerBoxIsScrolled) {
            return <Widget>[
              // 1. PROFESJONALNY HEADER (Banner z rozmyciem)
          SliverAppBar(
            expandedHeight: 320.0,
            floating: false,
            pinned: true,
            backgroundColor: primaryColor,
            stretch: true,
            flexibleSpace: FlexibleSpaceBar(
              background: Stack(
                fit: StackFit.expand,
                children: [
                  // Tło (Zdjęcie)
                  widget.business.photoUrl != null
                      ? Image.network(widget.business.photoUrl!, fit: BoxFit.cover)
                      : Container(color: Colors.grey[800]),
                  
                  // Efekt rozmycia i przyciemnienia (Glassmorphism)
                  BackdropFilter(
                    filter: ImageFilter.blur(sigmaX: 10.0, sigmaY: 10.0),
                    child: Container(
                      color: Colors.black.withOpacity(0.4),
                    ),
                  ),

                  // Zawartość Headera (Avatar, Nazwa, Ocena)
                  Center(
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        const SizedBox(height: 40), // Miejsce na status bar
                        // Avatar z pierścieniem
                        Container(
                          padding: const EdgeInsets.all(4),
                          decoration: BoxDecoration(
                            shape: BoxShape.circle,
                            border: Border.all(color: Colors.white.withOpacity(0.5), width: 2),
                          ),
                          child: CircleAvatar(
                            radius: 50,
                            backgroundColor: Colors.white,
                            backgroundImage: widget.business.photoUrl != null 
                              ? NetworkImage(widget.business.photoUrl!) 
                              : null,
                            child: widget.business.photoUrl == null
                              ? const Icon(Icons.store, size: 50, color: Colors.grey)
                              : null,
                          ),
                        ),
                        const SizedBox(height: 15),
                        // Nazwa
                        Padding(
                          padding: const EdgeInsets.symmetric(horizontal: 20),
                          child: Text(
                            widget.business.name,
                            textAlign: TextAlign.center,
                            style: const TextStyle(
                              color: Colors.white,
                              fontSize: 28,
                              fontWeight: FontWeight.bold,
                              shadows: [Shadow(color: Colors.black54, blurRadius: 10)],
                            ),
                          ),
                        ),
                        const SizedBox(height: 5),
                        // Miasto
                        Text(
                          widget.business.city,
                          style: TextStyle(color: Colors.white.withOpacity(0.9), fontSize: 16),
                        ),
                        const SizedBox(height: 15),
                        // Gwiazdki (Badge)
                        Container(
                          padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                          decoration: BoxDecoration(
                            color: Colors.white.withOpacity(0.2),
                            borderRadius: BorderRadius.circular(20),
                            border: Border.all(color: Colors.white.withOpacity(0.3)),
                          ),
                          child: Row(
                            mainAxisSize: MainAxisSize.min,
                            children: [
                              const Icon(Icons.star, color: Colors.amber, size: 20),
                              const SizedBox(width: 5),
                              Text(
                                _fullBusiness != null 
                                  ? "${_fullBusiness!.averageRating.toStringAsFixed(2)} (${_fullBusiness!.reviewCount} opinii)"
                                  : "${widget.business.rating} (${widget.business.reviewCount} opinii)",
                                style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
                              ),
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ),
          
          // 2. PRZYPIĘTY PASEK ZAKŁADEK (Sticky TabBar)
          SliverPersistentHeader(
            pinned: true,
            delegate: _SliverAppBarDelegate(
              TabBar(
                isScrollable: true, // Pozwala na wiele zakładek na ekranie
                labelColor: primaryColor,
                unselectedLabelColor: Colors.grey[600],
                indicatorColor: primaryColor,
                indicatorWeight: 3,
                tabAlignment: TabAlignment.start,
                labelStyle: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
                unselectedLabelStyle: const TextStyle(fontWeight: FontWeight.w500, fontSize: 14),
                // Wypełnione tło nagłówka aby nie było przezroczyste pod spodem:
                labelPadding: const EdgeInsets.symmetric(horizontal: 20, vertical: 4),
                tabs: const [
                  Tab(text: "O Nas"),
                  Tab(text: "Usługi"),
                  Tab(text: "Pakiety"),
                  Tab(text: "Zespół"),
                  Tab(text: "Opinie"),
                ],
              ),
            ),
          ),
        ];
      },
      body: TabBarView(
        children: [
          _buildAboutTab(),
          _buildServicesTab(),
          _buildBundlesTab(),
          _buildTeamTab(),
          _buildReviewsTab(),
        ],
      ),
    ),
    bottomNavigationBar: null,
  ),
  );
}

// --- ZAKŁADKI (TABS) ---

Widget _buildAboutTab() {
  if (_isLoadingBusiness) {
    return const Center(child: CircularProgressIndicator());
  }

  final description = _fullBusiness?.description ?? 
      (widget.business.city.isNotEmpty 
          ? "Zapraszamy do naszego salonu w mieście ${widget.business.city}. Oferujemy profesjonalne usługi najwyższej jakości."
          : "Witamy w naszym salonie!");

  final address = _fullBusiness?.address ?? widget.business.city;
  final phoneNumber = _fullBusiness?.phoneNumber;

  return LayoutBuilder(
    builder: (context, constraints) {
      return Padding(
        padding: const EdgeInsets.fromLTRB(16, 16, 16, 16),
        child: _buildSectionCard(
          title: "O Nas",
          icon: Icons.info_outline,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                description,
                style: TextStyle(color: Colors.grey[700], height: 1.5, fontSize: 14),
                textAlign: TextAlign.justify,
              ),
              const SizedBox(height: 20),
              
              if (address.isNotEmpty || (phoneNumber != null && phoneNumber.isNotEmpty))
                Container(
                  padding: const EdgeInsets.all(16),
                  decoration: BoxDecoration(
                    color: Colors.grey[50],
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(color: Colors.grey[200]!),
                  ),
                  child: Column(
                    children: [
                      if (address.isNotEmpty) ...[
                        Row(
                          children: [
                            const Icon(Icons.location_on, color: Color(0xFF16a34a), size: 20),
                            const SizedBox(width: 12),
                            Expanded(child: Text(address, style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w500))),
                          ],
                        ),
                      ],
                      if (phoneNumber != null && phoneNumber.isNotEmpty) ...[
                         if (address.isNotEmpty) const Divider(height: 24),
                         Row(
                          children: [
                            const Icon(Icons.phone, color: Color(0xFF16a34a), size: 20),
                            const SizedBox(width: 12),
                            Expanded(child: Text(phoneNumber, style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w500))),
                          ],
                        ),
                      ],
                    ],
                  ),
                ),
              
              const SizedBox(height: 20),
              Row(
                children: [
                  Expanded(
                    child: OutlinedButton.icon(
                      onPressed: _startChat,
                      icon: const Icon(Icons.chat_bubble_outline, size: 18),
                      label: const Text("Napisz"),
                      style: OutlinedButton.styleFrom(
                        foregroundColor: const Color(0xFF16a34a),
                        side: const BorderSide(color: Color(0xFF16a34a)),
                        padding: const EdgeInsets.symmetric(vertical: 12),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                      ),
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: ElevatedButton.icon(
                      onPressed: () {
                         ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text("Otwieranie mapy...")));
                      },
                      icon: const Icon(Icons.directions, size: 18),
                      label: const Text("Trasa"),
                      style: ElevatedButton.styleFrom(
                        backgroundColor: const Color(0xFF16a34a),
                        foregroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(vertical: 12),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                        elevation: 0,
                      ),
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),
      );
    },
  );
}

Widget _buildServicesTab() {
  return SingleChildScrollView(
    padding: const EdgeInsets.fromLTRB(16, 16, 16, 40),
    child: _buildSectionCard(
      title: "Cennik Usług",
      icon: Icons.list_alt,
      child: _isLoadingBusiness 
        ? const Center(child: CircularProgressIndicator())
        : _fullBusiness == null || _fullBusiness!.categories.isEmpty
          ? const Text("Brak usług.", style: TextStyle(color: Colors.grey))
          : ListView.builder(
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              itemCount: _fullBusiness!.categories.length,
              itemBuilder: (context, catIndex) {
                 final category = _fullBusiness!.categories[catIndex];
                 if (category.services.isEmpty) return const SizedBox.shrink();
                 return Column(
                   crossAxisAlignment: CrossAxisAlignment.start,
                   children: [
                      Padding(
                        padding: const EdgeInsets.only(top: 8.0, bottom: 12.0),
                        child: Text(category.name, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 18, color: Color(0xFF16a34a))),
                      ),
                      ...category.services.expand((service) => service.variants.map((variant) => _buildServiceRow(service, variant))),
                      const SizedBox(height: 16),
                   ]
                 );
              },
            ),
    ),
  );
}

Widget _buildBundlesTab() {
  return SingleChildScrollView(
    padding: const EdgeInsets.fromLTRB(16, 16, 16, 40),
    child: _buildSectionCard(
      title: "Pakiety",
      icon: Icons.card_giftcard,
      child: _isLoadingBundles
        ? const Center(child: CircularProgressIndicator())
        : _bundles.isEmpty
          ? const Center(child: Padding(
              padding: EdgeInsets.all(20.0),
              child: Text("Brak dostępnych pakietów.", style: TextStyle(color: Colors.grey)),
            ))
          : ListView.separated(
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              itemCount: _bundles.length,
              separatorBuilder: (_, _) => const SizedBox(height: 16),
              itemBuilder: (context, index) => _buildBundleCard(_bundles[index]),
            ),
    ),
  );
}

Widget _buildBundleCard(ServiceBundleDto bundle) {
  return Container(
    decoration: BoxDecoration(
      borderRadius: BorderRadius.circular(12),
      border: Border.all(color: Colors.grey.shade200),
      color: Colors.white,
    ),
    child: Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        // Nagłówek pakietu
        Container(
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(
            color: const Color(0xFF16a34a).withOpacity(0.05),
            borderRadius: const BorderRadius.vertical(top: Radius.circular(12)),
          ),
          child: Row(
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(bundle.name, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16, color: Color(0xFF1F2937))),
                    if (bundle.description != null && bundle.description!.isNotEmpty)
                      Padding(
                        padding: const EdgeInsets.only(top: 4.0),
                        child: Text(bundle.description!, style: TextStyle(color: Colors.grey[600], fontSize: 13)),
                      ),
                  ],
                ),
              ),
              if (bundle.discountPercent > 0)
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                  decoration: BoxDecoration(
                    color: Colors.redAccent,
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: Text("-${bundle.discountPercent}%", style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 12)),
                ),
            ],
          ),
        ),
        // Lista elementów pakietu
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
          child: Column(
            children: bundle.items.map((item) => Padding(
              padding: const EdgeInsets.symmetric(vertical: 6.0),
              child: Row(
                children: [
                  Icon(Icons.check_circle_outline, size: 16, color: const Color(0xFF16a34a).withOpacity(0.7)),
                  const SizedBox(width: 10),
                  Expanded(
                    child: Text(
                      item.serviceName + (item.variantName.isNotEmpty && item.variantName.toLowerCase() != "domyślny" ? " - ${item.variantName}" : ""),
                      style: const TextStyle(fontSize: 13, color: Color(0xFF374151)),
                    ),
                  ),
                  Text("${item.durationMinutes} min", style: TextStyle(color: Colors.grey[500], fontSize: 12)),
                  const SizedBox(width: 8),
                  if (bundle.discountPercent > 0)
                    Text("${item.originalPrice.toInt()} zł", 
                         style: TextStyle(color: Colors.grey[400], fontSize: 12, decoration: TextDecoration.lineThrough)),
                ],
              ),
            )).toList(),
          ),
        ),
        const Divider(height: 1),
        // Cena całkowita
        Padding(
          padding: const EdgeInsets.all(16),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Row(
                children: [
                  Icon(Icons.schedule, size: 16, color: Colors.grey[400]),
                  const SizedBox(width: 4),
                  Text("Łącznie ~${bundle.totalDurationMinutes} min", style: TextStyle(color: Colors.grey[500], fontSize: 13)),
                ],
              ),
              Row(
                children: [
                  if (bundle.discountPercent > 0)
                    Padding(
                      padding: const EdgeInsets.only(right: 8),
                      child: Text("${bundle.originalTotalPrice.toInt()} zł", 
                                  style: TextStyle(color: Colors.grey[400], fontSize: 14, decoration: TextDecoration.lineThrough)),
                    ),
                  Text("${bundle.totalPrice.toInt()} zł", style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 18, color: Color(0xFF16a34a))),
                ],
              ),
            ],
          ),
        ),
      ],
    ),
  );
}

Widget _buildTeamTab() {
  return SingleChildScrollView(
    padding: const EdgeInsets.fromLTRB(16, 16, 16, 40),
    child: _buildSectionCard(
      title: "Nasz Zespół",
      icon: Icons.group_outlined,
      child: _isLoadingBusiness 
        ? const Center(child: CircularProgressIndicator())
        : _fullBusiness == null || _fullBusiness!.employees.isEmpty
          ? const Center(child: Padding(
              padding: EdgeInsets.all(20.0),
              child: Text("Brak przypisanych pracowników.", style: TextStyle(color: Colors.grey)),
            ))
          : GridView.builder(
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                crossAxisCount: 2,
                crossAxisSpacing: 16,
                mainAxisSpacing: 16,
                childAspectRatio: 0.75, // Proporcje karty
              ),
              itemCount: _fullBusiness!.employees.length,
              itemBuilder: (context, index) {
                return _buildEmployeeCard(_fullBusiness!.employees[index]);
              },
            ),
    ),
  );
}

Widget _buildEmployeeCard(EmployeeDto employee) {
  return InkWell(
    onTap: () {
      _showEmployeeDetails(employee);
    },
    borderRadius: BorderRadius.circular(16),
    child: Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: Colors.grey.shade200),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.03),
            blurRadius: 10,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Stack(
            children: [
              Container(
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  border: Border.all(color: const Color(0xFF16a34a).withOpacity(0.2), width: 3),
                ),
                child: CircleAvatar(
                  radius: 35,
                  backgroundColor: Colors.grey[100],
                  backgroundImage: employee.photoUrl != null ? NetworkImage(employee.photoUrl!) : null,
                  child: employee.photoUrl == null
                      ? Text(employee.firstName.isNotEmpty ? employee.firstName[0].toUpperCase() : '?', 
                             style: const TextStyle(fontSize: 24, color: Colors.grey, fontWeight: FontWeight.bold))
                      : null,
                ),
              ),
              if (employee.isStudent)
                Positioned(
                  bottom: -2,
                  right: -2,
                  child: Container(
                    padding: const EdgeInsets.all(4),
                    decoration: BoxDecoration(
                      color: Colors.orangeAccent,
                      shape: BoxShape.circle,
                      border: Border.all(color: Colors.white, width: 2),
                    ),
                    child: const Icon(Icons.school, size: 12, color: Colors.white),
                  ),
                ),
            ],
          ),
          const SizedBox(height: 12),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 8.0),
            child: Text(
              employee.fullName,
              style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14, color: Color(0xFF1F2937)),
              textAlign: TextAlign.center,
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
            ),
          ),
          if (employee.position != null || employee.specialization != null)
            Padding(
              padding: const EdgeInsets.only(top: 4.0, left: 8.0, right: 8.0),
              child: Text(
                employee.position ?? employee.specialization ?? "",
                style: TextStyle(color: Colors.grey[600], fontSize: 12),
                textAlign: TextAlign.center,
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
              ),
            ),
        ],
      ),
    ),
  );
}

void _showEmployeeDetails(EmployeeDto employee) {
  showModalBottomSheet(
    context: context,
    isScrollControlled: true,
    backgroundColor: Colors.transparent,
    builder: (context) {
      return Container(
        decoration: const BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
        ),
        padding: const EdgeInsets.fromLTRB(24, 12, 24, 24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.center,
          children: [
            Container(
              width: 40,
              height: 4,
              decoration: BoxDecoration(
                color: Colors.grey[300],
                borderRadius: BorderRadius.circular(2),
              ),
            ),
            const SizedBox(height: 24),
            CircleAvatar(
              radius: 50,
              backgroundColor: Colors.grey[100],
              backgroundImage: employee.photoUrl != null ? NetworkImage(employee.photoUrl!) : null,
              child: employee.photoUrl == null
                  ? Text(employee.firstName.isNotEmpty ? employee.firstName[0].toUpperCase() : '?', 
                         style: const TextStyle(fontSize: 32, color: Colors.grey))
                  : null,
            ),
            const SizedBox(height: 16),
            Text(employee.fullName, style: const TextStyle(fontSize: 24, fontWeight: FontWeight.bold, color: Color(0xFF1F2937))),
            if (employee.position != null || employee.specialization != null)
              Padding(
                padding: const EdgeInsets.only(top: 4.0),
                child: Text(employee.position ?? employee.specialization!, 
                            style: const TextStyle(fontSize: 16, color: Color(0xFF16a34a), fontWeight: FontWeight.w500)),
              ),
            const SizedBox(height: 24),
            if (employee.bio != null && employee.bio!.isNotEmpty) ...[
              const Align(
                alignment: Alignment.centerLeft,
                child: Text("O Mnie", style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16, color: Color(0xFF1F2937))),
              ),
              const SizedBox(height: 8),
              Text(
                employee.bio!, 
                style: TextStyle(color: Colors.grey[700], height: 1.5, fontSize: 14),
                textAlign: TextAlign.justify,
              ),
              const SizedBox(height: 24),
            ],
            SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                onPressed: () {
                  Navigator.pop(context);
                  DefaultTabController.of(this.context).animateTo(1); // Zakładka "Usługi" to index 1
                },
                icon: const Icon(Icons.calendar_today, size: 20),
                label: const Text("ZOBACZ USŁUGI", style: TextStyle(fontWeight: FontWeight.bold, letterSpacing: 1.1)),
                style: ElevatedButton.styleFrom(
                  backgroundColor: const Color(0xFF16a34a),
                  foregroundColor: Colors.white,
                  padding: const EdgeInsets.symmetric(vertical: 16),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                  elevation: 0,
                ),
              ),
            ),
            SafeArea(child: const SizedBox(height: 8)),
          ],
        ),
      );
    }
  );
}

Widget _buildReviewsTab() {
  return SingleChildScrollView(
    padding: const EdgeInsets.fromLTRB(16, 16, 16, 40),
    child: _buildSectionCard(
      title: "Opinie Klientów",
      icon: Icons.star_outline,
      child: _isLoadingReviews
        ? const Center(child: CircularProgressIndicator())
        : _reviews.isEmpty
          ? const Padding(
              padding: EdgeInsets.symmetric(vertical: 20),
              child: Center(child: Text("Brak opinii. Bądź pierwszy!", style: TextStyle(color: Colors.grey))),
            )
          : Column(
              children: [
                ListView.separated(
                  shrinkWrap: true,
                  physics: const NeverScrollableScrollPhysics(),
                  itemCount: _reviews.length,
                  separatorBuilder: (context, index) => const SizedBox(height: 20),
                  itemBuilder: (context, index) => _buildReviewItem(_reviews[index]),
                ),
                if (_hasMoreReviews) ...[
                  const SizedBox(height: 20),
                  SizedBox(
                    width: double.infinity,
                    child: OutlinedButton(
                      onPressed: _isLoadingMoreReviews ? null : () => _loadReviews(loadMore: true),
                      style: OutlinedButton.styleFrom(
                        foregroundColor: const Color(0xFF16a34a),
                        side: const BorderSide(color: Color(0xFF16a34a)),
                        padding: const EdgeInsets.symmetric(vertical: 14),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                      ),
                      child: _isLoadingMoreReviews
                          ? const SizedBox(height: 20, width: 20, child: CircularProgressIndicator(strokeWidth: 2))
                          : const Text("Ładuj więcej opinii", style: TextStyle(fontWeight: FontWeight.w600)),
                    ),
                  ),
                ],
              ],
            ),
    ),
  );
}

  // --- WIDGETY POMOCNICZE ---

  // Ogólna biała karta sekcji (jak w Angularze 'card bg-base-100')
  Widget _buildSectionCard({required String title, required IconData icon, required Widget child}) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 10,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Icon(icon, color: Colors.grey[400], size: 24),
                const SizedBox(width: 10),
                Text(
                  title,
                  style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: Color(0xFF1F2937)),
                ),
              ],
            ),
            const SizedBox(height: 20),
            child,
          ],
        ),
      ),
    );
  }

  // Wiersz usługi (Service Row)
  Widget _buildServiceRow(Service service, ServiceVariant variant) {
    return Material(
      color: Colors.transparent,
      child: InkWell(
        onTap: () {
          final dummyDto = ServiceDto(
             id: service.id,
             name: variant.name.toLowerCase() == "domyślny" || variant.name.toLowerCase() == "default" 
                 ? service.name 
                 : "${service.name} - ${variant.name}",
             description: service.description ?? "",
             price: variant.price,
             durationMinutes: variant.durationMinutes,
          );
          Navigator.push(
            context,
            MaterialPageRoute(
              builder: (context) => BookingScreen(
                business: widget.business,
                service: dummyDto,
              ),
            ),
          );
        },
        borderRadius: BorderRadius.circular(8),
        child: Padding(
          padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 8),
          child: Row(
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      variant.name.toLowerCase() == "domyślny" || variant.name.toLowerCase() == "default" 
                          ? service.name 
                          : "${service.name} - ${variant.name}",
                      style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 15, color: Color(0xFF1F2937)),
                    ),
                    const SizedBox(height: 6),
                    Row(
                      children: [
                        Icon(Icons.schedule, size: 14, color: Colors.grey[400]),
                        const SizedBox(width: 4),
                        Text(
                          "${variant.durationMinutes} min",
                          style: TextStyle(color: Colors.grey[500], fontSize: 13),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
              Row(
                children: [
                  Text(
                    "${variant.price.toInt()} zł",
                    style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16, color: Color(0xFF16a34a)),
                  ),
                  const SizedBox(width: 8),
                  Consumer<FavoritesProvider>(
                    builder: (context, provider, child) {
                      final isFav = provider.favorites.any((f) => f.serviceVariantId == variant.serviceVariantId);
                      return Padding(
                        padding: const EdgeInsets.only(right: 8.0),
                        child: InkWell(
                          onTap: () async {
                            if (isFav) {
                              await provider.removeFavorite(variant.serviceVariantId);
                            } else {
                              await provider.addFavorite(variant.serviceVariantId);
                            }
                          },
                          child: Icon(
                            isFav ? Icons.favorite : Icons.favorite_border,
                            size: 20,
                            color: isFav ? Colors.redAccent : Colors.grey[400],
                          ),
                        ),
                      );
                    },
                  ),
                  const SizedBox(width: 4),
                  Icon(Icons.arrow_forward_ios_rounded, size: 14, color: Colors.grey[300]),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }

  // Karta Opinii (z dymkiem)
  Widget _buildReviewItem(ReviewDto review) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        // Avatar
        CircleAvatar(
          radius: 20,
          backgroundColor: Colors.grey[200],
          backgroundImage: review.reviewerPhotoUrl != null ? NetworkImage(review.reviewerPhotoUrl!) : null,
          child: review.reviewerPhotoUrl == null
              ? Text(review.reviewerName.isNotEmpty ? review.reviewerName[0].toUpperCase() : '?',
                  style: const TextStyle(fontSize: 16, color: Colors.grey))
              : null,
        ),
        const SizedBox(width: 12),
        // Treść
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text(review.reviewerName, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14)),
                  Text(
                    "${review.createdAt.day}.${review.createdAt.month}.${review.createdAt.year}",
                    style: TextStyle(color: Colors.grey[400], fontSize: 12),
                  ),
                ],
              ),
              const SizedBox(height: 4),
              // Gwiazdki
              Row(
                children: List.generate(5, (index) {
                  return Icon(
                    index < review.rating ? Icons.star : Icons.star_border,
                    size: 14,
                    color: index < review.rating ? Colors.amber : Colors.grey[300],
                  );
                }),
              ),
              const SizedBox(height: 8),
              // Dymek z komentarzem (jak w Angularze)
              if (review.comment.isNotEmpty)
                Container(
                  width: double.infinity,
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    color: Colors.grey[100], // Szare tło (bg-base-200/50)
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Text(
                    review.comment,
                    style: TextStyle(color: Colors.grey[800], fontSize: 13, height: 1.4),
                  ),
                ),
            ],
          ),
        ),
      ],
    );
  }
}

class _SliverAppBarDelegate extends SliverPersistentHeaderDelegate {
  _SliverAppBarDelegate(this._tabBar);

  final TabBar _tabBar;

  @override
  double get minExtent => _tabBar.preferredSize.height;
  @override
  double get maxExtent => _tabBar.preferredSize.height;

  @override
  Widget build(BuildContext context, double shrinkOffset, bool overlapsContent) {
    return Container(
      color: Colors.white, // Tło paska zakładek
      child: _tabBar,
    );
  }

  @override
  bool shouldRebuild(_SliverAppBarDelegate oldDelegate) {
    return false;
  }
}