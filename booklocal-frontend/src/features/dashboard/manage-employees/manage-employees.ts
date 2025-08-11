import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BusinessService } from '../../../core/services/business-service';
import { EmployeeService } from '../../../core/services/employee-service';
import { BusinessDetail, Employee, Service } from '../../../types/business.model';
import { ToastrService } from 'ngx-toastr';
import { AssignServicesModalComponent } from '../../../shared/components/assign-services-modal/assign-services-modal';
import { AddEmployeeModalComponent } from '../../../shared/components/add-employee-modal/add-employee-modal';
import { EditEmployeeModalComponent } from '../../../shared/components/edit-employee-modal/edit-employee-modal';
import { EmployeePhotoModalComponent } from '../../../shared/components/employee-photo-modal/employee-photo-modal';
import { ScheduleModalComponent } from '../../../shared/components/schedule-modal/schedule-modal';

@Component({
  selector: 'app-manage-employees',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    AssignServicesModalComponent,
    AddEmployeeModalComponent,
    EditEmployeeModalComponent,
    EmployeePhotoModalComponent,
    ScheduleModalComponent
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
  employeeForPhoto: Employee | null = null;
  employeeToAssignServices: Employee | null = null;
  isAddEmployeeModalVisible = false;

  employeeForSchedule: Employee | null = null;

  onManageSchedule(employee: Employee) {
    this.employeeForSchedule = employee;
  }

  closeScheduleModal() {
    this.employeeForSchedule = null;
  }

  get allBusinessServices(): Service[] {
    if (!this.business?.categories) {
      return [];
    }
    return this.business.categories.flatMap(category => category.services);
  }

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

  openPhotoModal(employee: Employee) {
    this.employeeForPhoto = employee;
  }

  closePhotoModalAndRefresh(newPhotoUrl?: string | void) {
    if (typeof newPhotoUrl === 'string' && this.employeeForPhoto) {
      const employeeInList = this.business?.employees.find(e => e.id === this.employeeForPhoto?.id);
      if (employeeInList) {
        employeeInList.photoUrl = newPhotoUrl;
      }
    }
    this.employeeForPhoto = null;
  }

  closeModalAndRefresh() {
    this.employeeToEdit = null;
    this.employeeToAssignServices = null;
    this.isAddEmployeeModalVisible = false;
    this.loadBusinessData();
  }
}