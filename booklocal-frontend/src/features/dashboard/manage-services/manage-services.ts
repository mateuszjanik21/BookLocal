import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BusinessService } from '../../../core/services/business-service';
import { CategoryService } from '../../../core/services/category';
import { BusinessDetail, ServiceCategory, Service } from '../../../types/business.model';
import { ToastrService } from 'ngx-toastr';
import { CategoryModalComponent } from '../../../shared/components/category-modal/category-modal';
import { AddServiceModalComponent } from '../../../shared/components/add-service-modal/add-service-modal';
import { EditServiceModalComponent } from '../../../shared/components/edit-service-modal/edit-service-modal';
import { of, switchMap } from 'rxjs';
import { PhotoService } from '../../../core/services/photo';

@Component({
  selector: 'app-manage-services',
  standalone: true,
  imports: [
    CommonModule, 
    CategoryModalComponent,
    AddServiceModalComponent,
    EditServiceModalComponent
  ],
  templateUrl: './manage-services.html',
})
export class ManageServicesComponent implements OnInit {
  private businessService = inject(BusinessService);
  private categoryService = inject(CategoryService);
  private photoService = inject(PhotoService);
  private toastr = inject(ToastrService);

  isLoading = true;
  business: BusinessDetail | null = null;
  
  isCategoryModalVisible = false;
  categoryToEdit: ServiceCategory | null = null;
  
  isAddServiceModalVisible = false;
  serviceToEdit: Service | null = null;
  activeCategoryForService: ServiceCategory | null = null;

  ngOnInit(): void { this.loadData(); }

  loadData(): void {
    this.isLoading = true;
    this.businessService.getMyBusiness().subscribe(data => {
      this.business = data;
      this.isLoading = false;
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

    if (this.categoryToEdit) {
      this.categoryService.updateCategory(this.business.id, this.categoryToEdit.serviceCategoryId, event.payload)
        .pipe(
          switchMap(() => {
            if (event.file) {
              return this.photoService.uploadCategoryPhoto(this.categoryToEdit!.serviceCategoryId, event.file);
            }
            return of(null);
          })
        ).subscribe({
          next: () => {
            this.toastr.success('Kategoria zaktualizowana!');
            this.closeCategoryModal();
            this.loadData();
          },
          error: () => this.toastr.error('Wystąpił błąd podczas aktualizacji.')
        });
    } 
    else {
      this.categoryService.addCategory(this.business.id, event.payload)
        .pipe(
          switchMap(newCategory => {
            if (event.file) {
              return this.photoService.uploadCategoryPhoto(newCategory.serviceCategoryId, event.file);
            }
            return of(null);
          })
        ).subscribe({
          next: () => {
            this.toastr.success('Kategoria dodana!');
            this.closeCategoryModal();
            this.loadData();
          },
          error: () => this.toastr.error('Wystąpił błąd podczas tworzenia kategorii.')
        });
    }
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