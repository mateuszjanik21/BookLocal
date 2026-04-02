import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../../../core/services/auth_service.dart';
import '../../../core/services/presence_service.dart';
import 'providers/chat_provider.dart';
import 'widgets/message_bubble.dart';

class ConversationScreen extends StatefulWidget {
  final int conversationId;
  final String participantName;

  const ConversationScreen({
    super.key,
    required this.conversationId,
    required this.participantName,
  });

  @override
  State<ConversationScreen> createState() => _ConversationScreenState();
}

class _ConversationScreenState extends State<ConversationScreen> {
  final TextEditingController _messageController = TextEditingController();
  final ScrollController _scrollController = ScrollController();
  String _currentUserId = "";

  // Zapisujemy referencje w initState — KLUCZOWE!
  // Provider.of(context) w dispose() NIE DZIAŁA niezawodnie.
  late final ChatProvider _chatProvider;
  late final PresenceService _presenceService;

  @override
  void initState() {
    super.initState();
    final authService = Provider.of<AuthService>(context, listen: false);
    _currentUserId = authService.currentUser?.id ?? "";

    // Zapisz referencje RAZ — będą użyte w dispose()
    _chatProvider = Provider.of<ChatProvider>(context, listen: false);
    _presenceService = Provider.of<PresenceService>(context, listen: false);

    WidgetsBinding.instance.addPostFrameCallback((_) async {
      // Połącz ChatProvider z PresenceService (dla badge'a)
      _chatProvider.setPresenceService(_presenceService);

      // Ustaw aktywną konwersację — blokuje toasty dla tego czatu
      _chatProvider.setActiveConversationId(widget.conversationId);
      _presenceService.setActiveConversationId(widget.conversationId);

      // Załaduj historię wiadomości
      await _chatProvider.loadMessageThread(widget.conversationId);

      // Rozpocznij nasłuch SignalR + oznacz jako przeczytane
      await _chatProvider.startListening(widget.conversationId);

      _scrollToBottom();
      _chatProvider.addListener(_onMessagesUpdated);
    });
  }

  void _onMessagesUpdated() {
    _scrollToBottom();
  }

