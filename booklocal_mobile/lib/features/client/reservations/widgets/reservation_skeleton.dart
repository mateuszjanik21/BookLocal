import 'package:flutter/material.dart';
import 'package:shimmer/shimmer.dart';

class ReservationSkeleton extends StatelessWidget {
  const ReservationSkeleton({super.key});

  @override
  Widget build(BuildContext context) {
    return Shimmer.fromColors(
      baseColor: Colors.grey.shade300,
      highlightColor: Colors.grey.shade100,
      child: ListView.builder(
        padding: const EdgeInsets.all(16.0),
        itemCount: 5,
        itemBuilder: (context, index) {
          return Container(
            margin: const EdgeInsets.only(bottom: 16.0),
            height: 120,
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(16),
            ),
            child: Row(
              children: [
                Container(
                  width: 100,
                  decoration: BoxDecoration(
                    border: Border(right: BorderSide(color: Colors.grey.shade200)),
                  ),
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Container(width: 40, height: 30, color: Colors.white),
                      const SizedBox(height: 8),
                      Container(width: 60, height: 12, color: Colors.white),
                    ],
                  ),
                ),
                Expanded(
                  child: Padding(
                    padding: const EdgeInsets.all(16.0),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Container(width: 150, height: 16, color: Colors.white),
                        const SizedBox(height: 8),
                        Container(width: 100, height: 12, color: Colors.white),
                        const Spacer(),
                        Align(
                          alignment: Alignment.bottomRight,
                          child: Container(width: 80, height: 32, decoration: BoxDecoration(color: Colors.white, borderRadius: BorderRadius.circular(8))),
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
          );
        },
      ),
    );
  }
}
