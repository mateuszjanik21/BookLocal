import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../../../core/models/business_list_item_dto.dart';
import '../../../core/models/employee_models.dart';
import '../../../core/models/service_bundle_dto.dart';
import '../../../core/services/auth_service.dart';
import '../../../core/services/client_service.dart';
import '../../../core/services/reservation_service.dart';
import '../../../core/services/service_bundle_service.dart';
import 'booking_success_screen.dart';
import 'widgets/step_bundle_info.dart';
import 'widgets/step_bundle_final.dart';
import 'widgets/step_bundle_review.dart';
import 'widgets/step_datetime.dart';
import 'widgets/step_employee.dart';

class BundleBookingScreen extends StatefulWidget {
  final BusinessListItemDto business;
  final ServiceBundleDto bundle;

  const BundleBookingScreen({
    super.key,
    required this.business,
    required this.bundle,
  });

  @override
  State<BundleBookingScreen> createState() => _BundleBookingScreenState();
}

class _BundleBookingScreenState extends State<BundleBookingScreen> {
  static const Color _primaryColor = Color(0xFF16a34a);

  int _currentStep = 0;

  List<EmployeeDto> _employees = [];
  bool _isLoadingEmployees = false;
  EmployeeDto? _selectedEmployee;

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
  int _loyaltyPointsBalance = 0;
  int _loyaltyPointsToUse = 0;

  bool _isReserving = false;
  bool _isProcessingPayment = false;

  @override
  void initState() {
    super.initState();
  }


  Future<void> _loadEmployees() async {
    setState(() => _isLoadingEmployees = true);
    final clientService = Provider.of<ClientService>(context, listen: false);
    final emps = await clientService.getEmployees(widget.business.id);
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

    final bundleService = Provider.of<ServiceBundleService>(
      context,
      listen: false,
    );
    final dateStr = DateFormat('yyyy-MM-dd').format(_selectedDate);
    final slots = await bundleService.getBundleAvailableSlots(
      _selectedEmployee!.id,
      dateStr,
      widget.bundle.serviceBundleId,
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
    final resService = Provider.of<ReservationService>(
      context,
      listen: false,
    );
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
      lastDate: DateTime.now().add(const Duration(days: 90)),
      locale: const Locale('pl'),
    );
    if (picked != null && picked != _selectedDate) {
      setState(() {
        _selectedDate = picked;
        _selectedTime = null;
      });
      _loadSlots();
    }
  }

