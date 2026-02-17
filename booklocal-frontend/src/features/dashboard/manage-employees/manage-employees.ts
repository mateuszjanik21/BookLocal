import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BusinessService } from '../../../core/services/business-service';
import { BusinessDetail, Employee } from '../../../types/business.model';
import { ToastrService } from 'ngx-toastr';
import { AddEmployeeModalComponent } from '../../../shared/components/add-employee-modal/add-employee-modal';
import { EmployeePhotoModalComponent } from '../../../shared/components/employee-photo-modal/employee-photo-modal';

@Component({
  selector: 'app-manage-employees',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    AddEmployeeModalComponent,
    EmployeePhotoModalComponent
  ],
  templateUrl: './manage-employees.html',
})
export class ManageEmployeesComponent implements OnInit {
  private businessService = inject(BusinessService);
  private toastr = inject(ToastrService);

  isLoading = true;
  business: BusinessDetail | null = null;
  employeeForPhoto: Employee | null = null;
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

  onAddModalClosed(saved: boolean) {
    this.isAddEmployeeModalVisible = false;
    if (saved) {
      this.loadBusinessData();
    }
  }

  translateContractType(type: string): string {
    const map: Record<string, string> = {
      'EmploymentContract': 'Umowa o pracę',
      'B2B': 'B2B',
      'MandateContract': 'Umowa zlecenie',
      'Apprenticeship': 'Staż / Praktyka'
    };
    return map[type] || type;
  }
}