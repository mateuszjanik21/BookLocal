import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BusinessService } from '../../../core/services/business-service';
import { ServiceService } from '../../../core/services/service';
import { BusinessDetail, Service } from '../../../types/business.model';
import { ToastrService } from 'ngx-toastr';
import { AddServiceModalComponent } from '../../../shared/components/add-service-modal/add-service-modal';
import { EditServiceModalComponent } from '../../../shared/components/edit-service-modal/edit-service-modal';

@Component({
  selector: 'app-manage-services',
  standalone: true,
  imports: [CommonModule, AddServiceModalComponent, EditServiceModalComponent],
  templateUrl: './manage-services.html',
})
export class ManageServicesComponent implements OnInit {
  private businessService = inject(BusinessService);
  private serviceService = inject(ServiceService);
  private toastr = inject(ToastrService);

  isLoading = true;
  business: BusinessDetail | null = null;
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
        this.serviceService.deleteService(this.business.id, service.id).subscribe({
          next: () => {
            this.toastr.success('Usługa usunięta.');
            this.loadBusinessData();
          },
          error: (err) => this.toastr.error('Wystąpił błąd podczas usuwania usługi.')
        });
      }
    }
  }

  closeModalAndRefresh() {
    this.isAddServiceModalVisible = false;
    this.serviceToEdit = null;
    this.loadBusinessData();
  }
}