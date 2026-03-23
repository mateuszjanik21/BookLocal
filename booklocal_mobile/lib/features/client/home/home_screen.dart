import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../../core/services/auth_service.dart';
import 'providers/home_provider.dart';
import 'widgets/home_top_bar.dart';
import 'widgets/category_buttons.dart';
import 'widgets/service_category_grid.dart';
import 'widgets/sorting_header.dart';
import 'widgets/rebook_section.dart';

class HomeScreen extends StatelessWidget {
  const HomeScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return ChangeNotifierProvider(
      create: (context) {
        final authService = Provider.of<AuthService>(context, listen: false);
        final provider = HomeProvider();
        provider.init(authService);
        return provider;
      },
      child: const _HomeScreenBody(),
    );
  }
}

class _HomeScreenBody extends StatelessWidget {
  const _HomeScreenBody();

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF8FAF9),
      body: CustomScrollView(
        slivers: [
          const SliverToBoxAdapter(
            child: HomeTopBar(),
          ),
          
          const SliverToBoxAdapter(
            child: CategoryButtons(),
          ),

          const SliverToBoxAdapter(
            child: RebookSection(),
          ),

          const SliverToBoxAdapter(
            child: SortingHeader(),
          ),

          const SliverPadding(
            padding: EdgeInsets.symmetric(horizontal: 16.0, vertical: 8.0),
            sliver: ServiceCategoryGrid(),
          ),
        ],
      ),
    );
  }
}