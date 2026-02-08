import 'package:booklocal_mobile/features/client/booking/booking_success_screen.dart';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../../../core/models/business_list_item_dto.dart';
import '../../../core/models/service_models.dart';
import '../../../core/models/employee_models.dart';
import '../../../core/services/client_service.dart';
import '../../../core/services/reservation_service.dart';

class BookingScreen extends StatefulWidget {
  final BusinessListItemDto business;
  final ServiceDto service;

  const BookingScreen({
    super.key,
    required this.business,
    required this.service,
  });

  @override
  State<BookingScreen> createState() => _BookingScreenState();
}

class _BookingScreenState extends State<BookingScreen> {
  DateTime _selectedDate = DateTime.now();
  String? _selectedTime;
  EmployeeDto? _selectedEmployee;

  List<EmployeeDto> _employees = [];
  List<String> _availableSlots = [];
  bool _isLoadingEmployees = true;
  bool _isLoadingSlots = false;

  final primaryColor = const Color(0xFF16a34a);

  @override
  void initState() {
    super.initState();
    _loadEmployees();
  }

  Future<void> _loadEmployees() async {
    final clientService = Provider.of<ClientService>(context, listen: false);
    final emps = await clientService.getEmployeesForService(
      widget.business.id,
      widget.service.id,
    );

    if (mounted) {
      setState(() {
        _employees = emps;
        _isLoadingEmployees = false;
        if (emps.isNotEmpty) {
          _selectedEmployee = emps.first;
          _loadSlots();
        }
      });
    }
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
      widget.service.id,
      _selectedDate,
    );

