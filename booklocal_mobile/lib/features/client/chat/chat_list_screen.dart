import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'providers/chat_provider.dart';
import 'widgets/conversation_list_item.dart';

class ChatListScreen extends StatefulWidget {
  const ChatListScreen({super.key});

  @override
  State<ChatListScreen> createState() => _ChatListScreenState();
}

class _ChatListScreenState extends State<ChatListScreen> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      Provider.of<ChatProvider>(context, listen: false).loadMyConversations();
    });
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

          final conversations = provider.conversations;
          if (conversations.isEmpty) {
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

          return ListView.builder(
            padding: const EdgeInsets.only(top: 12, bottom: 24),
            itemCount: conversations.length,
            itemBuilder: (context, index) {
              final conv = conversations[index];
              return ConversationListItem(
                conversation: conv,
                onReturn: () {
                  provider.loadMyConversations();
                },
              );
            },
          );
        },
      ),
    );
  }
}