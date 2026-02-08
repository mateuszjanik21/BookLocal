import 'package:booklocal_mobile/core/services/chat_services.dart';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../../../core/models/chat_models.dart';
import 'conversation_screen.dart';

class ChatListScreen extends StatefulWidget {
  const ChatListScreen({super.key});

  @override
  State<ChatListScreen> createState() => _ChatListScreenState();
}

class _ChatListScreenState extends State<ChatListScreen> {
  late Future<List<ConversationDto>> _conversationsFuture;

  @override
  void initState() {
    super.initState();
    _conversationsFuture = Provider.of<ChatService>(context, listen: false).getMyConversations();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text("Wiadomości"),
        backgroundColor: Colors.white,
        foregroundColor: Colors.black,
        elevation: 0,
      ),
      body: FutureBuilder<List<ConversationDto>>(
        future: _conversationsFuture,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          } else if (snapshot.hasError) {
            return const Center(child: Text("Błąd pobierania wiadomości"));
          } else if (!snapshot.hasData || snapshot.data!.isEmpty) {
            return const Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(Icons.chat_bubble_outline, size: 60, color: Colors.grey),
                  SizedBox(height: 10),
                  Text("Brak wiadomości"),
                ],
              ),
            );
          }

          final conversations = snapshot.data!;
          return ListView.separated(
            itemCount: conversations.length,
            separatorBuilder: (ctx, i) => const Divider(height: 1),
            itemBuilder: (context, index) {
              final conv = conversations[index];
              return ListTile(
                leading: CircleAvatar(
                  backgroundColor: const Color(0xFF16a34a).withOpacity(0.1),
                  child: Text(conv.participantName[0], style: const TextStyle(color: Color(0xFF16a34a))),
                ),
                title: Text(conv.participantName, style: const TextStyle(fontWeight: FontWeight.bold)),
                subtitle: Text(
                  conv.lastMessage, 
                  maxLines: 1, 
                  overflow: TextOverflow.ellipsis,
                  style: conv.unreadCount > 0 
                      ? const TextStyle(fontWeight: FontWeight.bold, color: Colors.black87)
                      : null,
                ),
                trailing: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Text(
                      DateFormat('HH:mm').format(conv.lastMessageDate),
                      style: const TextStyle(fontSize: 12, color: Colors.grey),
                    ),
                    if (conv.unreadCount > 0)
                      Container(
                        margin: const EdgeInsets.only(top: 5),
                        padding: const EdgeInsets.all(6),
                        decoration: BoxDecoration(color: const Color(0xFF16a34a), shape: BoxShape.circle),
                        child: Text(
                          conv.unreadCount.toString(),
                          style: const TextStyle(color: Colors.white, fontSize: 10),
                        ),
                      )
                  ],
                ),
                onTap: () {
                  Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (context) => ConversationScreen(
                        conversationId: conv.conversationId,
                        participantName: conv.participantName,
                      ),
                    ),
                  );
                },
              );
            },
          );
        },
      ),
    );
  }
}