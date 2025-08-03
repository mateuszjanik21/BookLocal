import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { BusinessService } from '../../core/services/business-service';
import { BusinessDetail, Employee, Service } from '../../types/business.model';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth-service';
import { ReservationModalComponent } from '../../shared/components/reservation-modal/reservation-modal';

@Component({
  selector: 'app-business-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, ReservationModalComponent ],
  templateUrl: './business-detail.html',
  styleUrl: './business-detail.css'
})
export class BusinessDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private businessService = inject(BusinessService);
  filteredEmployees: Employee[] = [];

  business: BusinessDetail | null = null;
  isLoading = true;

  ngOnInit(): void {
    const businessId = this.route.snapshot.paramMap.get('id');

    if (businessId) {
      this.businessService.getBusinessById(+businessId).subscribe({
        next: (data) => {
          this.business = data;
          this.isLoading = false; 
        },
        error: (err) => {
          console.error('Błąd pobierania szczegółów firmy:', err);
          this.isLoading = false; 
        }
      });
    }
  }

  authService = inject(AuthService);

  selectedService: Service | null = null;

  openReservationModal(service: Service, modal: any): void {
    this.selectedService = service;

    if (this.business) {
      this.businessService.getEmployeesForService(this.business.id, service.id).subscribe(employees => {
        this.filteredEmployees = employees;
        modal.showModal(); 
      });
    }
  }
}