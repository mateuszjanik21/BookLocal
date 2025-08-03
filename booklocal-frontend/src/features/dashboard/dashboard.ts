import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BusinessService } from '../../core/services/business-service';
import { EmployeeService } from '../../core/services/employee-service';
import { BusinessDetail, Employee, Service } from '../../types/business.model';
import { AssignServicesModalComponent } from '../../shared/components/assign-services-modal/assign-services-modal';
import { AddEmployeeModalComponent } from '../../shared/components/add-employee-modal/add-employee-modal';
import { EditEmployeeModalComponent } from '../../shared/components/edit-employee-modal/edit-employee-modal';
import { ToastrService } from 'ngx-toastr';
import { AddServiceModalComponent } from '../../shared/components/add-service-modal/add-service-modal';
import { EditServiceModalComponent } from '../../shared/components/edit-service-modal/edit-service-modal';
import { ServiceService } from '../../core/services/service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    AssignServicesModalComponent,
    AddEmployeeModalComponent,
    EditEmployeeModalComponent,
    AddServiceModalComponent,
    EditServiceModalComponent
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class DashboardComponent implements OnInit {
  private serviceService = inject(ServiceService);
  private businessService = inject(BusinessService);
  private employeeService = inject(EmployeeService);
  private toastr = inject(ToastrService);

  isLoading = true;
  business: BusinessDetail | null = null;
  employeeToEdit: Employee | null = null;
  employeeToAssignServices: Employee | null = null;
  isAddEmployeeModalVisible = false;
  isAddServiceModalVisible = false;
  serviceToEdit: Service | null = null; 

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
        console.error("Błąd podczas pobierania danych firmy:", err);
        this.isLoading = false;
      }
    });
  }

  onEditService(service: Service) {
    this.serviceToEdit = service;
  }

  onDeleteService(service: Service) {
    if (confirm(`Czy na pewno chcesz usunąć usługę: ${service.name}?`)) {
      if (this.business) {
        this.serviceService.deleteService(this.business.id, service.id).subscribe(() => {
          this.toastr.success('Usługa usunięta.');
          this.loadBusinessData();
        });
      }
    }
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
          error: (err) => this.toastr.error('Wystąpił błąd podczas usuwania pracownika.')
        });
      }
    }
  }

  closeModalAndRefresh() {
    this.employeeToEdit = null;
    this.employeeToAssignServices = null;
    this.isAddEmployeeModalVisible = false;
    this.isAddServiceModalVisible = false;
    this.serviceToEdit = null;
    this.loadBusinessData();
  }
}