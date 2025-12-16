import 'dart:ui'; // Potrzebne do ImageFilter
import 'package:booklocal_mobile/core/services/chat_services.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../core/models/business_list_item_dto.dart';
import '../../../core/models/service_models.dart';
import '../../../core/services/client_service.dart';
import '../booking/booking_screen.dart';
import '../chat/conversation_screen.dart';
import '../../../core/models/review_models.dart';
import '../../../core/services/review_service.dart';

class BusinessDetailsScreen extends StatefulWidget {
  final BusinessListItemDto business;
  
  const BusinessDetailsScreen({super.key, required this.business});

  @override
  State<BusinessDetailsScreen> createState() => _BusinessDetailsScreenState();
}

class _BusinessDetailsScreenState extends State<BusinessDetailsScreen> {
  List<ServiceDto> _services = [];
  bool _isLoading = true;
  List<ReviewDto> _reviews = [];
  bool _isLoadingReviews = true;

  @override
  void initState() {
    super.initState();
    _loadServices();
    _loadReviews();
  }

  Future<void> _loadReviews() async {
    final reviewService = Provider.of<ReviewService>(context, listen: false);
    final result = await reviewService.getReviews(widget.business.id, pageNumber: 1, pageSize: 5);
    
    if (mounted) {
      setState(() {
        _reviews = result.items;
        _isLoadingReviews = false;
      });
    }
  }

  Future<void> _loadServices() async {
    final clientService = Provider.of<ClientService>(context, listen: false);
    final services = await clientService.getBusinessServices(widget.business.id);

    if (mounted) {
      setState(() {
        _services = services;
        _isLoading = false;
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

    return Scaffold(
      backgroundColor: backgroundColor,
      body: CustomScrollView(
        slivers: [
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
                                "${widget.business.rating} (${widget.business.reviewCount} opinii)",
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

          // 2. KARTA "O NAS" (Description)
          SliverToBoxAdapter(
            child: Padding(
              padding: const EdgeInsets.fromLTRB(16, 24, 16, 0),
              child: _buildSectionCard(
                title: "O Nas",
                icon: Icons.info_outline,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      "Zapraszamy do naszego salonu w mieście ${widget.business.city}. Oferujemy profesjonalne usługi najwyższej jakości.", // Placeholder jeśli brak opisu w DTO
                      style: TextStyle(color: Colors.grey[700], height: 1.5),
                    ),
                    const SizedBox(height: 15),
                    const Divider(),
                    const SizedBox(height: 10),
                    SizedBox(
                      width: double.infinity,
                      child: OutlinedButton.icon(
                        onPressed: _startChat,
                        icon: const Icon(Icons.chat_bubble_outline),
                        label: const Text("Napisz wiadomość"),
                        style: OutlinedButton.styleFrom(
                          foregroundColor: primaryColor,
                          side: const BorderSide(color: primaryColor),
                          padding: const EdgeInsets.symmetric(vertical: 12),
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),

          // 3. KARTA "CENNIK USŁUG"
          SliverToBoxAdapter(
            child: Padding(
              padding: const EdgeInsets.fromLTRB(16, 20, 16, 0),
              child: _buildSectionCard(
                title: "Cennik Usług",
                icon: Icons.list_alt,
                child: _isLoading 
                  ? const Center(child: CircularProgressIndicator())
                  : _services.isEmpty
                    ? const Text("Brak usług.", style: TextStyle(color: Colors.grey))
                    : ListView.separated(
                        shrinkWrap: true,
                        physics: const NeverScrollableScrollPhysics(),
                        itemCount: _services.length,
                        separatorBuilder: (context, index) => const Divider(height: 20),
                        itemBuilder: (context, index) => _buildServiceRow(_services[index]),
                      ),
              ),
            ),
          ),

          // 4. KARTA "OPINIE"
          SliverToBoxAdapter(
            child: Padding(
              padding: const EdgeInsets.fromLTRB(16, 20, 16, 40),
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
                    : ListView.separated(
                        shrinkWrap: true,
                        physics: const NeverScrollableScrollPhysics(),
                        itemCount: _reviews.length,
                        separatorBuilder: (context, index) => const SizedBox(height: 20),
                        itemBuilder: (context, index) => _buildReviewItem(_reviews[index]),
                      ),
              ),
            ),
          ),
        ],
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
  Widget _buildServiceRow(ServiceDto service) {
    return Material(
      color: Colors.transparent,
      child: InkWell(
        onTap: () {
          Navigator.push(
            context,
            MaterialPageRoute(
              builder: (context) => BookingScreen(
                business: widget.business,
                service: service,
              ),
            ),
          );
        },
        borderRadius: BorderRadius.circular(8),
        child: Padding(
          padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 8),
          child: Row(
            children: [
              // 1. Nazwa i czas trwania (Lewa strona)
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      service.name,
                      style: const TextStyle(
                        fontWeight: FontWeight.w600,
                        fontSize: 15,
                        color: Color(0xFF1F2937), // Ciemny grafit
                      ),
                    ),
                    const SizedBox(height: 6),
                    Row(
                      children: [
                        Icon(Icons.schedule, size: 14, color: Colors.grey[400]),
                        const SizedBox(width: 4),
                        Text(
                          "${service.durationMinutes} min",
                          style: TextStyle(color: Colors.grey[500], fontSize: 13),
                        ),
                      ],
                    ),
                  ],
                ),
              ),

              // 2. Cena i strzałka (Prawa strona)
              Row(
                children: [
                  Text(
                    "${service.price.toInt()} zł",
                    style: const TextStyle(
                      fontWeight: FontWeight.bold,
                      fontSize: 16,
                      color: Color(0xFF16a34a), // Twój zielony kolor firmowy
                    ),
                  ),
                  const SizedBox(width: 12),
                  Icon(
                    Icons.arrow_forward_ios_rounded,
                    size: 14,
                    color: Colors.grey[300], // Subtelna strzałka
                  ),
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