import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import 'providers/profile_provider.dart';
import 'widgets/profile_header.dart';
import 'widgets/profile_stats_section.dart';
import 'widgets/profile_menu_section.dart';

class ProfileScreen extends StatefulWidget {
  const ProfileScreen({super.key});

  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      Provider.of<ProfileProvider>(context, listen: false).loadStats();
    });
  }

  @override
  Widget build(BuildContext context) {
    const primaryColor = Color(0xFF16a34a);

    return Scaffold(
      backgroundColor: const Color(0xFFF3F4F6),
      body: RefreshIndicator(
        color: primaryColor,
        onRefresh: () => Provider.of<ProfileProvider>(context, listen: false).loadStats(),
        child: SingleChildScrollView(
          physics: const AlwaysScrollableScrollPhysics(),
          child: Column(
            children: [
              const ProfileHeader(),
              const ProfileStatsSection(),
              const SizedBox(height: 20),
              const ProfileMenuSection(),
              const SizedBox(height: 32),
              Text(
                'BookLocal v1.0.0',
                style: TextStyle(fontSize: 12, color: Colors.grey[400]),
              ),
              const SizedBox(height: 40),
            ],
          ),
        ),
      ),
    );
  }
}