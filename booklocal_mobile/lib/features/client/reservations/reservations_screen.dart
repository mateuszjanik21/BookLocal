import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../../core/models/reservation_models.dart';
import '../../../core/services/reservation_service.dart';
import 'providers/reservations_provider.dart';
import 'widgets/reservation_card.dart';
import 'widgets/reservation_skeleton.dart';
import 'widgets/empty_state.dart';
import 'add_review_dialog.dart';

class ReservationsScreen extends StatelessWidget {
  const ReservationsScreen({super.key});

  void _showReviewFlow(BuildContext context, ReservationDto reservation) async {
    final provider = Provider.of<ReservationsProvider>(context, listen: false);

    int targetId = reservation.reservationId;
    String targetName = reservation.serviceName;

    if (reservation.isBundle && reservation.subItems != null && reservation.subItems!.isNotEmpty) {
      final selected = await showModalBottomSheet<BundleSubItemDto>(
        context: context,
        backgroundColor: Colors.white,
        shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(24))),
        builder: (ctx) => _BundleServicePicker(items: reservation.subItems!),
      );

      if (selected == null) return;
      targetId = selected.reservationId;
      targetName = selected.serviceName;
    }

    if (!context.mounted) return;

    final result = await showModalBottomSheet<Map<String, dynamic>>(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (ctx) => AddReviewDialog(
        serviceName: targetName,
        businessName: reservation.businessName,
      ),
    );

    if (result != null && context.mounted) {
      final bool success = await provider.submitReview(
        reservationId: targetId,
        rating: result['rating'],
        comment: result['comment'],
      );

      if (success && context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text("Dziękujemy za Twoją opinię!")),
        );
      } else if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text("Błąd podczas dodawania opinii.")),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    const primaryColor = Color(0xFF16a34a);

    return ChangeNotifierProvider(
      create: (context) {
        final service = Provider.of<ReservationService>(context, listen: false);
        return ReservationsProvider(service)..init();
      },
      child: DefaultTabController(
        length: 2,
        child: Scaffold(
          backgroundColor: const Color(0xFFF3F4F6),
          appBar: AppBar(
            backgroundColor: Colors.white,
            elevation: 0,
            toolbarHeight: 70,
            title: const Text(
              'Moje wizyty',
              style: TextStyle(
                color: Colors.black, 
                fontWeight: FontWeight.w900,
                fontSize: 32,
                letterSpacing: -1.5,
              ),
            ),
            centerTitle: false,
            bottom: PreferredSize(
              preferredSize: const Size.fromHeight(60),
              child: Container(
                margin: const EdgeInsets.fromLTRB(20, 0, 20, 15),
                padding: const EdgeInsets.all(4),
                decoration: BoxDecoration(
                  color: Colors.grey.shade100,
                  borderRadius: BorderRadius.circular(15),
                ),
                child: TabBar(
                  indicator: BoxDecoration(
                    color: primaryColor,
                    borderRadius: BorderRadius.circular(12),
                    boxShadow: [
                      BoxShadow(
                        color: primaryColor.withOpacity(0.3),
                        blurRadius: 8,
                        offset: const Offset(0, 4),
                      ),
                    ],
                  ),
                  labelColor: Colors.white,
                  unselectedLabelColor: Colors.grey[600],
                  labelStyle: const TextStyle(fontWeight: FontWeight.w800, fontSize: 13, letterSpacing: 0.5),
                  unselectedLabelStyle: const TextStyle(fontWeight: FontWeight.w700, fontSize: 13),
                  dividerColor: Colors.transparent,
                  indicatorSize: TabBarIndicatorSize.tab,
                  tabs: const [
                    Tab(text: 'Wkrótce'),
                    Tab(text: 'Poprzednie'),
                  ],
                ),
              ),
            ),
          ),
          body: TabBarView(
            physics: const BouncingScrollPhysics(),
            children: [
              const _UpcomingTab(),
              _PastTab(onReviewRequested: (ctx, res) => _showReviewFlow(ctx, res)),
            ],
          ),
        ),
      ),
    );
  }
}

class _BundleServicePicker extends StatelessWidget {
  final List<BundleSubItemDto> items;

