import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../core/services/presence_service.dart';
import 'providers/chat_provider.dart';
import 'widgets/conversation_list_item.dart';

class ChatListScreen extends StatefulWidget {
  const ChatListScreen({super.key});

  @override
  State<ChatListScreen> createState() => _ChatListScreenState();
}

class _ChatListScreenState extends State<ChatListScreen> {
  late final ChatProvider _chatProvider;
  late final PresenceService _presenceService;

  @override
  void initState() {
    super.initState();
    _chatProvider = Provider.of<ChatProvider>(context, listen: false);
    _presenceService = Provider.of<PresenceService>(context, listen: false);

    // Nasłuchuj zmian PresenceService (nowe wiadomości) → odśwież listę
    _presenceService.addListener(_onPresenceChanged);

    WidgetsBinding.instance.addPostFrameCallback((_) {
      _refresh();
    });
  }

  void _onPresenceChanged() {
    // Gdy PresenceService zmieni unread count (nowa wiadomość),
    // odśwież listę konwersacji by pokazać nowe dane
    _chatProvider.loadMyConversations();
  }

  @override
  void dispose() {
    _presenceService.removeListener(_onPresenceChanged);
    super.dispose();
  }

  Future<void> _refresh() async {
    await _chatProvider.loadMyConversations();
    await _presenceService.refreshUnreadCount();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF3F4F6),
      appBar: AppBar(
        title: const Text("Wiadomości", style: TextStyle(fontWeight: FontWeight.bold)),
        backgroundColor: Colors.white,
        foregroundColor: Colors.black,
        elevation: 0,
        surfaceTintColor: Colors.transparent,
      ),
      body: Consumer<ChatProvider>(
        builder: (context, provider, child) {
          if (provider.isLoadingConversations) {
            return const Center(child: CircularProgressIndicator());
          }

          final conversations = List.of(provider.conversations)
            ..sort((a, b) => b.lastMessageDate.compareTo(a.lastMessageDate));

          if (conversations.isEmpty) {
            return Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Container(
                    width: 100,
                    height: 100,
                    decoration: BoxDecoration(
                      color: const Color(0xFF16a34a).withOpacity(0.05),
                      shape: BoxShape.circle,
                    ),
                    child: Icon(Icons.chat_bubble_outline_rounded, size: 48, color: const Color(0xFF16a34a).withOpacity(0.5)),
                  ),
                  const SizedBox(height: 24),
                  const Text(
                    "Brak wiadomości",
                    style: TextStyle(
                      fontSize: 20,
                      fontWeight: FontWeight.bold,
                      color: Colors.black87,
                    ),
                  ),
                  const SizedBox(height: 12),
                  Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 40),
                    child: Text(
                      "Tutaj pojawią się Twoje konwersacje z salonami. Umów wizytę, aby rozpocząć czat!",
                      textAlign: TextAlign.center,
                      style: TextStyle(
                        fontSize: 14,
                        color: Colors.grey[600],
                        height: 1.4,
                      ),
                    ),
                  ),
                ],
              ),
            );
          }

          return RefreshIndicator(
            onRefresh: _refresh,
            color: const Color(0xFF16a34a),
            child: ListView.builder(
              physics: const AlwaysScrollableScrollPhysics(),
              padding: const EdgeInsets.only(top: 8, bottom: 24),
              itemCount: conversations.length,
              itemBuilder: (context, index) {
                final conv = conversations[index];
                return ConversationListItem(
                  conversation: conv,
                  onReturn: () => _refresh(),
                );
              },
            ),
          );
        },
      ),
    );
  }
}