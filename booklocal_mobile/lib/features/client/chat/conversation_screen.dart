import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../core/services/auth_service.dart';
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

  @override
  void initState() {
    super.initState();
    final authService = Provider.of<AuthService>(context, listen: false);
    _currentUserId = authService.currentUser?.id ?? "";

    WidgetsBinding.instance.addPostFrameCallback((_) async {
      final provider = Provider.of<ChatProvider>(context, listen: false);
      await provider.loadMessageThread(widget.conversationId);
      await provider.startListening(widget.conversationId);
      _scrollToBottom();
      
      provider.addListener(_onMessagesUpdated);
    });
  }

  void _onMessagesUpdated() {
    _scrollToBottom();
  }

  void _scrollToBottom() {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (_scrollController.hasClients) {
        _scrollController.animateTo(
          _scrollController.position.maxScrollExtent,
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
    final provider = Provider.of<ChatProvider>(context, listen: false);
    
    try {
      await provider.sendMessage(widget.conversationId, text, _currentUserId);
      _scrollToBottom();
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text("Błąd wysyłania: $e")));
      }
    }
  }

  @override
  void dispose() {
    final provider = Provider.of<ChatProvider>(context, listen: false);
    provider.stopListening();
    provider.removeListener(_onMessagesUpdated);
    _messageController.dispose();
    _scrollController.dispose();
    super.dispose();
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
                  padding: const EdgeInsets.all(15),
                  itemCount: provider.currentMessages.length,
                  itemBuilder: (context, index) {
                    final msg = provider.currentMessages[index];
                    final isMe = msg.senderId == _currentUserId;
                    return MessageBubble(message: msg, isMe: isMe);
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