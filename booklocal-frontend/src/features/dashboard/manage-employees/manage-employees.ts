import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BusinessService } from '../../../core/services/business-service';
import { EmployeeService } from '../../../core/services/employee-service';
import { BusinessDetail, Employee } from '../../../types/business.model';
import { ToastrService } from 'ngx-toastr';
import { AssignServicesModalComponent } from '../../../shared/components/assign-services-modal/assign-services-modal';
import { AddEmployeeModalComponent } from '../../../shared/components/add-employee-modal/add-employee-modal';
import { EditEmployeeModalComponent } from '../../../shared/components/edit-employee-modal/edit-employee-modal';

@Component({
  selector: 'app-manage-employees',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    AssignServicesModalComponent,
    AddEmployeeModalComponent,
    EditEmployeeModalComponent
  ],
  templateUrl: './manage-employees.html',
})
export class ManageEmployeesComponent implements OnInit {
  private businessService = inject(BusinessService);
  private employeeService = inject(EmployeeService);
  private toastr = inject(ToastrService);

  isLoading = true;
  business: BusinessDetail | null = null;
  employeeToEdit: Employee | null = null;
  employeeToAssignServices: Employee | null = null;
  isAddEmployeeModalVisible = false;

  ngOnInit(): void {
    this.loadBusinessData();
  }

  loadBusinessData(): void {
    this.isLoading = true;
    this.businessService.getMyBusiness().subscribe({
      next: (data) => {
        this.business = data;
        this.isLoading = false;
      },
      error: (err: any) => {
        console.error("Błąd:", err);
        this.isLoading = false;
      }
    });
  }

  onEditEmployee(employee: Employee) {
    this.employeeToEdit = employee;
  }

  onAssignServices(employee: Employee) {
    this.employeeToAssignServices = employee;
  }

  onDeleteEmployee(employee: Employee) {
    if (confirm(`Czy na pewno chcesz usunąć pracownika: ${employee.firstName} ${employee.lastName}?`)) {
      if (this.business) {
        this.employeeService.deleteEmployee(this.business.id, employee.id).subscribe({
          next: () => {
            this.toastr.success('Pracownik został usunięty.');
            this.loadBusinessData();
          },
          error: (err) => this.toastr.error('Wystąpił błąd.')
        });
      }
    }
  }

  closeModalAndRefresh() {
    this.employeeToEdit = null;
    this.employeeToAssignServices = null;
    this.isAddEmployeeModalVisible = false;
    this.loadBusinessData();
  }
}