import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'home/home_screen.dart';
import 'favorites/favorites_screen.dart';
import 'reservations/reservations_screen.dart';
import 'chat/chat_list_screen.dart';
import '../../core/services/presence_service.dart';
import 'profile/profile_screen.dart';


class MainScreen extends StatefulWidget {
  final int initialIndex; 
  const MainScreen({super.key, this.initialIndex = 0}); 

  @override
  State<MainScreen> createState() => _MainScreenState();
}

class _MainScreenState extends State<MainScreen> {
  int _selectedIndex = 0;
  List<bool> _visitedTabs = [];

  final List<GlobalKey<NavigatorState>> _navigatorKeys = [
    GlobalKey<NavigatorState>(),
    GlobalKey<NavigatorState>(),
    GlobalKey<NavigatorState>(),
    GlobalKey<NavigatorState>(),
    GlobalKey<NavigatorState>(),
  ];

  @override
  void initState() {
    super.initState();
    _selectedIndex = widget.initialIndex;
    _visitedTabs = List<bool>.generate(5, (index) => index == _selectedIndex);

    // Jeśli startujemy na zakładce Czat, od razu wycisz toasty
    WidgetsBinding.instance.addPostFrameCallback((_) {
      final presence = Provider.of<PresenceService>(context, listen: false);
      presence.setChatScreenActive(_selectedIndex == 2);
    });
  }

  void _onItemTapped(int index) {
    if (_selectedIndex == index) {
      _navigatorKeys[index].currentState?.popUntil((route) => route.isFirst);
    } else {
      setState(() {
        _selectedIndex = index;
        _visitedTabs[index] = true; 
      });
    }
    final presence = Provider.of<PresenceService>(context, listen: false);
    presence.setChatScreenActive(index == 2);

    // Gdy wychodzimy z Czatu → wyczyść aktywną konwersację
    // (IndexedStack nie niszczy ConversationScreen, więc dispose() się nie wywołuje)
    if (index != 2) {
      presence.setActiveConversationId(null);
    }
  }

  Widget _buildNavigator(int index, Widget rootWidget) {
    return Navigator(
      key: _navigatorKeys[index],
      onGenerateRoute: (settings) {
        return MaterialPageRoute(
          builder: (_) => rootWidget,
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    const primaryColor = Color(0xFF16a34a);

    return PopScope(
      canPop: false,
      onPopInvokedWithResult: (didPop, result) async {
        if (didPop) return;

        final isFirstRouteInCurrentTab = !await _navigatorKeys[_selectedIndex].currentState!.maybePop();
        if (isFirstRouteInCurrentTab) {
          if (_selectedIndex != 0) {
            setState(() { 
              _selectedIndex = 0; 
              _visitedTabs[0] = true;
            });
          } else {
            return;
          }
        }
      },
      child: Scaffold(
        body: SafeArea(
          child: IndexedStack(
            index: _selectedIndex,
            children: [
              _visitedTabs[0] ? _buildNavigator(0, const HomeScreen()) : const SizedBox.shrink(),
              _visitedTabs[1] ? _buildNavigator(1, const ReservationsScreen()) : const SizedBox.shrink(),
              _visitedTabs[2] ? _buildNavigator(2, const ChatListScreen()) : const SizedBox.shrink(),
              _visitedTabs[3] ? _buildNavigator(3, const FavoritesScreen()) : const SizedBox.shrink(),
              _visitedTabs[4] ? _buildNavigator(4, const ProfileScreen()) : const SizedBox.shrink(),
            ],
          ),
        ),
        bottomNavigationBar: Container(
          decoration: BoxDecoration(
            boxShadow: [
              BoxShadow(
                color: Colors.black.withOpacity(0.1),
                blurRadius: 10,
                offset: const Offset(0, -5),
              ),
            ],
          ),
          child: BottomNavigationBar(
            items: <BottomNavigationBarItem>[
              const BottomNavigationBarItem(
                icon: Icon(Icons.home_outlined),
                activeIcon: Icon(Icons.home),
                label: 'Start',
              ),
              const BottomNavigationBarItem(
                icon: Icon(Icons.calendar_today_outlined),
                activeIcon: Icon(Icons.calendar_month),
                label: 'Wizyty',
              ),
              BottomNavigationBarItem(
                icon: Consumer<PresenceService>(
                  builder: (context, presence, child) {
                    final unread = presence.totalUnreadCount;
                    return unread > 0
                        ? Badge(
                            label: Text(unread.toString(), style: const TextStyle(fontWeight: FontWeight.bold)),
                            backgroundColor: Colors.redAccent,
                            child: const Icon(Icons.chat_bubble_outline),
                          )
                        : const Icon(Icons.chat_bubble_outline);
                  },
                ),
                activeIcon: Consumer<PresenceService>(
                  builder: (context, presence, child) {
                    final unread = presence.totalUnreadCount;
                    return unread > 0
                        ? Badge(
                            label: Text(unread.toString(), style: const TextStyle(fontWeight: FontWeight.bold)),
                            backgroundColor: Colors.redAccent,
                            child: const Icon(Icons.chat_bubble),
                          )
                        : const Icon(Icons.chat_bubble);
                  },
                ),
                label: 'Czat',
              ),
              const BottomNavigationBarItem(
                icon: Icon(Icons.favorite_outline),
                activeIcon: Icon(Icons.favorite),
                label: 'Ulubione',
              ),
              const BottomNavigationBarItem(
                icon: Icon(Icons.person_outline),
                activeIcon: Icon(Icons.person),
                label: 'Profil',
              ),
            ],
            currentIndex: _selectedIndex,
            selectedItemColor: primaryColor,
            unselectedItemColor: Colors.grey,
            showUnselectedLabels: true,
            type: BottomNavigationBarType.fixed,
            onTap: _onItemTapped,
            backgroundColor: Colors.white,
            elevation: 0,
            selectedLabelStyle: const TextStyle(fontWeight: FontWeight.bold, fontSize: 12),
            unselectedLabelStyle: const TextStyle(fontSize: 12),
          ),
        ),
      ),
    );
  }
}