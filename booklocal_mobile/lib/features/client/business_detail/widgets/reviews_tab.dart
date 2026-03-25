import 'package:flutter/material.dart';
import '../../../../core/models/review_models.dart';
import 'section_card.dart';

class ReviewsTab extends StatelessWidget {
  final List<ReviewDto> reviews;
  final bool isLoading;
  final bool isLoadingMore;
  final bool hasMore;
  final VoidCallback onLoadMore;

  const ReviewsTab({
    super.key,
    required this.reviews,
    required this.isLoading,
    required this.isLoadingMore,
    required this.hasMore,
    required this.onLoadMore,
  });

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      physics: const BouncingScrollPhysics(parent: AlwaysScrollableScrollPhysics()),
      padding: const EdgeInsets.fromLTRB(16, 16, 16, 40),
      child: SectionCard(
        title: "Opinie Klientów",
        icon: Icons.star_outline,
        child: isLoading
            ? const Center(child: CircularProgressIndicator())
            : reviews.isEmpty
                ? const Padding(
                    padding: EdgeInsets.symmetric(vertical: 20),
                    child: Center(child: Text("Brak opinii. Bądź pierwszy!", style: TextStyle(color: Colors.grey))),
                  )
                : Column(
                    children: [
                      ListView.separated(
                        shrinkWrap: true,
                        physics: const NeverScrollableScrollPhysics(),
                        itemCount: reviews.length,
                        separatorBuilder: (context, index) => const SizedBox(height: 20),
                        itemBuilder: (context, index) => _ReviewItem(review: reviews[index]),
                      ),
                      if (hasMore) ...[
                        const SizedBox(height: 20),
                        SizedBox(
                          width: double.infinity,
                          child: OutlinedButton(
                            onPressed: isLoadingMore ? null : onLoadMore,
                            style: OutlinedButton.styleFrom(
                              foregroundColor: const Color(0xFF16a34a),
                              side: const BorderSide(color: Color(0xFF16a34a)),
                              padding: const EdgeInsets.symmetric(vertical: 14),
                              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                            ),
                            child: isLoadingMore
                                ? const SizedBox(height: 20, width: 20, child: CircularProgressIndicator(strokeWidth: 2))
                                : const Text("Ładuj więcej opinii", style: TextStyle(fontWeight: FontWeight.w600)),
                          ),
                        ),
                      ],
                    ],
                  ),
      ),
    );
  }
}

class _ReviewItem extends StatelessWidget {
  final ReviewDto review;

  const _ReviewItem({required this.review});

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
                  Text(review.reviewerName, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14)),
                  Text(
                    "${review.createdAt.day}.${review.createdAt.month}.${review.createdAt.year}",
                    style: TextStyle(color: Colors.grey[400], fontSize: 12),
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
