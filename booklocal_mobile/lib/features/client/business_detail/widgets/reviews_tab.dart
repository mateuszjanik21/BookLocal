import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../../core/models/review_models.dart';
import '../../../../core/services/auth_service.dart';
import '../../../../core/services/review_service.dart';
import 'package:shimmer/shimmer.dart';
import 'section_card.dart';

class ReviewsTab extends StatelessWidget {
  final int businessId;
  final List<ReviewDto> reviews;
  final bool isLoading;
  final bool isLoadingMore;
  final bool hasMore;
  final String sortBy;
  final ValueChanged<String> onSortChanged;
  final VoidCallback onReviewChanged;

  const ReviewsTab({
    super.key,
    required this.businessId,
    required this.reviews,
    required this.isLoading,
    required this.isLoadingMore,
    required this.hasMore,
    required this.sortBy,
    required this.onSortChanged,
    required this.onReviewChanged,
  });

  @override
  Widget build(BuildContext context) {
    final currentUserId = Provider.of<AuthService>(context).currentUser?.id;

    return SingleChildScrollView(
      physics: const BouncingScrollPhysics(parent: AlwaysScrollableScrollPhysics()),
      padding: const EdgeInsets.fromLTRB(16, 16, 16, 40),
      child: SectionCard(
        title: "Opinie Klientów",
        icon: Icons.star_outline,
        child: AnimatedSwitcher(
          duration: const Duration(milliseconds: 300),
          child: isLoading
              ? _buildSkeletonList()
              : reviews.isEmpty
                  ? const Padding(
                      key: ValueKey('reviews_empty'),
                      padding: EdgeInsets.symmetric(vertical: 20),
                      child: Center(child: Text("Brak opinii. Bądź pierwszy!", style: TextStyle(color: Colors.grey))),
                    )
                  : Column(
                      key: const ValueKey('reviews_loaded'),
                      children: [
                        // Opcje sortowania
                        Padding(
                          padding: const EdgeInsets.only(bottom: 16.0),
                          child: Row(
                            mainAxisAlignment: MainAxisAlignment.end,
                            children: [
                              const Text(
                                "Sortuj:",
                                style: TextStyle(
                                    fontSize: 14,
                                    fontWeight: FontWeight.w500,
                                    color: Colors.grey),
                              ),
                              const SizedBox(width: 8),
                              Container(
                                padding: const EdgeInsets.symmetric(
                                    horizontal: 12, vertical: 4),
                                decoration: BoxDecoration(
                                  color: Colors.grey[100],
                                  borderRadius: BorderRadius.circular(8),
                                ),
                                child: DropdownButton<String>(
                                  value: sortBy,
                                  isDense: true,
                                  icon: const Icon(Icons.keyboard_arrow_down,
                                      size: 20),
                                  underline: const SizedBox(),
                                  style: const TextStyle(
                                      fontSize: 14,
                                      fontWeight: FontWeight.w600,
                                      color: Color(0xFF1F2937)),
                                  onChanged: (value) {
                                    if (value != null) onSortChanged(value);
                                  },
                                  items: const [
                                    DropdownMenuItem(
                                        value: 'newest',
                                        child: Text("Najnowsze")),
                                    DropdownMenuItem(
                                        value: 'highest',
                                        child: Text("Najwyższa ocena")),
                                    DropdownMenuItem(
                                        value: 'lowest',
                                        child: Text("Najniższa ocena")),
                                  ],
                                ),
                              ),
                            ],
                          ),
                        ),
                        ListView.separated(
                          shrinkWrap: true,
                          physics: const NeverScrollableScrollPhysics(),
                          itemCount: reviews.length,
                          separatorBuilder: (context, index) =>
                              const SizedBox(height: 20),
                          itemBuilder: (context, index) =>
                              _ReviewItem(
                                review: reviews[index],
                                businessId: businessId,
                                currentUserId: currentUserId,
                                onChanged: onReviewChanged,
                              ),
                        ),
                        if (isLoadingMore) ...[
                          const SizedBox(height: 20),
                          const Center(
                              child: CircularProgressIndicator(strokeWidth: 2)),
                        ] else if (!hasMore && reviews.isNotEmpty) ...[
                          const SizedBox(height: 30),
                          const Center(
                            child: Text("Brak więcej opinii",
                                style: TextStyle(color: Colors.grey, fontSize: 13)),
                          ),
                        ],
                      ],
                    ),
        ),
      ),
    );
  }

