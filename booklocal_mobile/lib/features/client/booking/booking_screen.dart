import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../core/models/business_list_item_dto.dart';
import '../../../core/models/service_models.dart';
import '../../../core/models/employee_models.dart';
import '../../../core/services/auth_service.dart';
import '../../../core/services/client_service.dart';
import '../../../core/services/reservation_service.dart';
import 'widgets/step_employee.dart';
import 'widgets/step_datetime.dart';
import 'widgets/step_summary.dart';
import 'booking_success_screen.dart';

class BookingScreen extends StatefulWidget {
  final BusinessListItemDto business;
  final ServiceDto service;
  final int originalServiceId;
  final int serviceVariantId;
  final EmployeeDto? preselectedEmployee;

  const BookingScreen({
    super.key,
    required this.business,
    required this.service,
    required this.originalServiceId,
    required this.serviceVariantId,
    this.preselectedEmployee,
  });

  @override
  State<BookingScreen> createState() => _BookingScreenState();
}

class _BookingScreenState extends State<BookingScreen> {
  static const _primaryColor = Color(0xFF16a34a);

  int _currentStep = 1;

  List<EmployeeDto> _employees = [];
  EmployeeDto? _selectedEmployee;
  bool _isLoadingEmployees = true;

  DateTime _selectedDate = DateTime.now();
  String? _selectedTime;
  List<String> _availableSlots = [];
  Map<String, List<String>> _timeGroups = {
    'Rano': [],
    'Południe': [],
    'Popołudnie': [],
    'Wieczór': [],
  };
  String _activeGroup = 'Rano';
  bool _isLoadingSlots = false;

  String _paymentMethod = 'Cash';
  final TextEditingController _discountController = TextEditingController();
  Map<String, dynamic>? _verifiedDiscount;
  bool _isVerifyingDiscount = false;
  int _loyaltyPointsBalance = 0;
  int _loyaltyPointsToUse = 0;

  bool _isReserving = false;
  bool _isProcessingPayment = false;


  @override
  void initState() {
    super.initState();
    if (widget.preselectedEmployee != null) {
      _selectedEmployee = widget.preselectedEmployee;
      _employees = [widget.preselectedEmployee!];
      _isLoadingEmployees = false;
      _currentStep = 2;
      _loadSlots();
    } else {
      _loadEmployees();
    }
    _loadLoyaltyBalance();
  }

  @override
  void dispose() {
    _discountController.dispose();
    super.dispose();
  }

  Future<void> _loadEmployees() async {
    final clientService = Provider.of<ClientService>(context, listen: false);
    final emps = await clientService.getEmployeesForService(
      widget.business.id,
      widget.originalServiceId,
    );
    if (!mounted) return;
    setState(() {
      _employees = emps;
      _isLoadingEmployees = false;
    });
  }

  Future<void> _loadSlots() async {
    if (_selectedEmployee == null) return;
    setState(() {
      _isLoadingSlots = true;
      _selectedTime = null;
    });

    final resService = Provider.of<ReservationService>(context, listen: false);
    final slots = await resService.getAvailableSlots(
      _selectedEmployee!.id,
      widget.serviceVariantId,
      _selectedDate,
    );
    if (!mounted) return;
    setState(() {
      _availableSlots = slots;
      _groupSlots(slots);
      _isLoadingSlots = false;
    });
  }

  void _groupSlots(List<String> slots) {
    final groups = <String, List<String>>{
      'Rano': [],
      'Południe': [],
      'Popołudnie': [],
      'Wieczór': [],
    };
    for (final slot in slots) {
      int hour;
      if (slot.contains('T')) {
        hour = DateTime.parse(slot).hour;
      } else if (slot.contains(':')) {
        hour = int.parse(slot.split(':')[0]);
      } else {
        continue;
      }
      if (hour >= 6 && hour < 11) {
        groups['Rano']!.add(slot);
      } else if (hour >= 11 && hour < 15) {
        groups['Południe']!.add(slot);
      } else if (hour >= 15 && hour < 18) {
        groups['Popołudnie']!.add(slot);
      } else if (hour >= 18) {
        groups['Wieczór']!.add(slot);
      }
    }
    _timeGroups = groups;
    if (_timeGroups[_activeGroup]?.isEmpty ?? true) {
      _activeGroup = _timeGroups.entries
          .firstWhere(
            (e) => e.value.isNotEmpty,
            orElse: () => _timeGroups.entries.first,
          )
          .key;
    }
  }

