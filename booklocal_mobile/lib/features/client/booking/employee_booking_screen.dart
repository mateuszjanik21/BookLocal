import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../../core/models/business_detail_dto.dart';
import '../../../core/models/business_list_item_dto.dart';
import '../../../core/models/employee_models.dart';
import '../../../core/models/service.dart';
import '../../../core/models/service_models.dart';
import '../../../core/models/service_variant.dart';
import '../../../core/services/client_service.dart';
import 'booking_screen.dart';

/// Screen that shows services for a pre-selected employee.
/// User picks a service → navigates to BookingScreen with employee pre-set.
class EmployeeBookingScreen extends StatefulWidget {
  final BusinessListItemDto business;
  final EmployeeDto employee;

  const EmployeeBookingScreen({
    super.key,
    required this.business,
    required this.employee,
  });

  @override
  State<EmployeeBookingScreen> createState() => _EmployeeBookingScreenState();
}

class _EmployeeBookingScreenState extends State<EmployeeBookingScreen> {
  static const Color _primary = Color(0xFF16a34a);

  BusinessDetailDto? _fullBusiness;
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadServices();
  }

  Future<void> _loadServices() async {
    final clientService = Provider.of<ClientService>(context, listen: false);
    final details = await clientService.getBusinessById(widget.business.id);
    if (!mounted) return;
    setState(() {
      _fullBusiness = details;
      _isLoading = false;
    });
  }

  void _onServiceSelected(Service service, ServiceVariant variant) {
    final dummyDto = ServiceDto(
      id: service.id,
      name: variant.name.toLowerCase() == "domyślny" ||
              variant.name.toLowerCase() == "default"
          ? service.name
          : "${service.name} - ${variant.name}",
      description: service.description ?? "",
      price: variant.price,
      durationMinutes: variant.durationMinutes,
    );

    Navigator.pushReplacement(
      context,
      MaterialPageRoute(
        builder: (_) => BookingScreen(
          business: widget.business,
          service: dummyDto,
          originalServiceId: service.id,
          serviceVariantId: variant.serviceVariantId,
          preselectedEmployee: widget.employee,
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF8F9FA),
      appBar: AppBar(
        title: Text(
          "Rezerwacja u ${widget.employee.firstName}",
          style: const TextStyle(fontWeight: FontWeight.bold),
        ),
        centerTitle: true,
        backgroundColor: Colors.white,
        foregroundColor: Colors.black,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back_ios_new, size: 20),
          onPressed: () => Navigator.pop(context),
        ),
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator(color: _primary))
          : _buildContent(),
    );
  }

  Widget _buildContent() {
    if (_fullBusiness == null || _fullBusiness!.categories.isEmpty) {
      return const Center(
        child: Text(
          "Brak dostępnych usług.",
          style: TextStyle(color: Colors.grey, fontSize: 16),
        ),
      );
    }

    return Column(
      children: [
        // Employee info card
        _buildEmployeeHeader(),

        // Instruction
        Padding(
          padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
          child: Row(
            children: [
              Icon(Icons.touch_app, size: 18, color: Colors.grey[400]),
              const SizedBox(width: 8),
              Text(
                "Wybierz usługę",
                style: TextStyle(
                  fontSize: 14,
                  fontWeight: FontWeight.w700,
                  color: Colors.grey[700],
                ),
              ),
            ],
          ),
        ),

        // Services list
        Expanded(
          child: ListView.builder(
            physics: const BouncingScrollPhysics(),
            padding: const EdgeInsets.fromLTRB(16, 0, 16, 40),
            itemCount: _fullBusiness!.categories.length,
            itemBuilder: (context, catIndex) {
              final category = _fullBusiness!.categories[catIndex];
              if (category.services.isEmpty) return const SizedBox.shrink();
              return Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Padding(
                    padding: const EdgeInsets.only(top: 12.0, bottom: 8.0),
                    child: Text(
                      category.name,
                      style: const TextStyle(
                        fontWeight: FontWeight.bold,
                        fontSize: 16,
                        color: _primary,
                      ),
                    ),
                  ),
                  ...category.services
                      .map((service) => _buildServiceCard(service)),
                  const SizedBox(height: 8),
                ],
              );
            },
          ),
        ),
      ],
    );
  }

  Widget _buildEmployeeHeader() {
    return Container(
      margin: const EdgeInsets.fromLTRB(16, 16, 16, 0),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.04),
            blurRadius: 10,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Row(
        children: [
          CircleAvatar(
            radius: 28,
            backgroundColor: Colors.grey[100],
            backgroundImage: widget.employee.photoUrl != null
                ? NetworkImage(widget.employee.photoUrl!)
                : null,
            child: widget.employee.photoUrl == null
                ? Text(
                    widget.employee.firstName.isNotEmpty
                        ? widget.employee.firstName[0].toUpperCase()
                        : '?',
                    style: const TextStyle(
                      fontSize: 22,
                      color: Colors.grey,
                      fontWeight: FontWeight.bold,
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
                  widget.employee.fullName,
                  style: const TextStyle(
                    fontWeight: FontWeight.bold,
                    fontSize: 16,
                    color: Color(0xFF1F2937),
                  ),
                ),
                if (widget.employee.position != null)
                  Padding(
                    padding: const EdgeInsets.only(top: 2),
                    child: Text(
                      widget.employee.position!,
                      style: TextStyle(
                        color: Colors.grey[600],
                        fontSize: 13,
                      ),
                    ),
                  ),
              ],
            ),
          ),
          Container(
            padding: const EdgeInsets.all(8),
            decoration: BoxDecoration(
              color: _primary.withOpacity(0.1),
              borderRadius: BorderRadius.circular(10),
            ),
            child: const Icon(Icons.person, color: _primary, size: 20),
          ),
        ],
      ),
    );
  }

  Widget _buildServiceCard(Service service) {
    if (service.variants.isEmpty) return const SizedBox.shrink();

    return Container(
      margin: const EdgeInsets.only(bottom: 8),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: Colors.grey.shade200),
      ),
      child: Theme(
        data: Theme.of(context).copyWith(dividerColor: Colors.transparent),
        child: ExpansionTile(
          key: PageStorageKey<String>('emp_service_${service.id}'),
          title: Text(
            service.name,
            style: const TextStyle(
              fontWeight: FontWeight.w600,
              fontSize: 15,
              color: Color(0xFF1F2937),
            ),
          ),
          subtitle: service.description != null &&
                  service.description!.trim().isNotEmpty
              ? Text(
                  service.description!,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: TextStyle(color: Colors.grey[500], fontSize: 12),
                )
              : null,
          childrenPadding: EdgeInsets.zero,
          tilePadding: const EdgeInsets.symmetric(horizontal: 16),
          children: service.variants
              .map((variant) => _buildVariantRow(service, variant))
              .toList(),
        ),
      ),
    );
  }

  Widget _buildVariantRow(Service service, ServiceVariant variant) {
    return InkWell(
      onTap: () => _onServiceSelected(service, variant),
      child: Padding(
        padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 16),
        child: Row(
          children: [
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    variant.name.toLowerCase() == "domyślny" ||
                            variant.name.toLowerCase() == "default"
                        ? service.name
                        : variant.name,
                    style: const TextStyle(
                      fontSize: 14,
                      fontWeight: FontWeight.w500,
                      color: Color(0xFF374151),
                    ),
                  ),
                  const SizedBox(height: 2),
                  Text(
                    "${variant.durationMinutes} min",
                    style: TextStyle(
                      fontSize: 12,
                      color: Colors.grey[400],
                    ),
                  ),
                ],
              ),
            ),
            Text(
              "${variant.price.toStringAsFixed(0)} zł",
              style: const TextStyle(
                fontWeight: FontWeight.bold,
                fontSize: 15,
                color: _primary,
              ),
            ),
            const SizedBox(width: 8),
            Icon(Icons.arrow_forward_ios, size: 14, color: Colors.grey[400]),
          ],
        ),
      ),
    );
  }
}
