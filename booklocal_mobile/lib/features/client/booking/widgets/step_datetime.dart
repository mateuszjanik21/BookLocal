import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

class StepDatetime extends StatelessWidget {
  final DateTime selectedDate;
  final String? selectedTime;
  final List<String> availableSlots;
  final Map<String, List<String>> timeGroups;
  final String activeGroup;
  final bool isLoadingSlots;
  final VoidCallback onPickDate;
  final ValueChanged<String> onGroupSelected;
  final ValueChanged<String> onSlotSelected;
  final Color primaryColor;

  const StepDatetime({
    super.key,
    required this.selectedDate,
    required this.selectedTime,
    required this.availableSlots,
    required this.timeGroups,
    required this.activeGroup,
    required this.isLoadingSlots,
    required this.onPickDate,
    required this.onGroupSelected,
    required this.onSlotSelected,
    this.primaryColor = const Color(0xFF16a34a),
  });

  @override
  Widget build(BuildContext context) {
    final dateStr = DateFormat('EEEE, d MMMM', 'pl_PL').format(selectedDate);

    return SingleChildScrollView(
      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 24),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            "Kiedy chcesz nas odwiedzić?",
            style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: Color(0xFF1F2937)),
          ),
          const SizedBox(height: 8),
          Text(
            "Wybierz datę i godzinę wizyty",
            style: TextStyle(fontSize: 14, color: Colors.grey[500]),
          ),
          const SizedBox(height: 24),

          // Date picker card
          _buildDatePicker(dateStr),
          const SizedBox(height: 28),

          // Time slots
          const Text("Dostępne godziny",
              style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Color(0xFF1F2937))),
          const SizedBox(height: 16),
          _buildTimeSlots(),
        ],
      ),
    );
  }

  Widget _buildDatePicker(String dateStr) {
    return InkWell(
      onTap: onPickDate,
      borderRadius: BorderRadius.circular(16),
      child: Container(
        padding: const EdgeInsets.all(18),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(16),
          border: Border.all(color: primaryColor.withOpacity(0.3)),
          boxShadow: [BoxShadow(color: Colors.black.withOpacity(0.03), blurRadius: 10, offset: const Offset(0, 4))],
        ),
        child: Row(
          children: [
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: primaryColor.withOpacity(0.1),
                borderRadius: BorderRadius.circular(12),
              ),
              child: Icon(Icons.calendar_month, color: primaryColor, size: 22),
            ),
            const SizedBox(width: 16),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text("Data wizyty",
                      style: TextStyle(fontSize: 11, color: Colors.grey[500], fontWeight: FontWeight.w600)),
                  const SizedBox(height: 4),
                  Text(
                    _capitalize(dateStr),
                    style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                  ),
                ],
              ),
            ),
            Icon(Icons.arrow_forward_ios, size: 16, color: Colors.grey[400]),
          ],
        ),
      ),
    );
  }

  Widget _buildTimeSlots() {
    if (isLoadingSlots) {
      return Padding(
        padding: const EdgeInsets.all(40),
        child: Center(child: CircularProgressIndicator(color: primaryColor)),
      );
    }

    if (availableSlots.isEmpty) {
      return Container(
        width: double.infinity,
        padding: const EdgeInsets.all(30),
        decoration: BoxDecoration(color: Colors.orange.withOpacity(0.05), borderRadius: BorderRadius.circular(16), border: Border.all(color: Colors.orange.withOpacity(0.2))),
        child: Column(
          children: [
            Icon(Icons.event_busy, size: 40, color: Colors.orange[300]),
            const SizedBox(height: 12),
            const Text("Brak wolnych terminów", style: TextStyle(fontWeight: FontWeight.bold, fontSize: 15)),
            const SizedBox(height: 4),
            Text("Spróbuj wybrać inny dzień.", style: TextStyle(color: Colors.grey[500], fontSize: 13)),
          ],
        ),
      );
    }

    final nonEmptyGroups = timeGroups.entries.where((e) => e.value.isNotEmpty).toList();
    final activeSlots = timeGroups[activeGroup] ?? [];

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        // Group filter chips
        Wrap(
          spacing: 8,
          runSpacing: 8,
          children: nonEmptyGroups.map((entry) {
            final isActive = entry.key == activeGroup;
            return ChoiceChip(
              label: Text(
                "${entry.key} (${entry.value.length})",
                style: TextStyle(
                  fontWeight: FontWeight.bold,
                  fontSize: 13,
                  color: isActive ? Colors.white : Colors.grey[700],
                ),
              ),
              selected: isActive,
              selectedColor: primaryColor,
              backgroundColor: Colors.grey[100],
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
              side: BorderSide.none,
              onSelected: (_) => onGroupSelected(entry.key),
            );
          }).toList(),
        ),
        const SizedBox(height: 16),

        // Slot grid
        GridView.builder(
          shrinkWrap: true,
          physics: const NeverScrollableScrollPhysics(),
          gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
            crossAxisCount: 4,
            childAspectRatio: 2.0,
            crossAxisSpacing: 10,
            mainAxisSpacing: 10,
          ),
          itemCount: activeSlots.length,
          itemBuilder: (context, index) {
            final slot = activeSlots[index];
            final displayTime = slot.contains('T')
                ? DateFormat('HH:mm').format(DateTime.parse(slot))
                : slot;
            final isSelected = selectedTime == displayTime;

            return GestureDetector(
              onTap: () => onSlotSelected(displayTime),
              child: AnimatedContainer(
                duration: const Duration(milliseconds: 200),
                decoration: BoxDecoration(
                  color: isSelected ? primaryColor : Colors.white,
                  borderRadius: BorderRadius.circular(10),
                  border: Border.all(
                    color: isSelected ? primaryColor : Colors.grey.shade300,
                    width: isSelected ? 2 : 1,
                  ),
                  boxShadow: isSelected
                      ? [BoxShadow(color: primaryColor.withOpacity(0.3), blurRadius: 8, offset: const Offset(0, 4))]
                      : [],
                ),
                alignment: Alignment.center,
                child: Text(
                  displayTime,
                  style: TextStyle(
                    color: isSelected ? Colors.white : Colors.black87,
                    fontWeight: FontWeight.bold,
                    fontSize: 14,
                  ),
                ),
              ),
            );
          },
        ),
      ],
    );
  }

  String _capitalize(String s) {
    if (s.isEmpty) return s;
    return s[0].toUpperCase() + s.substring(1);
  }
}
