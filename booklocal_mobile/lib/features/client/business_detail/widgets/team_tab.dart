import 'package:flutter/material.dart';
import '../../../../core/models/business_detail_dto.dart';
import '../../../../core/models/employee_models.dart';
import 'section_card.dart';

class TeamTab extends StatelessWidget {
  final BusinessDetailDto? fullBusiness;
  final bool isLoading;

  const TeamTab({
    super.key,
    required this.fullBusiness,
    required this.isLoading,
  });

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      padding: const EdgeInsets.fromLTRB(16, 16, 16, 40),
      child: SectionCard(
        title: "Nasz Zespół",
        icon: Icons.group_outlined,
        child: isLoading
            ? const Center(child: CircularProgressIndicator())
            : fullBusiness == null || fullBusiness!.employees.isEmpty
                ? const Center(
                    child: Padding(
                      padding: EdgeInsets.all(20.0),
                      child: Text("Brak przypisanych pracowników.", style: TextStyle(color: Colors.grey)),
                    ),
                  )
                : GridView.builder(
                    shrinkWrap: true,
                    physics: const NeverScrollableScrollPhysics(),
                    gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                      crossAxisCount: 2,
                      crossAxisSpacing: 16,
                      mainAxisSpacing: 16,
                      childAspectRatio: 0.75,
                    ),
                    itemCount: fullBusiness!.employees.length,
                    itemBuilder: (context, index) {
                      return _EmployeeCard(
                        employee: fullBusiness!.employees[index],
                        onTap: () => _showEmployeeDetails(context, fullBusiness!.employees[index]),
                      );
                    },
                  ),
      ),
    );
  }

  void _showEmployeeDetails(BuildContext context, EmployeeDto employee) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (sheetContext) {
        return Container(
          decoration: const BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
          ),
          padding: const EdgeInsets.fromLTRB(24, 12, 24, 24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.center,
            children: [
              Container(
                width: 40,
                height: 4,
                decoration: BoxDecoration(
                  color: Colors.grey[300],
                  borderRadius: BorderRadius.circular(2),
                ),
              ),
              const SizedBox(height: 24),
              CircleAvatar(
                radius: 50,
                backgroundColor: Colors.grey[100],
                backgroundImage: employee.photoUrl != null ? NetworkImage(employee.photoUrl!) : null,
                child: employee.photoUrl == null
                    ? Text(employee.firstName.isNotEmpty ? employee.firstName[0].toUpperCase() : '?',
                        style: const TextStyle(fontSize: 32, color: Colors.grey))
                    : null,
              ),
              const SizedBox(height: 16),
              Text(employee.fullName, style: const TextStyle(fontSize: 24, fontWeight: FontWeight.bold, color: Color(0xFF1F2937))),
              if (employee.position != null || employee.specialization != null)
                Padding(
                  padding: const EdgeInsets.only(top: 4.0),
                  child: Text(employee.position ?? employee.specialization!,
                      style: const TextStyle(fontSize: 16, color: Color(0xFF16a34a), fontWeight: FontWeight.w500)),
                ),
              const SizedBox(height: 24),
              if (employee.bio != null && employee.bio!.isNotEmpty) ...[
                const Align(
                  alignment: Alignment.centerLeft,
                  child: Text("O Mnie", style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16, color: Color(0xFF1F2937))),
                ),
                const SizedBox(height: 8),
                Text(
                  employee.bio!,
                  style: TextStyle(color: Colors.grey[700], height: 1.5, fontSize: 14),
                  textAlign: TextAlign.justify,
                ),
                const SizedBox(height: 24),
              ],
              SizedBox(
                width: double.infinity,
                child: ElevatedButton.icon(
                  onPressed: () {
                    Navigator.pop(sheetContext);
                    DefaultTabController.of(context).animateTo(1);
                  },
                  icon: const Icon(Icons.calendar_today, size: 20),
                  label: const Text("ZOBACZ USŁUGI", style: TextStyle(fontWeight: FontWeight.bold, letterSpacing: 1.1)),
                  style: ElevatedButton.styleFrom(
                    backgroundColor: const Color(0xFF16a34a),
                    foregroundColor: Colors.white,
                    padding: const EdgeInsets.symmetric(vertical: 16),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                    elevation: 0,
                  ),
                ),
              ),
              SafeArea(child: const SizedBox(height: 8)),
            ],
          ),
        );
      },
    );
  }
}

class _EmployeeCard extends StatelessWidget {
  final EmployeeDto employee;
  final VoidCallback onTap;

  const _EmployeeCard({required this.employee, required this.onTap});

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(16),
      child: Container(
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(16),
          border: Border.all(color: Colors.grey.shade200),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withOpacity(0.03),
              blurRadius: 10,
              offset: const Offset(0, 4),
            ),
          ],
        ),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Stack(
              children: [
                Container(
                  decoration: BoxDecoration(
                    shape: BoxShape.circle,
                    border: Border.all(color: const Color(0xFF16a34a).withOpacity(0.2), width: 3),
                  ),
                  child: CircleAvatar(
                    radius: 35,
                    backgroundColor: Colors.grey[100],
                    backgroundImage: employee.photoUrl != null ? NetworkImage(employee.photoUrl!) : null,
                    child: employee.photoUrl == null
                        ? Text(employee.firstName.isNotEmpty ? employee.firstName[0].toUpperCase() : '?',
                            style: const TextStyle(fontSize: 24, color: Colors.grey, fontWeight: FontWeight.bold))
                        : null,
                  ),
                ),
                if (employee.isStudent)
                  Positioned(
                    bottom: -2,
                    right: -2,
                    child: Container(
                      padding: const EdgeInsets.all(4),
                      decoration: BoxDecoration(
                        color: Colors.orangeAccent,
                        shape: BoxShape.circle,
                        border: Border.all(color: Colors.white, width: 2),
                      ),
                      child: const Icon(Icons.school, size: 12, color: Colors.white),
                    ),
                  ),
              ],
            ),
            const SizedBox(height: 12),
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 8.0),
              child: Text(
                employee.fullName,
                style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14, color: Color(0xFF1F2937)),
                textAlign: TextAlign.center,
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
              ),
            ),
            if (employee.position != null || employee.specialization != null)
              Padding(
                padding: const EdgeInsets.only(top: 4.0, left: 8.0, right: 8.0),
                child: Text(
                  employee.position ?? employee.specialization ?? "",
                  style: TextStyle(color: Colors.grey[600], fontSize: 12),
                  textAlign: TextAlign.center,
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                ),
              ),
          ],
        ),
      ),
    );
  }
}