  Future<void> _confirmBooking() async {
    if (_selectedEmployee == null || _selectedTime == null) return;

    if (_paymentMethod == 'Online' && !_isProcessingPayment) {
      setState(() => _isProcessingPayment = true);
      await Future.delayed(const Duration(seconds: 2));
      if (!mounted) return;
      setState(() => _isProcessingPayment = false);
    }

    setState(() => _isReserving = true);

    String timeStr = _selectedTime!;
    if (timeStr.contains('T')) {
      timeStr = timeStr.split('T').last.substring(0, 5);
    }
    final timeParts = timeStr.split(':');
    final fullDate = DateTime(
      _selectedDate.year,
      _selectedDate.month,
      _selectedDate.day,
      int.parse(timeParts[0]),
      int.parse(timeParts[1]),
    );

    bool success = false;
    try {
      final bundleService = Provider.of<ServiceBundleService>(
        context,
        listen: false,
      );
      success = await bundleService.createBundleReservation(
        serviceBundleId: widget.bundle.serviceBundleId,
        employeeId: _selectedEmployee!.id,
        startTime: fullDate,
        paymentMethod: _paymentMethod,
        loyaltyPointsUsed: _loyaltyPointsToUse,
      );
    } catch (_) {}

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
          content: Text(
            "Błąd rezerwacji pakietu. Termin może być już zajęty.",
          ),
          backgroundColor: Colors.red,
        ),
      );
    }
  }

  void _nextStep() {
    if (_currentStep < 4) {
      if (_currentStep == 0) {
        _loadEmployees();
      }
      if (_currentStep == 1) {
        _loadSlots();
      }
      if (_currentStep == 3) {
        _loadLoyaltyBalance();
      }
      setState(() => _currentStep++);
    }
  }

  void _prevStep() {
    if (_currentStep > 0) {
      setState(() => _currentStep--);
    }
  }

  bool get _canAdvance {
    switch (_currentStep) {
      case 0:
        return true;
      case 1:
        return _selectedEmployee != null;
      case 2:
        return _selectedTime != null;
      case 3:
        return true; 
      case 4:
        return true;
      default:
        return false;
    }
  }

  double get _finalPrice {
    final price = widget.bundle.totalPrice - _loyaltyPointsToUse;
    return price < 1 ? 1 : price;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF8F9FA),
      resizeToAvoidBottomInset: true,
      appBar: AppBar(
        title: const Text(
          "Rezerwacja pakietu",
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
                  if (_currentStep > 0) {
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
              if (_currentStep > 0) _buildStepIndicator(),
              _buildBundleInfoBar(),
              Expanded(child: _buildCurrentStep()),
            ],
          ),

          // Bottom bar
          Positioned(
            left: 0,
            right: 0,
            bottom: 0,
            child: _buildBottomBar(),
          ),

          // Overlays
          if (_isProcessingPayment)
            _buildOverlay(
              "Przetwarzanie płatności...",
              "Proszę nie zamykać aplikacji.",
            ),
          if (_isReserving) _buildOverlay("Rezerwuję pakiet...", null),
        ],
      ),
    );
  }

  Widget _buildStepIndicator() {
    final steps = ['Kto', 'Termin', 'Pakiet', 'Finał'];
    final adjustedStep = _currentStep;

    return Container(
      color: Colors.white,
      padding: const EdgeInsets.fromLTRB(20, 12, 20, 16),
      child: Row(
        children: List.generate(steps.length * 2 - 1, (index) {
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
                duration: const Duration(milliseconds: 300),
                width: 32,
                height: 32,
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
                  child: adjustedStep > stepIndex
                      ? const Icon(
                          Icons.check,
                          size: 16,
                          color: Colors.white,
                        )
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
                steps[stepIndex - 1],
                style: TextStyle(
                  fontSize: 10,
                  fontWeight: FontWeight.w700,
                  color: isActive ? _primaryColor : Colors.grey[400],
                ),
              ),
            ],
          );
        }),
      ),
    );
  }

  Widget _buildBundleInfoBar() {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
      decoration: BoxDecoration(
        color: Colors.white,
        border: Border(bottom: BorderSide(color: Colors.grey.shade200)),
      ),
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(8),
            decoration: BoxDecoration(
              color: _primaryColor.withOpacity(0.1),
              borderRadius: BorderRadius.circular(10),
            ),
            child: const Icon(
              Icons.card_giftcard,
              color: _primaryColor,
              size: 18,
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Text(
              "${widget.bundle.name} • ${widget.bundle.totalDurationMinutes} min",
              style: TextStyle(
                fontSize: 13,
                fontWeight: FontWeight.w600,
                color: Colors.grey[700],
              ),
              overflow: TextOverflow.ellipsis,
            ),
          ),
          Text(
            "${widget.bundle.totalPrice.toStringAsFixed(2)} zł",
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
    switch (_currentStep) {
      case 0:
        return StepBundleInfo(
          bundle: widget.bundle,
          onStartBooking: _nextStep,
          primaryColor: _primaryColor,
        );
      case 1:
        if (_isLoadingEmployees) {
          return const Center(
            child: CircularProgressIndicator(color: _primaryColor),
          );
        }
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
          onGroupSelected: (group) {
            setState(() => _activeGroup = group);
          },
          onSlotSelected: (time) {
            setState(() => _selectedTime = time);
          },
        );
      case 3:
        return StepBundleReview(
          bundle: widget.bundle,
          employee: _selectedEmployee!,
          selectedDate: _selectedDate,
          selectedTime: _formatTimeDisplay(_selectedTime!),
          primaryColor: _primaryColor,
        );
      case 4:
        return StepBundleFinal(
          bundle: widget.bundle,
          employee: _selectedEmployee!,
          selectedDate: _selectedDate,
          selectedTime: _formatTimeDisplay(_selectedTime!),
          paymentMethod: _paymentMethod,
          onPaymentMethodChanged: (v) {
            setState(() => _paymentMethod = v);
          },
          loyaltyPointsBalance: _loyaltyPointsBalance,
          loyaltyPointsToUse: _loyaltyPointsToUse,
          onLoyaltyPointsChanged: (v) {
            setState(() => _loyaltyPointsToUse = v);
          },
          primaryColor: _primaryColor,
        );
      default:
        return const SizedBox();
    }
  }

  String _formatTimeDisplay(String slot) {
    if (slot.contains('T')) {
      final dt = DateTime.parse(slot);
      return DateFormat('HH:mm').format(dt);
    }
    return slot;
  }

  Widget _buildBottomBar() {
    return Container(
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
      child: SafeArea(
        top: false,
        child: Padding(
          padding: const EdgeInsets.fromLTRB(20, 14, 20, 12),
          child: Row(
            children: [
              if (_currentStep >= 1) ...[
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
                        if (_loyaltyPointsToUse > 0) ...[
                          Text(
                            "${widget.bundle.totalPrice.toInt()} zł",
                            style: TextStyle(
                              fontSize: 12,
                              color: Colors.grey[400],
                              decoration: TextDecoration.lineThrough,
                            ),
                          ),
                          const SizedBox(width: 4),
                        ],
                        Text(
                          "${_finalPrice.toStringAsFixed(0)} zł",
                          style: const TextStyle(
                            fontSize: 18,
                            fontWeight: FontWeight.w800,
                            color: _primaryColor,
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
                const SizedBox(width: 8),
              ],

              if (_currentStep > 0) ...[
                const Spacer(),
                TextButton(
                  onPressed: _isReserving || _isProcessingPayment
                      ? null
                      : _prevStep,
                  style: TextButton.styleFrom(
                    padding: const EdgeInsets.symmetric(
                      horizontal: 12,
                      vertical: 10,
                    ),
                  ),
                  child: const Text(
                    "Wstecz",
                    style: TextStyle(fontWeight: FontWeight.bold, fontSize: 13),
                  ),
                ),
                const SizedBox(width: 6),
              ],

              // Main action button
              if (_currentStep == 0) const Spacer(),
              Expanded(
                child: ElevatedButton(
                  onPressed: _currentStep == 4
                      ? (_selectedTime != null && !_isReserving
                          ? _confirmBooking
                          : null)
                      : (_canAdvance ? _nextStep : null),
                  style: ElevatedButton.styleFrom(
                    backgroundColor: _primaryColor,
                    foregroundColor: Colors.white,
                    padding: const EdgeInsets.symmetric(
                      vertical: 14,
                      horizontal: 12,
                    ),
                    elevation: 0,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(14),
                    ),
                    disabledBackgroundColor: Colors.grey[300],
                  ),
                  child: FittedBox(
                    fit: BoxFit.scaleDown,
                    child: Text(
                      _currentStep == 0
                          ? "Zarezerwuj pakiet"
                          : _currentStep == 4
                              ? (_paymentMethod == 'Online'
                                  ? "Zapłać i zarezerwuj"
                                  : "Zarezerwuj")
                              : "Dalej",
                      style: const TextStyle(
                        fontSize: 15,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildOverlay(String title, String? subtitle) {
    return Positioned.fill(
      child: Container(
        color: Colors.white.withOpacity(0.92),
        child: Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const CircularProgressIndicator(color: _primaryColor),
              const SizedBox(height: 24),
              Text(
                title,
                style: const TextStyle(
                  fontSize: 18,
                  fontWeight: FontWeight.bold,
                ),
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
      ),
    );
  }
}
