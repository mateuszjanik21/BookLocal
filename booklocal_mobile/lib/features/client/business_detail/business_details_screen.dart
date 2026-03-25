import 'dart:ui'; // Potrzebne do ImageFilter
import 'package:booklocal_mobile/core/services/chat_services.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../core/models/business_detail_dto.dart';
import '../../../core/models/business_list_item_dto.dart';
import '../../../core/models/review_models.dart';
import '../../../core/models/service_bundle_dto.dart';
import '../../../core/services/client_service.dart';
import '../../../core/services/review_service.dart';
import '../../../core/services/service_bundle_service.dart';
import '../chat/conversation_screen.dart';
import 'widgets/about_tab.dart';
import 'widgets/bundles_tab.dart';
import 'widgets/reviews_tab.dart';
import 'widgets/services_tab.dart';
import 'widgets/team_tab.dart';
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

  Future<void> _handleRefresh() async {
    setState(() {
      _isLoadingBusiness = true;
      _isLoadingReviews = true;
      _isLoadingBundles = true;
    });
    
    await Future.wait([
      _loadBusinessDetails(),
      _loadReviews(),
      _loadBundles(),
    ]);

    if (mounted) {
      // Import FavoritesProvider to refresh it (added in next edit if needed, or already works if imported)
      try {
        Provider.of<FavoritesProvider>(context, listen: false).fetchFavorites(refresh: true);
      } catch (e) {
        // Zignoruj jeśli provider nie jest dostępny
      }
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
    const primaryColor = Color(0xFF16a34a);
    const backgroundColor = Color(0xFFF3F4F6);

    return DefaultTabController(
      length: 5,
      child: Scaffold(
        backgroundColor: backgroundColor,
        body: NestedScrollView(
          physics: const BouncingScrollPhysics(parent: AlwaysScrollableScrollPhysics()),
          headerSliverBuilder: (BuildContext context, bool innerBoxIsScrolled) {
            return <Widget>[
              // 1. HEADER (Banner z rozmyciem)
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
                      widget.business.photoUrl != null
                          ? Image.network(widget.business.photoUrl!, fit: BoxFit.cover)
                          : Container(color: Colors.grey[800]),
                      BackdropFilter(
                        filter: ImageFilter.blur(sigmaX: 10.0, sigmaY: 10.0),
                        child: Container(color: Colors.black.withOpacity(0.4)),
                      ),
                      Center(
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            const SizedBox(height: 40),
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
                            Text(
                              widget.business.city,
                              style: TextStyle(color: Colors.white.withOpacity(0.9), fontSize: 16),
                            ),
                            const SizedBox(height: 15),
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

              // 2. STICKY TABBAR
              SliverPersistentHeader(
                pinned: true,
                delegate: _SliverAppBarDelegate(
                  TabBar(
                    isScrollable: true,
                    labelColor: primaryColor,
                    unselectedLabelColor: Colors.grey[600],
                    indicatorColor: primaryColor,
                    indicatorWeight: 3,
                    tabAlignment: TabAlignment.start,
                    labelStyle: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
                    unselectedLabelStyle: const TextStyle(fontWeight: FontWeight.w500, fontSize: 14),
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
              RefreshIndicator(
                color: primaryColor,
                onRefresh: _handleRefresh,
                child: AboutTab(
                  fullBusiness: _fullBusiness,
                  fallbackCity: widget.business.city,
                  isLoading: _isLoadingBusiness,
                  onStartChat: _startChat,
                ),
              ),
              RefreshIndicator(
                color: primaryColor,
                onRefresh: _handleRefresh,
                child: ServicesTab(
                  fullBusiness: _fullBusiness,
                  business: widget.business,
                  isLoading: _isLoadingBusiness,
                ),
              ),
              RefreshIndicator(
                color: primaryColor,
                onRefresh: _handleRefresh,
                child: BundlesTab(
                  bundles: _bundles,
                  isLoading: _isLoadingBundles,
                ),
              ),
              RefreshIndicator(
                color: primaryColor,
                onRefresh: _handleRefresh,
                child: TeamTab(
                  fullBusiness: _fullBusiness,
                  isLoading: _isLoadingBusiness,
                ),
              ),
              RefreshIndicator(
                color: primaryColor,
                onRefresh: _handleRefresh,
                child: ReviewsTab(
                  reviews: _reviews,
                  isLoading: _isLoadingReviews,
                  isLoadingMore: _isLoadingMoreReviews,
                  hasMore: _hasMoreReviews,
                  onLoadMore: () => _loadReviews(loadMore: true),
                ),
              ),
            ],
          ),
        ),
      ),
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
      color: Colors.white,
      child: _tabBar,
    );
  }

  @override
  bool shouldRebuild(_SliverAppBarDelegate oldDelegate) {
    return false;
  }
}
