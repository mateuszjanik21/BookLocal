import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'dart:math';
import '../../../../core/models/employee_models.dart';
import '../../../../core/models/service_bundle_dto.dart';

class StepBundleFinal extends StatelessWidget {
  final ServiceBundleDto bundle;
  final EmployeeDto employee;
  final DateTime selectedDate;
  final String selectedTime;

  final String paymentMethod;
  final ValueChanged<String> onPaymentMethodChanged;

  final int loyaltyPointsBalance;
  final int loyaltyPointsToUse;
  final ValueChanged<int> onLoyaltyPointsChanged;

  final Color primaryColor;

  const StepBundleFinal({
    super.key,
    required this.bundle,
    required this.employee,
    required this.selectedDate,
    required this.selectedTime,
    required this.paymentMethod,
    required this.onPaymentMethodChanged,
    required this.loyaltyPointsBalance,
    required this.loyaltyPointsToUse,
    required this.onLoyaltyPointsChanged,
    this.primaryColor = const Color(0xFF16a34a),
  });

  int get _maxLoyaltyPoints {
    final basePrice = bundle.totalPrice;
    return min(
      loyaltyPointsBalance,
      max((basePrice - 1).floor(), 0),
    );
  }

  @override
  Widget build(BuildContext context) {
    final dateStr = DateFormat('dd.MM.yyyy', 'pl_PL').format(selectedDate);

    return SingleChildScrollView(
      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 24),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            "Potwierdzenie rezerwacji",
            style: TextStyle(
              fontSize: 20,
              fontWeight: FontWeight.bold,
              color: Color(0xFF1F2937),
            ),
          ),
          const SizedBox(height: 20),

