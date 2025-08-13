import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BusinessService } from '../../../core/services/business-service';
import { CategoryService } from '../../../core/services/category';
import { BusinessDetail, ServiceCategory, Service } from '../../../types/business.model';
import { ToastrService } from 'ngx-toastr';
import { CategoryModalComponent } from '../../../shared/components/category-modal/category-modal';
import { AddServiceModalComponent } from '../../../shared/components/add-service-modal/add-service-modal';
import { EditServiceModalComponent } from '../../../shared/components/edit-service-modal/edit-service-modal';
import { of, switchMap, finalize } from 'rxjs';
import { PhotoService } from '../../../core/services/photo';
import { ServiceService } from '../../../core/services/service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-manage-services',
  standalone: true,
  imports: [
    CommonModule,
    CategoryModalComponent,
    AddServiceModalComponent,
    EditServiceModalComponent,
    FormsModule,
  ],
  templateUrl: './manage-services.html',
})
export class ManageServicesComponent implements OnInit {
  private serviceService = inject(ServiceService);
  private businessService = inject(BusinessService);
  private categoryService = inject(CategoryService);
  private photoService = inject(PhotoService);
  private toastr = inject(ToastrService);

  isLoading = true;
  isSavingCategory = false;

  business: BusinessDetail | null = null;
  showArchived = false;
  isCategoryModalVisible = false;
  categoryToEdit: ServiceCategory | null = null;
  
  isAddServiceModalVisible = false;
  serviceToEdit: Service | null = null;
  activeCategoryForService: ServiceCategory | null = null;

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;
    this.businessService.getMyBusiness().pipe(
      switchMap(businessData => {
        this.business = businessData;
        return this.categoryService.getCategories(businessData.id, this.showArchived);
      }),
      finalize(() => this.isLoading = false)
    ).subscribe({
      next: (categoriesData) => {
        if (this.business) {
          this.business.categories = categoriesData;
        }
      },
      error: () => this.toastr.error('Wystąpił błąd podczas ładowania danych.')
    });
  }

  openCategoryModal(category: ServiceCategory | null = null): void {
    this.categoryToEdit = category;
    this.isCategoryModalVisible = true;
  }

  closeCategoryModal(): void {
    this.isCategoryModalVisible = false;
    this.categoryToEdit = null;
  }

  onSaveCategory(event: { payload: { name: string }, file: File | null }): void {
    if (!this.business) return;

    this.isSavingCategory = true;

    const saveOperation$ = this.categoryToEdit
      ? this.categoryService.updateCategory(this.business.id, this.categoryToEdit.serviceCategoryId, event.payload).pipe(
          switchMap(() => {
            if (event.file) {
              return this.photoService.uploadCategoryPhoto(this.categoryToEdit!.serviceCategoryId, event.file);
            }
            return of(null);
          })
        ) : this.categoryService.addCategory(this.business.id, event.payload).pipe(
          switchMap(newCategory => {
            if (event.file) {
              return this.photoService.uploadCategoryPhoto(newCategory.serviceCategoryId, event.file);
            }
            return of(null);
          })
        );

    saveOperation$.pipe(
      finalize(() => this.isSavingCategory = false)
    ).subscribe({
      next: () => {
        const message = this.categoryToEdit ? 'Kategoria zaktualizowana!' : 'Kategoria dodana!';
        this.toastr.success(message);
        this.closeCategoryModal();
        this.loadData();
      },
      error: () => {
        const message = this.categoryToEdit ? 'Wystąpił błąd podczas aktualizacji.' : 'Wystąpił błąd podczas tworzenia kategorii.';
        this.toastr.error(message);
      }
    });
  }

  onDeleteCategory(category: ServiceCategory): void {
    if (confirm(`Czy na pewno chcesz usunąć kategorię "${category.name}"?`)) {
      this.categoryService.deleteCategory(this.business!.id, category.serviceCategoryId).subscribe({
        next: () => {
          this.toastr.success("Kategoria usunięta.");
          this.loadData();
        },
        error: (err) => this.toastr.error(err.error.title || 'Nie można usunąć kategorii, która zawiera usługi.')
      });
    }
  }

  onDeleteService(service: Service): void {
    if (confirm(`Czy na pewno chcesz usunąć usługę "${service.name}"?`)) {
      if (!this.business) return;
      
      this.serviceService.deleteService(this.business.id, service.id).subscribe({
        next: () => {
          this.toastr.success('Usługa została usunięta.');
          this.loadData();
        },
        error: (err) => {
          this.toastr.error(err.error?.title || 'Wystąpił błąd podczas usuwania usługi.');
        }
      });
    }
  }

  openAddServiceModal(category: ServiceCategory): void {
    this.activeCategoryForService = category;
    this.isAddServiceModalVisible = true;
  }

  openEditServiceModal(service: Service, category: ServiceCategory): void {
    this.activeCategoryForService = category;
    this.serviceToEdit = service;
  }
  
  closeServiceModalAndRefresh(): void {
    this.isAddServiceModalVisible = false;
    this.serviceToEdit = null;
    this.activeCategoryForService = null;
    this.loadData();
  }
}