  Future<void> _loadLoyaltyBalance() async {
    final authService = Provider.of<AuthService>(context, listen: false);
    final user = authService.currentUser;
    if (user == null) return;
    final resService = Provider.of<ReservationService>(context, listen: false);
    final balance = await resService.getLoyaltyBalance(
      businessId: widget.business.id,
      customerId: user.id,
    );
    if (mounted) setState(() => _loyaltyPointsBalance = balance);
  }

  Future<void> _pickDate() async {
    final picked = await showDatePicker(
      context: context,
      initialDate: _selectedDate,
      firstDate: DateTime.now(),
      lastDate: DateTime.now().add(const Duration(days: 30)),
      locale: const Locale('pl'),
      builder: (context, child) => Theme(
        data: Theme.of(context).copyWith(
          colorScheme: const ColorScheme.light(primary: _primaryColor),
        ),
        child: child!,
      ),
    );
    if (picked != null && picked != _selectedDate) {
      setState(() => _selectedDate = picked);
      _loadSlots();
    }
  }

  Future<void> _verifyDiscount() async {
    final code = _discountController.text.trim();
    if (code.isEmpty) return;
    setState(() => _isVerifyingDiscount = true);

    final authService = Provider.of<AuthService>(context, listen: false);
    final resService = Provider.of<ReservationService>(context, listen: false);
    final result = await resService.verifyDiscount(
      businessId: widget.business.id,
      code: code,
      serviceId: widget.originalServiceId,
      customerId: authService.currentUser?.id ?? '',
      originalPrice: widget.service.price,
    );
    if (!mounted) return;
    setState(() => _isVerifyingDiscount = false);

    if (result != null && result['isValid'] == true) {
      setState(() => _verifiedDiscount = result);
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(result?['message'] ?? 'Kod nieprawidłowy.'),
          backgroundColor: Colors.red,
        ),
      );
    }
  }

  void _removeDiscount() {
    setState(() {
      _verifiedDiscount = null;
      _discountController.clear();
    });
  }

  double get _discountAmount =>
      (_verifiedDiscount?['discountAmount'] as num?)?.toDouble() ?? 0.0;

  double get _finalPrice {
    final price = widget.service.price - _discountAmount - _loyaltyPointsToUse;
    return price < 0 ? 0 : price;
  }

  Future<void> _confirmBooking() async {
    if (_selectedTime == null || _selectedEmployee == null) return;

    if (_paymentMethod == 'Online' && _finalPrice > 0) {
      setState(() => _isProcessingPayment = true);
      await Future.delayed(const Duration(seconds: 2));
      if (!mounted) return;
      setState(() => _isProcessingPayment = false);
    }

    setState(() => _isReserving = true);

    final timeParts = _selectedTime!.split(':');
    final fullDate = DateTime(
      _selectedDate.year,
      _selectedDate.month,
      _selectedDate.day,
      int.parse(timeParts[0]),
      int.parse(timeParts[1]),
    );

    bool success = false;
    try {
      final resService = Provider.of<ReservationService>(
        context,
        listen: false,
      );
      success = await resService.createReservation(
        widget.serviceVariantId,
        _selectedEmployee!.id,
        fullDate,
        discountCode: _verifiedDiscount != null
            ? _discountController.text.trim()
            : null,
        paymentMethod: _paymentMethod,
        loyaltyPointsUsed: _loyaltyPointsToUse,
      );
    } catch (e) {
      debugPrint("Błąd rezerwacji: $e");
    }

    if (!mounted) return;
    setState(() => _isReserving = false);

    if (success) {
      Navigator.of(context, rootNavigator: true).pushAndRemoveUntil(
        MaterialPageRoute(builder: (_) => const BookingSuccessScreen()),
        (route) => false,
      );
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text("Błąd rezerwacji. Termin może być już zajęty."),
          backgroundColor: Colors.red,
        ),
      );
    }
  }

  void _nextStep() {
    if (_currentStep < 3) {
      if (_currentStep == 1) {
        _loadSlots();
      }
      setState(() => _currentStep++);
    }
  }

  void _prevStep() {
    if (_currentStep > 1) {
      setState(() => _currentStep--);
    }
  }

  bool get _canAdvance {
    switch (_currentStep) {
      case 1:
        return _selectedEmployee != null;
      case 2:
        return _selectedTime != null;
      case 3:
        return true;
      default:
        return false;
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF8F9FA),
      resizeToAvoidBottomInset: true,
      appBar: AppBar(
        title: const Text(
          "Rezerwacja",
          style: TextStyle(fontWeight: FontWeight.bold),
        ),
        centerTitle: true,
        backgroundColor: Colors.white,
        foregroundColor: Colors.black,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back_ios_new, size: 20),
          onPressed: _isReserving || _isProcessingPayment
              ? null
              : () {
                  if (_currentStep > 1) {
                    _prevStep();
                  } else {
                    Navigator.pop(context);
                  }
                },
        ),
      ),
      body: Stack(
        children: [
          Column(
            children: [
              _buildStepIndicator(),
              _buildServiceInfoBar(),
              Expanded(child: _buildCurrentStep()),
            ],
          ),

          if (!_isLoadingEmployees)
            Positioned(
              left: 0,
              right: 0,
              bottom: 0,
              child: _buildBottomBar(),
            ),

          if (_isProcessingPayment)
            _buildOverlay(
              "Przetwarzanie płatności...",
              "Proszę nie zamykać aplikacji.",
            ),
          if (_isReserving) _buildOverlay("Rezerwuję wizytę...", null),
        ],
      ),
    );
  }

  Widget _buildStepIndicator() {
    final steps = ['Kto', 'Termin', 'Finał'];
    final displaySteps = steps;
    final adjustedStep = _currentStep;

    return Container(
      color: Colors.white,
      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
      child: Row(
        children: List.generate(displaySteps.length * 2 - 1, (index) {
          if (index.isOdd) {
            final stepBefore = (index ~/ 2) + 1;
            return Expanded(
              child: Container(
                height: 2,
                color: adjustedStep > stepBefore
                    ? _primaryColor
                    : Colors.grey[300],
              ),
            );
          }
          final stepIndex = (index ~/ 2) + 1;
          final isActive = adjustedStep >= stepIndex;
          final isCurrent = adjustedStep == stepIndex;

          return Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              AnimatedContainer(
                duration: const Duration(milliseconds: 200),
                width: isCurrent ? 36 : 28,
                height: isCurrent ? 36 : 28,
                decoration: BoxDecoration(
                  color: isActive ? _primaryColor : Colors.grey[200],
                  shape: BoxShape.circle,
                  boxShadow: isCurrent
                      ? [
                          BoxShadow(
                            color: _primaryColor.withOpacity(0.3),
                            blurRadius: 8,
                            offset: const Offset(0, 3),
                          ),
                        ]
                      : [],
                ),
                child: Center(
                  child: isActive && !isCurrent
                      ? const Icon(Icons.check, size: 16, color: Colors.white)
                      : Text(
                          "$stepIndex",
                          style: TextStyle(
                            color: isActive ? Colors.white : Colors.grey[500],
                            fontWeight: FontWeight.bold,
                            fontSize: 13,
                          ),
                        ),
                ),
              ),
              const SizedBox(height: 6),
              Text(
                displaySteps[stepIndex - 1],
                style: TextStyle(
                  fontSize: 11,
                  fontWeight: isCurrent ? FontWeight.bold : FontWeight.w500,
                  color: isActive ? _primaryColor : Colors.grey[500],
                ),
              ),
            ],
          );
        }),
      ),
    );
  }

  Widget _buildServiceInfoBar() {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 10),
      color: _primaryColor.withOpacity(0.05),
      child: Row(
        children: [
          Icon(Icons.spa, size: 18, color: _primaryColor),
          const SizedBox(width: 10),
          Expanded(
            child: Text(
              "${widget.service.name} • ${widget.service.durationMinutes} min",
              style: TextStyle(
                fontSize: 13,
                fontWeight: FontWeight.w600,
                color: Colors.grey[700],
              ),
              overflow: TextOverflow.ellipsis,
            ),
          ),
          Text(
            "${widget.service.price.toStringAsFixed(2)} zł",
            style: const TextStyle(
              fontWeight: FontWeight.bold,
              fontSize: 14,
              color: _primaryColor,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildCurrentStep() {
    if (_isLoadingEmployees) {
      return const Center(
        child: CircularProgressIndicator(color: _primaryColor),
      );
    }

    switch (_currentStep) {
      case 1:
        return StepEmployee(
          employees: _employees,
          selectedEmployee: _selectedEmployee,
          primaryColor: _primaryColor,
          onSelected: (emp) {
            setState(() => _selectedEmployee = emp);
          },
        );
      case 2:
        return StepDatetime(
          selectedDate: _selectedDate,
          selectedTime: _selectedTime,
          availableSlots: _availableSlots,
          timeGroups: _timeGroups,
          activeGroup: _activeGroup,
          isLoadingSlots: _isLoadingSlots,
          primaryColor: _primaryColor,
          onPickDate: _pickDate,
          onGroupSelected: (group) => setState(() => _activeGroup = group),
          onSlotSelected: (time) {
            setState(() => _selectedTime = time);
          },
        );
      case 3:
        return StepSummary(
          service: widget.service,
          employee: _selectedEmployee!,
          selectedDate: _selectedDate,
          selectedTime: _selectedTime!,
          businessName: widget.business.name,
          paymentMethod: _paymentMethod,
          onPaymentMethodChanged: (v) => setState(() => _paymentMethod = v),
          discountController: _discountController,
          verifiedDiscount: _verifiedDiscount,
          isVerifyingDiscount: _isVerifyingDiscount,
          onVerifyDiscount: _verifyDiscount,
          onRemoveDiscount: _removeDiscount,
          loyaltyPointsBalance: _loyaltyPointsBalance,
          loyaltyPointsToUse: _loyaltyPointsToUse,
          onLoyaltyPointsChanged: (v) =>
              setState(() => _loyaltyPointsToUse = v),
          primaryColor: _primaryColor,
        );
      default:
        return const SizedBox.shrink();
    }
  }

  Widget _buildBottomBar() {
    final hasDiscount = _discountAmount > 0 || _loyaltyPointsToUse > 0;

    return Container(
      padding: const EdgeInsets.fromLTRB(20, 16, 20, 28),
      decoration: BoxDecoration(
        color: Colors.white,
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 10,
            offset: const Offset(0, -5),
          ),
        ],
        borderRadius: const BorderRadius.vertical(top: Radius.circular(20)),
      ),
      child: Row(
        children: [
          if (_currentStep == 3) ...[
            Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text(
                  "Koszt",
                  style: TextStyle(color: Colors.grey, fontSize: 11),
                ),
                Row(
                  crossAxisAlignment: CrossAxisAlignment.end,
                  children: [
                    if (hasDiscount) ...[
                      Text(
                        "${widget.service.price.toInt()} zł",
                        style: TextStyle(
                          fontSize: 13,
                          color: Colors.grey[400],
                          decoration: TextDecoration.lineThrough,
                        ),
                      ),
                      const SizedBox(width: 6),
                    ],
                    Text(
                      "${_finalPrice.toStringAsFixed(2)} zł",
                      style: const TextStyle(
                        fontSize: 20,
                        fontWeight: FontWeight.w800,
                        color: _primaryColor,
                      ),
                    ),
                  ],
                ),
              ],
            ),
            const SizedBox(width: 16),
          ],

          if (_currentStep > 1) ...[
            TextButton(
              onPressed: _isReserving || _isProcessingPayment
                  ? null
                  : _prevStep,
              style: TextButton.styleFrom(
                foregroundColor: Colors.grey[700],
                padding: const EdgeInsets.symmetric(
                  horizontal: 16,
                  vertical: 12,
                ),
              ),
              child: const Text(
                "Wstecz",
                style: TextStyle(fontWeight: FontWeight.bold),
              ),
            ),
            const SizedBox(width: 8),
          ],

          Expanded(
            child: ElevatedButton(
              onPressed: _currentStep == 3
                  ? (_selectedTime != null && !_isReserving
                        ? _confirmBooking
                        : null)
                  : (_canAdvance ? _nextStep : null),
              style: ElevatedButton.styleFrom(
                backgroundColor: _primaryColor,
                foregroundColor: Colors.white,
                padding: const EdgeInsets.symmetric(vertical: 16),
                elevation: 0,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(14),
                ),
                disabledBackgroundColor: Colors.grey[300],
              ),
              child: Text(
                _currentStep == 3
                    ? (_paymentMethod == 'Online'
                          ? "Zapłać i zarezerwuj"
                          : "Potwierdź wizytę")
                    : "Dalej",
                style: const TextStyle(
                  fontSize: 15,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildOverlay(String title, String? subtitle) {
    return Container(
      color: Colors.white.withOpacity(0.95),
      child: Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const CircularProgressIndicator(color: _primaryColor),
            const SizedBox(height: 24),
            Text(
              title,
              style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
            if (subtitle != null) ...[
              const SizedBox(height: 8),
              Text(
                subtitle,
                style: TextStyle(fontSize: 14, color: Colors.grey[500]),
              ),
            ],
          ],
        ),
      ),
    );
  }
}