          Container(
            padding: const EdgeInsets.all(20),
            decoration: BoxDecoration(
              color: Colors.grey[50],
              borderRadius: BorderRadius.circular(20),
              border: Border.all(color: Colors.grey.shade200),
            ),
            child: Column(
              children: [
                Row(
                  children: [
                    CircleAvatar(
                      radius: 22,
                      backgroundColor: Colors.grey[200],
                      backgroundImage: employee.photoUrl != null
                          ? NetworkImage(employee.photoUrl!)
                          : null,
                      child: employee.photoUrl == null
                          ? Text(
                              employee.firstName[0],
                              style: const TextStyle(
                                fontWeight: FontWeight.bold,
                                color: Colors.grey,
                              ),
                            )
                          : null,
                    ),
                    const SizedBox(width: 14),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            bundle.name,
                            style: const TextStyle(
                              fontWeight: FontWeight.bold,
                              fontSize: 15,
                            ),
                          ),
                          const SizedBox(height: 2),
                          Text(
                            "${employee.firstName} ${employee.lastName} • Pakiet ${bundle.items.length} usług",
                            style: TextStyle(
                              fontSize: 11,
                              color: Colors.grey[500],
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
                const Divider(height: 24),
                Row(
                  children: [
                    Container(
                      padding: const EdgeInsets.all(10),
                      decoration: BoxDecoration(
                        color: primaryColor.withOpacity(0.1),
                        borderRadius: BorderRadius.circular(10),
                      ),
                      child: Icon(
                        Icons.calendar_month,
                        color: primaryColor,
                        size: 20,
                      ),
                    ),
                    const SizedBox(width: 14),
                    Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          "Start",
                          style: TextStyle(
                            fontSize: 10,
                            color: Colors.grey[500],
                            fontWeight: FontWeight.w700,
                            letterSpacing: 1,
                          ),
                        ),
                        const SizedBox(height: 2),
                        Text(
                          "$dateStr • $selectedTime",
                          style: TextStyle(
                            fontWeight: FontWeight.bold,
                            fontSize: 14,
                            color: primaryColor,
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ],
            ),
          ),
          const SizedBox(height: 20),

          Container(
            padding: const EdgeInsets.all(20),
            decoration: BoxDecoration(
              color: Colors.grey[50],
              borderRadius: BorderRadius.circular(20),
              border: Border.all(color: Colors.grey.shade200),
            ),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  "Do zapłaty za pakiet:",
                  style: TextStyle(
                    fontSize: 13,
                    fontWeight: FontWeight.bold,
                    color: Colors.grey[600],
                  ),
                ),
                Text(
                  "${_currentPrice.toStringAsFixed(2)} zł",
                  style: TextStyle(
                    fontSize: 26,
                    fontWeight: FontWeight.w900,
                    color: primaryColor,
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(height: 20),

          if (loyaltyPointsBalance > 0) ...[
            _buildLoyaltySection(),
            const SizedBox(height: 20),
          ],

          const Text(
            "Forma płatności",
            style: TextStyle(
              fontSize: 16,
              fontWeight: FontWeight.bold,
              color: Color(0xFF1F2937),
            ),
          ),
          const SizedBox(height: 12),
          _buildPaymentCard(
            value: 'Cash',
            icon: Icons.payments_outlined,
            iconColor: primaryColor,
            title: 'Płatność w salonie',
            subtitle: 'Gotówka lub Karta',
          ),
          const SizedBox(height: 10),
          _buildPaymentCard(
            value: 'Online',
            icon: Icons.bolt,
            iconColor: const Color(0xFF3B82F6),
            title: 'Płatność online',
            subtitle: 'Blik, Przelewy24, Apple Pay',
            badge: 'FAST',
          ),

          const SizedBox(height: 140),
        ],
      ),
    );
  }

  double get _currentPrice {
    final price = bundle.totalPrice - loyaltyPointsToUse;
    return price < 1 ? 1 : price;
  }

  Widget _buildPaymentCard({
    required String value,
    required IconData icon,
    required Color iconColor,
    required String title,
    required String subtitle,
    String? badge,
  }) {
    final isSelected = paymentMethod == value;
    return GestureDetector(
      onTap: () => onPaymentMethodChanged(value),
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 200),
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(16),
          border: Border.all(
            color: isSelected ? primaryColor : Colors.grey.shade200,
            width: isSelected ? 2 : 1,
          ),
          boxShadow: isSelected
              ? [
                  BoxShadow(
                    color: primaryColor.withOpacity(0.15),
                    blurRadius: 12,
                    offset: const Offset(0, 4),
                  ),
                ]
              : [],
        ),
        child: Row(
          children: [
            Container(
              width: 46,
              height: 46,
              decoration: BoxDecoration(
                color: iconColor.withOpacity(0.1),
                borderRadius: BorderRadius.circular(13),
              ),
              child: Icon(icon, color: iconColor, size: 22),
            ),
            const SizedBox(width: 14),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      Text(
                        title,
                        style: const TextStyle(
                          fontWeight: FontWeight.bold,
                          fontSize: 14,
                        ),
                      ),
                      if (badge != null) ...[
                        const SizedBox(width: 8),
                        Container(
                          padding: const EdgeInsets.symmetric(
                            horizontal: 7,
                            vertical: 2,
                          ),
                          decoration: BoxDecoration(
                            color: primaryColor,
                            borderRadius: BorderRadius.circular(6),
                          ),
                          child: Text(
                            badge,
                            style: const TextStyle(
                              color: Colors.white,
                              fontSize: 9,
                              fontWeight: FontWeight.w800,
                            ),
                          ),
                        ),
                      ],
                    ],
                  ),
                  const SizedBox(height: 2),
                  Text(
                    subtitle,
                    style: TextStyle(
                      fontSize: 11,
                      color: Colors.grey[500],
                      fontWeight: FontWeight.w500,
                    ),
                  ),
                ],
              ),
            ),
            Container(
              width: 22,
              height: 22,
              decoration: BoxDecoration(
                shape: BoxShape.circle,
                border: Border.all(
                  color: isSelected ? primaryColor : Colors.grey.shade400,
                  width: 2,
                ),
              ),
              child: isSelected
                  ? Center(
                      child: Container(
                        width: 10,
                        height: 10,
                        decoration: BoxDecoration(
                          shape: BoxShape.circle,
                          color: primaryColor,
                        ),
                      ),
                    )
                  : null,
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildLoyaltySection() {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: const Color(0xFFFFFBEB),
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: Colors.amber.withOpacity(0.3)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              const Icon(Icons.star, color: Colors.amber, size: 22),
              const SizedBox(width: 8),
              const Text(
                "Punkty lojalnościowe",
                style: TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
              ),
              const Spacer(),
              Container(
                padding: const EdgeInsets.symmetric(
                  horizontal: 10,
                  vertical: 4,
                ),
                decoration: BoxDecoration(
                  color: Colors.amber.withOpacity(0.2),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: Text(
                  "$loyaltyPointsBalance pkt",
                  style: const TextStyle(
                    fontWeight: FontWeight.w800,
                    fontSize: 12,
                    color: Colors.amber,
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),
          Row(
            children: [
              Expanded(
                child: SliderTheme(
                  data: SliderThemeData(
                    activeTrackColor: Colors.amber,
                    inactiveTrackColor: Colors.amber.withOpacity(0.2),
                    thumbColor: Colors.amber,
                    overlayColor: Colors.amber.withOpacity(0.1),
                  ),
                  child: Slider(
                    value: loyaltyPointsToUse.toDouble(),
                    min: 0,
                    max: _maxLoyaltyPoints > 0
                        ? _maxLoyaltyPoints.toDouble()
                        : 1,
                    divisions: _maxLoyaltyPoints > 0 ? _maxLoyaltyPoints : 1,
                    onChanged: (val) => onLoyaltyPointsChanged(val.round()),
                  ),
                ),
              ),
              const SizedBox(width: 8),
              Container(
                width: 48,
                alignment: Alignment.center,
                padding: const EdgeInsets.symmetric(vertical: 6),
                decoration: BoxDecoration(
                  border: Border.all(color: Colors.amber.withOpacity(0.5)),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: Text(
                  "$loyaltyPointsToUse",
                  style: const TextStyle(
                    fontWeight: FontWeight.bold,
                    fontSize: 14,
                  ),
                ),
              ),
              const SizedBox(width: 4),
              Text(
                "pkt",
                style: TextStyle(fontSize: 12, color: Colors.grey[500]),
              ),
            ],
          ),
          if (loyaltyPointsToUse > 0)
            Padding(
              padding: const EdgeInsets.only(top: 8),
              child: Text(
                "Zniżka: -$loyaltyPointsToUse,00 PLN • Do zapłaty: ${_currentPrice.toStringAsFixed(2)} PLN",
                style: const TextStyle(
                  fontSize: 12,
                  fontWeight: FontWeight.w600,
                  color: Colors.amber,
                ),
              ),
            ),
        ],
      ),
    );
  }
}
