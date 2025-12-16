import 'package:booklocal_mobile/core/services/chat_services.dart';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../../../core/models/chat_models.dart';
import '../../../core/services/auth_service.dart';

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
  
  List<MessageDto> _messages = [];
  bool _isLoading = true;
  String _currentUserId = "";

  @override
  void initState() {
    super.initState();
    _loadData();
  }

  Future<void> _loadData() async {
    final chatService = Provider.of<ChatService>(context, listen: false);
    final authService = Provider.of<AuthService>(context, listen: false);

    // 1. Pobierz moje ID (żeby wiedzieć, które dymki są moje)
    _currentUserId = authService.currentUser?.id ?? "";

    // 2. Pobierz historię wiadomości (REST API)
    final history = await chatService.getMessageThread(widget.conversationId);

    if (mounted) {
      setState(() {
        _messages = history;
        _isLoading = false;
      });
      _scrollToBottom();
    }

    // 3. Połącz z SignalR (WebSockets)
    await chatService.startHubConnection(widget.conversationId);

    // 4. Nasłuchuj na nowe wiadomości
    chatService.onMessageReceived = (newMessage) {
      if (mounted) {
        setState(() {
          _messages.add(newMessage);
        });
        _scrollToBottom();
      }
    };
  }

  void _scrollToBottom() {
    // Przewiń na sam dół po krótkim opóźnieniu (żeby UI zdążyło się narysować)
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
    final chatService = Provider.of<ChatService>(context, listen: false);
    final authService = Provider.of<AuthService>(context, listen: false);

    // 1. OPTYMISTYCZNE DODANIE WIADOMOŚCI (Żeby użytkownik widział ją od razu)
    final tempMessage = MessageDto(
      id: 0, // Tymczasowe ID
      senderId: authService.currentUser?.id ?? "", // Twoje ID (String)
      content: text,
      messageSent: DateTime.now(),
      isRead: false,
    );

    setState(() {
      _messages.add(tempMessage);
    });
    _scrollToBottom();

    // 2. WYSŁANIE DO SERWERA
    try {
      await chatService.sendMessage(widget.conversationId, text);
      
      // Uwaga: ChatHub odeśle do nas zdarzenie 'ReceiveMessage'.
      // Ponieważ dodaliśmy wiadomość "ręcznie" powyżej, jak przyjdzie z serwera, 
      // będziemy mieć dubla.
      // 
      // Rozwiązanie: W onMessageReceived (w initState) sprawdzaj, czy wiadomość już jest,
      // albo po prostu nie dodawaj optymistycznie, jeśli wolisz pewność. 
      // Ale przy optymistycznym UI, zazwyczaj podmienia się wiadomość tymczasową na tę z serwera.
      
    } catch (e) {
      // Jak błąd -> usuń tymczasową wiadomość i pokaż błąd
      setState(() {
        _messages.remove(tempMessage);
      });
      // ignore: use_build_context_synchronously
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text("Błąd wysyłania: $e")));
    }
  }

  @override
  void dispose() {
    // Rozłącz SignalR przy wyjściu z ekranu
    Provider.of<ChatService>(context, listen: false).stopHubConnection();
    _messageController.dispose();
    _scrollController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.grey[100],
      appBar: AppBar(
        title: Text(widget.participantName),
        backgroundColor: Colors.white,
        foregroundColor: Colors.black,
        elevation: 1,
      ),
      body: Column(
        children: [
          // LISTA WIADOMOŚCI
          Expanded(
            child: _isLoading
                ? const Center(child: CircularProgressIndicator())
                : _messages.isEmpty
                    ? Center(child: Text("Rozpocznij rozmowę z ${widget.participantName}", style: const TextStyle(color: Colors.grey)))
                    : ListView.builder(
                        controller: _scrollController,
                        padding: const EdgeInsets.all(15),
                        itemCount: _messages.length,
                        itemBuilder: (context, index) {
                          final msg = _messages[index];
                          final isMe = msg.senderId == _currentUserId;
                          return _buildMessageBubble(msg, isMe);
                        },
                      ),
          ),

          // PASEK WPROWADZANIA
          Container(
            padding: const EdgeInsets.all(10),
            color: Colors.white,
            child: Row(
              children: [
                Expanded(
                  child: TextField(
                    controller: _messageController,
                    decoration: InputDecoration(
                      hintText: "Napisz wiadomość...",
                      filled: true,
                      fillColor: Colors.grey[200],
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(20),
                        borderSide: BorderSide.none,
                      ),
                      contentPadding: const EdgeInsets.symmetric(horizontal: 15, vertical: 10),
                    ),
                    textCapitalization: TextCapitalization.sentences,
                    minLines: 1,
                    maxLines: 4,
                  ),
                ),
                const SizedBox(width: 10),
                GestureDetector(
                  onTap: _sendMessage,
                  child: Container(
                    padding: const EdgeInsets.all(12),
                    decoration: const BoxDecoration(
                      color: Color(0xFF16a34a),
                      shape: BoxShape.circle,
                    ),
                    child: const Icon(Icons.send, color: Colors.white, size: 20),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildMessageBubble(MessageDto msg, bool isMe) {
    return Align(
      alignment: isMe ? Alignment.centerRight : Alignment.centerLeft,
      child: Container(
        margin: const EdgeInsets.only(bottom: 10),
        padding: const EdgeInsets.symmetric(horizontal: 15, vertical: 10),
        constraints: BoxConstraints(maxWidth: MediaQuery.of(context).size.width * 0.75),
        decoration: BoxDecoration(
          color: isMe ? const Color(0xFF16a34a) : Colors.white,
          borderRadius: BorderRadius.only(
            topLeft: const Radius.circular(15),
            topRight: const Radius.circular(15),
            bottomLeft: isMe ? const Radius.circular(15) : Radius.circular(2),
            bottomRight: isMe ? const Radius.circular(2) : const Radius.circular(15),
          ),
          boxShadow: [
            BoxShadow(color: Colors.black.withOpacity(0.05), blurRadius: 5, offset: const Offset(0, 2)),
          ],
        ),
        child: Column(
          crossAxisAlignment: isMe ? CrossAxisAlignment.end : CrossAxisAlignment.start,
          children: [
            Text(
              msg.content,
              style: TextStyle(
                color: isMe ? Colors.white : Colors.black87,
                fontSize: 15,
              ),
            ),
            const SizedBox(height: 4),
           Text(
              // DODANO: .toLocal() - to naprawi godzinę
              DateFormat('HH:mm').format(msg.messageSent.toLocal()),
              style: TextStyle(
                color: isMe ? Colors.white.withOpacity(0.7) : Colors.grey,
                fontSize: 10,
              ),
            ),
          ],
        ),
      ),
    );
  }
}