    if (mounted) {
      setState(() {
        _availableSlots = slots;
        _isLoadingSlots = false;
      });
    }
  }

  Future<void> _pickDate() async {
    final DateTime? picked = await showDatePicker(
      context: context,
      initialDate: _selectedDate,
      firstDate: DateTime.now(),
      lastDate: DateTime.now().add(const Duration(days: 30)),
      builder: (context, child) => Theme(
        data: Theme.of(context).copyWith(
          colorScheme: ColorScheme.light(primary: primaryColor),
          dialogBackgroundColor: Colors.white,
        ),
        child: child!,
      ),
    );
    if (picked != null && picked != _selectedDate) {
      setState(() => _selectedDate = picked);
      _loadSlots();
    }
  }

  Future<void> _confirmBooking() async {
    if (_selectedTime == null || _selectedEmployee == null) return;

    final timeParts = _selectedTime!.split(':');
    final hour = int.parse(timeParts[0]);
    final minute = int.parse(timeParts[1]);

    final fullDate = DateTime(
      _selectedDate.year,
      _selectedDate.month,
      _selectedDate.day,
      hour,
      minute,
    );

    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (context) => const Center(child: CircularProgressIndicator(color: Colors.white)),
    );

    final resService = Provider.of<ReservationService>(context, listen: false);
    final success = await resService.createReservation(
      widget.service.id,
      _selectedEmployee!.id,
      fullDate,
    );

    if (!mounted) return;
    Navigator.pop(context);

    if (success) {
      Navigator.of(context).pushAndRemoveUntil(
        MaterialPageRoute(
          builder: (context) => const BookingSuccessScreen(),
        ),
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

  @override
  Widget build(BuildContext context) {
    final dateStr = DateFormat('EEEE, d MMMM', 'pl_PL').format(_selectedDate); 

    return Scaffold(
      backgroundColor: const Color(0xFFF8F9FA),
      appBar: AppBar(
        title: const Text("Rezerwacja", style: TextStyle(fontWeight: FontWeight.bold)),
        centerTitle: true,
        backgroundColor: Colors.white,
        foregroundColor: Colors.black,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back_ios_new, size: 20),
          onPressed: () => Navigator.pop(context),
        ),
      ),
      body: _isLoadingEmployees
          ? Center(child: CircularProgressIndicator(color: primaryColor))
          : SingleChildScrollView(
              padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 24),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  _buildServiceSummaryCard(),
                  
                  const SizedBox(height: 30),

                  const Text(
                    "Wybierz specjalistę",
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Color(0xFF1F2937)),
                  ),
                  const SizedBox(height: 16),
                  SizedBox(
                    height: 110,
                    child: ListView.separated(
                      scrollDirection: Axis.horizontal,
                      itemCount: _employees.length,
                      separatorBuilder: (ctx, i) => const SizedBox(width: 16),
                      itemBuilder: (context, index) {
                        final emp = _employees[index];
                        final isSelected = _selectedEmployee?.id == emp.id;
                        return _buildEmployeeItem(emp, isSelected);
                      },
                    ),
                  ),

                  const SizedBox(height: 30),

                  const Text(
                    "Wybierz termin",
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Color(0xFF1F2937)),
                  ),
                  const SizedBox(height: 16),
                  InkWell(
                    onTap: _pickDate,
                    borderRadius: BorderRadius.circular(12),
                    child: Container(
                      padding: const EdgeInsets.all(16),
                      decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(color: Colors.grey.shade300),
                      ),
                      child: Row(
                        children: [
                          Container(
                            padding: const EdgeInsets.all(10),
                            decoration: BoxDecoration(
                              color: primaryColor.withOpacity(0.1),
                              borderRadius: BorderRadius.circular(8),
                            ),
                            child: Icon(Icons.calendar_month, color: primaryColor),
                          ),
                          const SizedBox(width: 16),
                          Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                "Data wizyty",
                                style: TextStyle(fontSize: 12, color: Colors.grey[500]),
                              ),
                              const SizedBox(height: 4),
                              Text(
                                toBeginningOfSentenceCase(dateStr) ?? dateStr,
                                style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                              ),
                            ],
                          ),
                          const Spacer(),
                          const Icon(Icons.arrow_forward_ios, size: 16, color: Colors.grey),
                        ],
                      ),
                    ),
                  ),

                  const SizedBox(height: 30),

                  const Text(
                    "Dostępne godziny",
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Color(0xFF1F2937)),
                  ),
                  const SizedBox(height: 16),
                  
                  _isLoadingSlots
                      ? Padding(
                          padding: const EdgeInsets.all(40),
                          child: Center(child: CircularProgressIndicator(color: primaryColor)),
                        )
                      : _availableSlots.isEmpty
                          ? Container(
                              width: double.infinity,
                              padding: const EdgeInsets.all(30),
                              decoration: BoxDecoration(
                                color: Colors.grey[100],
                                borderRadius: BorderRadius.circular(12),
                              ),
                              child: Column(
                                children: [
                                  Icon(Icons.event_busy, size: 40, color: Colors.grey[400]),
                                  const SizedBox(height: 10),
                                  Text("Brak wolnych terminów w tym dniu.", style: TextStyle(color: Colors.grey[600])),
                                ],
                              ),
                            )
                          : GridView.builder(
                              shrinkWrap: true,
                              physics: const NeverScrollableScrollPhysics(),
                              gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                                crossAxisCount: 4,
                                childAspectRatio: 2.0,
                                crossAxisSpacing: 10,
                                mainAxisSpacing: 10,
                              ),
                              itemCount: _availableSlots.length,
                              itemBuilder: (context, index) {
                                final slot = _availableSlots[index];
                                final displayTime = slot.contains('T')
                                    ? DateFormat('HH:mm').format(DateTime.parse(slot))
                                    : slot;
                                final isSelected = _selectedTime == displayTime;

                                return GestureDetector(
                                  onTap: () => setState(() => _selectedTime = displayTime),
                                  child: AnimatedContainer(
                                    duration: const Duration(milliseconds: 200),
                                    decoration: BoxDecoration(
                                      color: isSelected ? primaryColor : Colors.white,
                                      borderRadius: BorderRadius.circular(8),
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
                  const SizedBox(height: 100),
                ],
              ),
            ),
      
      bottomSheet: Container(
        padding: const EdgeInsets.fromLTRB(20, 20, 20, 30),
        decoration: BoxDecoration(
          color: Colors.white,
          boxShadow: [
            BoxShadow(color: Colors.black.withOpacity(0.05), blurRadius: 10, offset: const Offset(0, -5)),
          ],
          borderRadius: const BorderRadius.vertical(top: Radius.circular(20)),
        ),
        child: Row(
          children: [
            Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text("Całkowity koszt", style: TextStyle(color: Colors.grey, fontSize: 12)),
                Text(
                  "${widget.service.price.toInt()} zł",
                  style: TextStyle(fontSize: 22, fontWeight: FontWeight.w800, color: primaryColor),
                ),
              ],
            ),
            const SizedBox(width: 20),
            Expanded(
              child: ElevatedButton(
                onPressed: (_selectedTime != null && _selectedEmployee != null)
                    ? _confirmBooking
                    : null,
                style: ElevatedButton.styleFrom(
                  backgroundColor: primaryColor,
                  foregroundColor: Colors.white,
                  padding: const EdgeInsets.symmetric(vertical: 16),
                  elevation: 0,
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                  disabledBackgroundColor: Colors.grey[300],
                ),
                child: const Text(
                  "Zarezerwuj",
                  style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }


  Widget _buildServiceSummaryCard() {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(color: Colors.black.withOpacity(0.04), blurRadius: 15, offset: const Offset(0, 5)),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: primaryColor.withOpacity(0.1),
                  shape: BoxShape.circle,
                ),
                child: Icon(Icons.spa, color: primaryColor),
              ),
              const SizedBox(width: 15),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      widget.business.name,
                      style: TextStyle(fontSize: 13, color: Colors.grey[600], fontWeight: FontWeight.w500),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      widget.service.name,
                      style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Color(0xFF1F2937)),
                    ),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: 20),
          const Divider(),
          const SizedBox(height: 10),
          Row(
            children: [
              Icon(Icons.schedule, size: 16, color: Colors.grey[500]),
              const SizedBox(width: 6),
              Text(
                "Czas trwania: ${widget.service.durationMinutes} min",
                style: TextStyle(color: Colors.grey[700], fontWeight: FontWeight.w500),
              ),
            ],
          )
        ],
      ),
    );
  }

  Widget _buildEmployeeItem(EmployeeDto emp, bool isSelected) {
    return GestureDetector(
      onTap: () {
        setState(() => _selectedEmployee = emp);
        _loadSlots();
      },
      child: Column(
        children: [
          Stack(
            children: [
              Container(
                padding: const EdgeInsets.all(3),
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  border: Border.all(
                    color: isSelected ? primaryColor : Colors.transparent,
                    width: 2,
                  ),
                ),
                child: CircleAvatar(
                  radius: 32, 
                  backgroundColor: Colors.grey[200],
                  backgroundImage: emp.photoUrl != null ? NetworkImage(emp.photoUrl!) : null,
                  child: emp.photoUrl == null
                      ? Text(emp.firstName[0], style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: Colors.grey))
                      : null,
                ),
              ),
              if (isSelected)
                Positioned(
                  right: 0,
                  bottom: 0,
                  child: Container(
                    padding: const EdgeInsets.all(4),
                    decoration: BoxDecoration(
                      color: primaryColor,
                      shape: BoxShape.circle,
                      border: Border.all(color: Colors.white, width: 2),
                    ),
                    child: const Icon(Icons.check, size: 12, color: Colors.white),
                  ),
                ),
            ],
          ),
          const SizedBox(height: 8),
          Text(
            emp.firstName,
            style: TextStyle(
              fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
              fontSize: 12,
              color: isSelected ? Colors.black : Colors.grey[700],
            ),
          ),
        ],
      ),
    );
  }
}