  void _scrollToBottom() {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (_scrollController.hasClients) {
        _scrollController.animateTo(
          0.0,
          duration: const Duration(milliseconds: 300),
          curve: Curves.easeOut,
        );
      }
    });
  }

  Future<void> _sendMessage() async {
    final text = _messageController.text.trim();
    if (text.isEmpty) return;

    _messageController.clear();

    try {
      await _chatProvider.sendMessage(widget.conversationId, text, _currentUserId);
      _scrollToBottom();
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text("Błąd wysyłania: $e")));
      }
    }
  }

  @override
  void dispose() {
    // Używamy ZAPISANYCH referencji — NIE Provider.of(context)!
    _chatProvider.setActiveConversationId(null);
    _presenceService.setActiveConversationId(null);

    _chatProvider.stopListening();
    _chatProvider.removeListener(_onMessagesUpdated);

    // Odśwież listę konwersacji i badge po wyjściu
    _chatProvider.loadMyConversations();
    _presenceService.refreshUnreadCount();

    _messageController.dispose();
    _scrollController.dispose();
    super.dispose();
  }

  Widget _buildDateSeparator(DateTime date) {
    final now = DateTime.now();
    final today = DateTime(now.year, now.month, now.day);
    final messageDay = DateTime(date.year, date.month, date.day);
    final difference = today.difference(messageDay).inDays;

    String label;
    if (difference == 0) {
      label = 'Dzisiaj';
    } else if (difference == 1) {
      label = 'Wczoraj';
    } else if (date.year == now.year) {
      label = DateFormat('d MMMM', 'pl').format(date);
    } else {
      label = DateFormat('d MMMM yyyy', 'pl').format(date);
    }

    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 10),
      child: Center(
        child: Container(
          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 6),
          decoration: BoxDecoration(
            color: Colors.black.withOpacity(0.06),
            borderRadius: BorderRadius.circular(12),
          ),
          child: Text(
            label,
            style: TextStyle(
              fontSize: 12,
              fontWeight: FontWeight.w600,
              color: Colors.grey[600],
            ),
          ),
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF3F4F6),
      appBar: AppBar(
        title: Text(widget.participantName, style: const TextStyle(fontWeight: FontWeight.bold)),
        backgroundColor: Colors.white,
        foregroundColor: Colors.black,
        elevation: 1,
        surfaceTintColor: Colors.transparent,
        shadowColor: Colors.black.withOpacity(0.3),
      ),
      body: Column(
        children: [
          Expanded(
            child: Consumer<ChatProvider>(
              builder: (context, provider, child) {
                if (provider.isLoadingMessages) {
                  return const Center(child: CircularProgressIndicator());
                }

                if (provider.currentMessages.isEmpty) {
                  return Center(
                    child: Text(
                      "Rozpocznij rozmowę z ${widget.participantName}",
                      style: const TextStyle(color: Colors.grey),
                    ),
                  );
                }

                return ListView.builder(
                  controller: _scrollController,
                  reverse: true,
                  padding: const EdgeInsets.all(15),
                  itemCount: provider.currentMessages.length,
                  itemBuilder: (context, index) {
                    final messages = provider.currentMessages;
                    final msgIndex = messages.length - 1 - index;
                    final msg = messages[msgIndex];
                    final isMe = msg.senderId == _currentUserId;

                    // Sprawdź czy trzeba pokazać separator daty.
                    // W reversed list: separator pokazujemy PO bąbelku (wizualnie = NAD nim).
                    // Pokazuj jeśli to pierwsza wiadomość lub poprzednia (chronologicznie) jest z innego dnia.
                    bool showDateSeparator = false;
                    if (msgIndex == 0) {
                      showDateSeparator = true;
                    } else {
                      final prevMsg = messages[msgIndex - 1];
                      final msgDate = msg.messageSent.toLocal();
                      final prevDate = prevMsg.messageSent.toLocal();
                      if (msgDate.year != prevDate.year || msgDate.month != prevDate.month || msgDate.day != prevDate.day) {
                        showDateSeparator = true;
                      }
                    }

                    return Column(
                      children: [
                        if (showDateSeparator) _buildDateSeparator(msg.messageSent.toLocal()),
                        MessageBubble(message: msg, isMe: isMe),
                      ],
                    );
                  },
                );
              },
            ),
          ),
          Container(
            padding: const EdgeInsets.only(left: 12, right: 12, top: 12, bottom: 24),
            color: Colors.transparent,
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.end,
              children: [
                Expanded(
                  child: Container(
                    decoration: BoxDecoration(
                      color: Colors.white,
                      borderRadius: BorderRadius.circular(24),
                      boxShadow: [
                        BoxShadow(
                          color: Colors.black.withOpacity(0.04),
                          blurRadius: 10,
                          offset: const Offset(0, 4),
                        ),
                      ],
                    ),
                    child: TextField(
                      controller: _messageController,
                      decoration: const InputDecoration(
                        hintText: "Napisz wiadomość...",
                        hintStyle: TextStyle(color: Colors.grey),
                        border: InputBorder.none,
                        contentPadding: EdgeInsets.symmetric(horizontal: 20, vertical: 14),
                      ),
                      textCapitalization: TextCapitalization.sentences,
                      minLines: 1,
                      maxLines: 4,
                    ),
                  ),
                ),
                const SizedBox(width: 8),
                GestureDetector(
                  onTap: _sendMessage,
                  child: Container(
                    padding: const EdgeInsets.all(14),
                    decoration: BoxDecoration(
                      gradient: const LinearGradient(
                        colors: [Color(0xFF22c55e), Color(0xFF16a34a)],
                        begin: Alignment.topLeft,
                        end: Alignment.bottomRight,
                      ),
                      shape: BoxShape.circle,
                      boxShadow: [
                        BoxShadow(
                          color: const Color(0xFF16a34a).withOpacity(0.3),
                          blurRadius: 8,
                          offset: const Offset(0, 4),
                        ),
                      ],
                    ),
                    child: const Icon(Icons.send_rounded, color: Colors.white, size: 24),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}