  const _BundleServicePicker({required this.items});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(vertical: 20),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Padding(
            padding: EdgeInsets.symmetric(horizontal: 24, vertical: 8),
            child: Text(
              "Wybierz usługę do oceny",
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
          ),
          const SizedBox(height: 8),
          ...items.map((item) => ListTile(
                leading: const Icon(Icons.check_circle_outline, color: Color(0xFF16a34a)),
                title: Text(item.serviceName, style: const TextStyle(fontWeight: FontWeight.bold)),
                subtitle: Text(item.variantName),
                onTap: () => Navigator.pop(context, item),
              )),
          const SizedBox(height: 20),
        ],
      ),
    );
  }
}

class _UpcomingTab extends StatelessWidget {
  const _UpcomingTab();

  @override
  Widget build(BuildContext context) {
    final provider = Provider.of<ReservationsProvider>(context);

    if (provider.isLoadingUpcoming) {
      return const ReservationSkeleton();
    }

    if (provider.upcomingReservations.isEmpty) {
      return EmptyStateWidget(
        title: "Brak wizyt",
        message: "Nie masz zaplanowanych żadnych wizyt.\nZnajdź interesujący Cię salon i umów się już dziś!",
        icon: Icons.calendar_today_outlined,
        actionLabel: "Znajdź salon",
        onAction: () {
          Navigator.of(context, rootNavigator: true).pushReplacementNamed('/');
        },
      );
    }

    return RefreshIndicator(
      onRefresh: () => provider.fetchUpcoming(),
      color: const Color(0xFF16a34a),
      child: ListView.builder(
        padding: const EdgeInsets.all(20.0),
        physics: const AlwaysScrollableScrollPhysics(parent: BouncingScrollPhysics()),
        itemCount: provider.upcomingReservations.length,
        itemBuilder: (context, index) {
          final res = provider.upcomingReservations[index];
          return ReservationCard(
            reservation: res,
            isPast: false,
          );
        },
      ),
    );
  }
}

class _PastTab extends StatefulWidget {
  final Function(BuildContext, ReservationDto) onReviewRequested;

  const _PastTab({required this.onReviewRequested});

  @override
  State<_PastTab> createState() => _PastTabState();
}

class _PastTabState extends State<_PastTab> {
  final ScrollController _scrollController = ScrollController();

  @override
  void initState() {
    super.initState();
    _scrollController.addListener(_onScroll);
  }

  void _onScroll() {
    if (_scrollController.position.pixels >= _scrollController.position.maxScrollExtent - 200) {
      Provider.of<ReservationsProvider>(context, listen: false).loadMorePastReservations();
    }
  }

  @override
  void dispose() {
    _scrollController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final provider = Provider.of<ReservationsProvider>(context);

    if (provider.isLoadingPast) {
      return const ReservationSkeleton();
    }

    if (provider.pastReservations.isEmpty) {
      return EmptyStateWidget(
        title: "Historia jest pusta",
        message: "Tutaj pojawią się Twoje zrealizowane wizyty.",
        icon: Icons.history,
        actionLabel: "Odkryj salony",
        onAction: () {
          Navigator.of(context, rootNavigator: true).pushReplacementNamed('/');
        },
      );
    }

    return RefreshIndicator(
      onRefresh: () => provider.fetchPast(),
      color: const Color(0xFF16a34a),
      child: ListView.builder(
        controller: _scrollController,
        padding: const EdgeInsets.all(20.0),
        physics: const AlwaysScrollableScrollPhysics(parent: BouncingScrollPhysics()),
        itemCount: provider.pastReservations.length + (provider.hasMorePast ? 1 : 0),
        itemBuilder: (context, index) {
          if (index == provider.pastReservations.length) {
            return const Padding(
              padding: EdgeInsets.symmetric(vertical: 24.0),
              child: Center(
                child: SizedBox(
                  width: 24,
                  height: 24,
                  child: CircularProgressIndicator(strokeWidth: 2, color: Color(0xFF16a34a)),
                ),
              ),
            );
          }

          final res = provider.pastReservations[index];
          return ReservationCard(
            reservation: res,
            isPast: true,
            onReview: () => widget.onReviewRequested(context, res),
          );
        },
      ),
    );
  }
}