  Widget _buildSkeletonList() {
    return ListView.separated(
      key: const ValueKey('reviews_skeleton'),
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      itemCount: 4,
      separatorBuilder: (context, index) => const SizedBox(height: 20),
      itemBuilder: (context, index) => Shimmer.fromColors(
        baseColor: Colors.grey.shade300,
        highlightColor: Colors.grey.shade100,
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const CircleAvatar(
              radius: 20,
              backgroundColor: Colors.white,
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Container(width: 120, height: 14, color: Colors.white),
                      Container(width: 60, height: 12, color: Colors.white),
                    ],
                  ),
                  const SizedBox(height: 6),
                  Container(width: 80, height: 12, color: Colors.white),
                  const SizedBox(height: 12),
                  Container(
                    width: double.infinity,
                    height: 50,
                    decoration: BoxDecoration(
                      color: Colors.white,
                      borderRadius: BorderRadius.circular(12),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _ReviewItem extends StatelessWidget {
  final ReviewDto review;
  final int businessId;
  final String? currentUserId;
  final VoidCallback onChanged;

  const _ReviewItem({
    required this.review,
    required this.businessId,
    required this.currentUserId,
    required this.onChanged,
  });

  void _showDeleteDialog(BuildContext context) {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text("Usuń opinię"),
        content: const Text("Czy na pewno chcesz usunąć swoją opinię?"),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: const Text("Anuluj", style: TextStyle(color: Colors.grey)),
          ),
          ElevatedButton(
            onPressed: () async {
              Navigator.pop(ctx);
              final success = await Provider.of<ReviewService>(context, listen: false)
                  .deleteReview(businessId, review.reviewId);
              if (!context.mounted) return;
              if (success) {
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text("Opinia została usunięta.")),
                );
                onChanged();
              } else {
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text("Błąd podczas usuwania opinii.")),
                );
              }
            },
            style: ElevatedButton.styleFrom(backgroundColor: Colors.red),
            child: const Text("Usuń", style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );
  }

  void _showEditDialog(BuildContext context) {
    int rating = review.rating;
    final commentController = TextEditingController(text: review.comment);

    showDialog(
      context: context,
      builder: (ctx) {
        return StatefulBuilder(
          builder: (context, setState) {
            return AlertDialog(
              title: const Text("Edytuj opinię"),
              content: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: List.generate(5, (index) {
                      return IconButton(
                        icon: Icon(
                          index < rating ? Icons.star : Icons.star_border,
                          color: Colors.amber,
                        ),
                        onPressed: () => setState(() => rating = index + 1),
                      );
                    }),
                  ),
                  TextField(
                    controller: commentController,
                    maxLines: 3,
                    decoration: const InputDecoration(
                      hintText: "Napisz co myślisz (opcjonalnie)",
                      border: OutlineInputBorder(),
                    ),
                  ),
                ],
              ),
              actions: [
                TextButton(
                  onPressed: () => Navigator.pop(ctx),
                  child: const Text("Anuluj", style: TextStyle(color: Colors.grey)),
                ),
                ElevatedButton(
                  onPressed: () async {
                    Navigator.pop(ctx);
                    final success = await Provider.of<ReviewService>(context, listen: false)
                        .updateReview(businessId, review.reviewId, rating, commentController.text);
                    if (!context.mounted) return;
                    if (success) {
                      ScaffoldMessenger.of(context).showSnackBar(
                        const SnackBar(content: Text("Opinia została zaktualizowana.")),
                      );
                      onChanged();
                    } else {
                      ScaffoldMessenger.of(context).showSnackBar(
                        const SnackBar(content: Text("Błąd podczas edycji opinii.")),
                      );
                    }
                  },
                  style: ElevatedButton.styleFrom(backgroundColor: const Color(0xFF16a34a)),
                  child: const Text("Zapisz", style: TextStyle(color: Colors.white)),
                ),
              ],
            );
          },
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        CircleAvatar(
          radius: 20,
          backgroundColor: Colors.grey[200],
          backgroundImage: review.reviewerPhotoUrl != null ? NetworkImage(review.reviewerPhotoUrl!) : null,
          child: review.reviewerPhotoUrl == null
              ? Text(review.reviewerName.isNotEmpty ? review.reviewerName[0].toUpperCase() : '?',
                  style: const TextStyle(fontSize: 16, color: Colors.grey))
              : null,
        ),
        const SizedBox(width: 12),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Expanded(
                    child: Text(
                      review.reviewerName, 
                      style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
                      overflow: TextOverflow.ellipsis,
                    ),
                  ),
                  Text(
                    "${review.createdAt.day}.${review.createdAt.month.toString().padLeft(2, '0')}.${review.createdAt.year}",
                    style: TextStyle(color: Colors.grey[400], fontSize: 12),
                  ),
                  if (review.userId != null && review.userId == currentUserId)
                    PopupMenuButton<String>(
                      icon: const Icon(Icons.more_horiz, size: 20, color: Colors.grey),
                      onSelected: (value) {
                        if (value == 'edit') {
                          _showEditDialog(context);
                        } else if (value == 'delete') {
                          _showDeleteDialog(context);
                        }
                      },
                      itemBuilder: (BuildContext context) => [
                        const PopupMenuItem(
                          value: 'edit',
                          child: Row(
                            children: [
                              Icon(Icons.edit, size: 18),
                              SizedBox(width: 8),
                              Text("Edytuj"),
                            ],
                          ),
                        ),
                        const PopupMenuItem(
                          value: 'delete',
                          child: Row(
                            children: [
                              Icon(Icons.delete, size: 18, color: Colors.red),
                              SizedBox(width: 8),
                              Text("Usuń", style: TextStyle(color: Colors.red)),
                            ],
                          ),
                        ),
                      ],
                    ),
                ],
              ),
              const SizedBox(height: 4),
              Row(
                children: List.generate(5, (index) {
                  return Icon(
                    index < review.rating ? Icons.star : Icons.star_border,
                    size: 14,
                    color: index < review.rating ? Colors.amber : Colors.grey[300],
                  );
                }),
              ),
              const SizedBox(height: 8),
              if (review.comment.isNotEmpty)
                Container(
                  width: double.infinity,
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    color: Colors.grey[100],
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Text(
                    review.comment,
                    style: TextStyle(color: Colors.grey[800], fontSize: 13, height: 1.4),
                  ),
                ),
            ],
          ),
        ),
      ],
    );
  }
}
