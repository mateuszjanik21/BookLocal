import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../../../core/models/employee_models.dart';
import '../../../../core/models/service_bundle_dto.dart';

class StepBundleReview extends StatelessWidget {
  final ServiceBundleDto bundle;
  final EmployeeDto employee;
  final DateTime selectedDate;
  final String selectedTime;
  final Color primaryColor;

  const StepBundleReview({
    super.key,
    required this.bundle,
    required this.employee,
    required this.selectedDate,
    required this.selectedTime,
    this.primaryColor = const Color(0xFF16a34a),
  });

  @override
  Widget build(BuildContext context) {
    final dateStr = DateFormat('dd.MM', 'pl_PL').format(selectedDate);

    return SingleChildScrollView(
      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 24),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            "Przegląd pakietu",
            style: TextStyle(
              fontSize: 20,
              fontWeight: FontWeight.bold,
              color: Color(0xFF1F2937),
            ),
          ),
          const SizedBox(height: 20),

          Row(
            children: [
              Expanded(child: _infoChip("Specjalista", employee.firstName)),
              const SizedBox(width: 12),
              Expanded(child: _infoChip("Start", "$dateStr $selectedTime")),
            ],
          ),
          const SizedBox(height: 24),

          Text(
            "Usługi w pakiecie",
            style: TextStyle(
              fontSize: 11,
              fontWeight: FontWeight.w900,
              color: Colors.grey[400],
              letterSpacing: 1.2,
            ),
          ),
          const SizedBox(height: 16),
          _buildTimeline(),

          const SizedBox(height: 80),
        ],
      ),
    );
  }

  Widget _infoChip(String label, String value) {
    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: Colors.grey[50],
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: Colors.grey.shade200),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            label.toUpperCase(),
            style: TextStyle(
              fontSize: 10,
              fontWeight: FontWeight.w900,
              color: Colors.grey[400],
              letterSpacing: 1,
            ),
          ),
          const SizedBox(height: 4),
          Text(
            value,
            style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
          ),
        ],
      ),
    );
  }

  Widget _buildTimeline() {
    return Column(
      children: bundle.items.asMap().entries.map((entry) {
        final index = entry.key;
        final item = entry.value;
        final isLast = index == bundle.items.length - 1;

        return IntrinsicHeight(
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              SizedBox(
                width: 32,
                child: Column(
                  children: [
                    Container(
                      width: 28,
                      height: 28,
                      decoration: BoxDecoration(
                        color: primaryColor,
                        shape: BoxShape.circle,
                      ),
                      child: const Icon(
                        Icons.check,
                        size: 14,
                        color: Colors.white,
                      ),
                    ),
                    if (!isLast)
                      Expanded(
                        child: Container(
                          width: 2,
                          color: primaryColor.withOpacity(0.2),
                        ),
                      ),
                  ],
                ),
              ),
              const SizedBox(width: 14),
              Expanded(
                child: Container(
                  margin: EdgeInsets.only(bottom: isLast ? 0 : 16),
                  padding: const EdgeInsets.all(14),
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(14),
                    border: Border.all(color: Colors.grey.shade100),
                    boxShadow: [
                      BoxShadow(
                        color: Colors.black.withOpacity(0.02),
                        blurRadius: 8,
                        offset: const Offset(0, 2),
                      ),
                    ],
                  ),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        item.serviceName,
                        style: const TextStyle(
                          fontWeight: FontWeight.bold,
                          fontSize: 13,
                        ),
                      ),
                      const SizedBox(height: 2),
                      Text(
                        "${item.variantName} • ${item.durationMinutes} min",
                        style: TextStyle(
                          fontSize: 11,
                          color: Colors.grey[500],
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ],
          ),
        );
      }).toList(),
    );
  }
}
