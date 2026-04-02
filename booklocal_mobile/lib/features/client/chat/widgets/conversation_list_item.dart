import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../../../core/models/chat_models.dart';
import '../conversation_screen.dart';

class ConversationListItem extends StatelessWidget {
  final ConversationDto conversation;
  final VoidCallback onReturn;

  const ConversationListItem({
    super.key,
    required this.conversation,
    required this.onReturn,
  });

  String _formatMessageTime(DateTime? dateStr) {
    if (dateStr == null) return '';
    final date = dateStr.toLocal();
    final now = DateTime.now();
    
    final difference = DateTime(now.year, now.month, now.day).difference(DateTime(date.year, date.month, date.day)).inDays;
    final timeStr = DateFormat('HH:mm').format(date);

    if (difference == 0) {
      return timeStr;
    } else if (difference == 1) {
      return 'Wczoraj $timeStr';
    } else if (difference < 7) {
      String day = '';
      switch (date.weekday) {
        case 1: day = 'Pon'; break;
        case 2: day = 'Wt'; break;
        case 3: day = 'Śr'; break;
        case 4: day = 'Czw'; break;
        case 5: day = 'Pt'; break;
        case 6: day = 'Sob'; break;
        case 7: day = 'Ndz'; break;
      }
      return '$day $timeStr';
    } else {
      final dateFormated = DateFormat('d MMM', 'pl_PL').format(date).replaceAll('.', '');
      return '$dateFormated $timeStr';
    }
  }

  LinearGradient _getAvatarGradient(String name) {
    if (name.isEmpty) {
      return const LinearGradient(colors: [Color(0xFFe2e8f0), Color(0xFFcbd5e1)]);
    }
    
    final List<List<Color>> gradients = [
      [const Color(0xFFff9a9e), const Color(0xFFfecfef)], 
      [const Color(0xFFa18cd1), const Color(0xFFfbc2eb)], 
      [const Color(0xFF84fab0), const Color(0xFF8fd3f4)], 
      [const Color(0xFFfccb90), const Color(0xFFd57eeb)], 
      [const Color(0xFFe0c3fc), const Color(0xFF8ec5fc)], 
      [const Color(0xFFf093fb), const Color(0xFFf5576c)], 
      [const Color(0xFF4facfe), const Color(0xFF00f2fe)], 
      [const Color(0xFF43e97b), const Color(0xFF38f9d7)], 
      [const Color(0xFFfa709a), const Color(0xFFfee140)], 
      [const Color(0xFFc2e9fb), const Color(0xFFa1c4fd)], 
    ];
    
    int sum = 0;
    for (int i = 0; i < name.length; i++) {
        sum += name.codeUnitAt(i);
    }
    
    final colors = gradients[sum % gradients.length];
    return LinearGradient(
      begin: Alignment.topLeft,
      end: Alignment.bottomRight,
      colors: colors,
    );
  }

  @override
  Widget build(BuildContext context) {
    final bool hasUnread = conversation.unreadCount > 0;
    
    return Container(
      margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 6),
      decoration: BoxDecoration(
        color: hasUnread ? Colors.white : Colors.white.withOpacity(0.95),
        borderRadius: BorderRadius.circular(16),
        border: hasUnread ? Border.all(color: const Color(0xFF16a34a).withOpacity(0.3), width: 1.5) : Border.all(color: Colors.transparent, width: 1.5),
        boxShadow: [
          BoxShadow(
            color: hasUnread ? const Color(0xFF16a34a).withOpacity(0.08) : Colors.black.withOpacity(0.03),
            blurRadius: 10,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          borderRadius: BorderRadius.circular(16),
          onTap: () {
            Navigator.push(
              context,
              MaterialPageRoute(
                builder: (context) => ConversationScreen(
                  conversationId: conversation.conversationId,
                  participantName: conversation.participantName,
                ),
              ),
            ).then((_) => onReturn());
          },
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Row(
              children: [
                // Avatar Premium
                Container(
                  width: 54,
                  height: 54,
                  decoration: BoxDecoration(
                    shape: BoxShape.circle,
                    gradient: conversation.participantPhotoUrl == null ? _getAvatarGradient(conversation.participantName) : null,
                    image: conversation.participantPhotoUrl != null 
                      ? DecorationImage(
                          image: NetworkImage(conversation.participantPhotoUrl!),
                          fit: BoxFit.cover,
                        )
                      : null,
                    boxShadow: [
                      BoxShadow(
                        color: Colors.black.withOpacity(0.1),
                        blurRadius: 4,
                        offset: const Offset(0, 2),
                      ),
                    ],
                  ),
                  child: conversation.participantPhotoUrl == null
                    ? Center(
                        child: Text(
                          conversation.participantName.isNotEmpty ? conversation.participantName[0].toUpperCase() : '?',
                          style: const TextStyle(
                            color: Colors.white,
                            fontSize: 22,
                            fontWeight: FontWeight.w900,
                            shadows: [Shadow(color: Colors.black12, blurRadius: 4, offset: Offset(0, 2))],
                          ),
                        ),
                      )
                    : null,
                ),
                const SizedBox(width: 16),
                
                // Informacje
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Expanded(
                            child: Text(
                              conversation.participantName,
                              style: TextStyle(
                                fontWeight: hasUnread ? FontWeight.w900 : FontWeight.w700,
                                fontSize: 16,
                                color: const Color(0xFF1F2937),
                              ),
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                            ),
                          ),
                          const SizedBox(width: 8),
                          Text(
                            _formatMessageTime(conversation.lastMessageDate),
                            style: TextStyle(
                              fontSize: 12,
                              color: hasUnread ? const Color(0xFF16a34a) : Colors.grey[500],
                              fontWeight: hasUnread ? FontWeight.w800 : FontWeight.w600,
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 4),
                      Row(
                        children: [
                          Expanded(
                            child: Text(
                              conversation.lastMessage.isEmpty ? 'Brak wiadomości' : conversation.lastMessage,
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                              style: TextStyle(
                                fontSize: 14,
                                color: hasUnread ? Colors.black87 : Colors.grey[600],
                                fontWeight: hasUnread ? FontWeight.w700 : FontWeight.normal,
                              ),
                            ),
                          ),
                          if (hasUnread)
                            Container(
                              margin: const EdgeInsets.only(left: 10),
                              padding: const EdgeInsets.symmetric(horizontal: 7, vertical: 3),
                              decoration: BoxDecoration(
                                color: const Color(0xFF16a34a),
                                borderRadius: BorderRadius.circular(12),
                              ),
                              child: Text(
                                conversation.unreadCount > 99 ? '99+' : conversation.unreadCount.toString(),
                                style: const TextStyle(
                                  color: Colors.white,
                                  fontSize: 11,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                            )
                        ],
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
