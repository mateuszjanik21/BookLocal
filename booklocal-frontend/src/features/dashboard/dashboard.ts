import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BusinessService } from '../../core/services/business-service';
import { BusinessDetail, Employee, Service } from '../../types/business.model';
import { AssignServicesModalComponent } from '../../shared/components/assign-services-modal/assign-services-modal';
import { AddEmployeeModalComponent } from '../../shared/components/add-employee-modal/add-employee-modal';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    AssignServicesModalComponent,
    AddEmployeeModalComponent
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class DashboardComponent implements OnInit {
  private businessService = inject(BusinessService);
  
  business: BusinessDetail | null = null;
  selectedEmployee: Employee | null = null;

  ngOnInit(): void {
    this.loadBusinessData();
  }

  loadBusinessData(): void {
    this.businessService.getMyBusiness().subscribe({
      next: (data) => {
        this.business = data;
      },
      error: (err: any) => {
        console.error("Błąd podczas pobierania danych firmy:", err);
      }
    });
  }

  openAssignModal(employee: Employee, modal: any): void {
    this.selectedEmployee = employee;
    modal.showModal();
  }
}