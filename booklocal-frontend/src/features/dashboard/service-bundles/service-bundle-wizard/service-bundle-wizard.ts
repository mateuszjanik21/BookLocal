import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ServiceBundleService } from '../../../../core/services/service-bundle';
import { BusinessService } from '../../../../core/services/business-service';
import { CategoryService } from '../../../../core/services/category';
import { CreateServiceBundleItemPayload, CreateServiceBundlePayload } from '../../../../types/service-bundle.model';
import { ServiceCategory, Service, ServiceVariant } from '../../../../types/business.model';
import { ToastrService } from 'ngx-toastr';
import { switchMap } from 'rxjs';

interface DraftItem {
    serviceVariantId: number;
    variantName: string;
    serviceName: string;
    durationMinutes: number;
    originalPrice: number;
}

import { PhotoService } from '../../../../core/services/photo';

@Component({
  selector: 'app-service-bundle-wizard',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './service-bundle-wizard.html'
})
export class ServiceBundleWizardComponent implements OnInit {
  private serviceBundleService = inject(ServiceBundleService);
  private businessService = inject(BusinessService);
  private categoryService = inject(CategoryService);
  private photoService = inject(PhotoService);
  private toastr = inject(ToastrService);
  private router = inject(Router);

  currentStep = 1;
  isSaving = false;
  businessId: number | null = null;

  bundleName = '';
  bundleDescription = '';
  bundlePrice = 0;
  selectedFile: File | null = null;

  categories: ServiceCategory[] = [];
  selectedCategory: ServiceCategory | null = null;
  selectedService: Service | null = null;
  
  items: DraftItem[] = [];

  ngOnInit() {
    this.businessService.getMyBusiness().pipe(
        switchMap(business => {
            this.businessId = business.id;
            return this.categoryService.getCategories(business.id, false);
        })
    ).subscribe({
        next: (cats) => this.categories = cats,
        error: () => this.toastr.error('Nie udało się pobrać usług.')
    });
  }

  getStepTitle() {
      switch(this.currentStep) {
          case 1: return 'Podstawowe Informacje';
          case 2: return 'Wybór Usług';
          case 3: return 'Podsumowanie';
          default: return '';
      }
  }

  nextStep() {
      if (this.currentStep < 3) this.currentStep++;
  }

  onFileSelected(event: any) {
      const file = event.target.files[0];
      if (file) {
          this.selectedFile = file;
      }
  }

  onCategoryChange() {
      this.selectedService = null;
  }

  onServiceChange() {

  }

  addVariant(variant: ServiceVariant) {
      if (!this.selectedService) return;
      
      const item: DraftItem = {
          serviceVariantId: variant.serviceVariantId,
          variantName: variant.name,
          serviceName: this.selectedService.name,
          durationMinutes: variant.durationMinutes,
          originalPrice: variant.price
      };
      
      this.items.push(item);
      this.toastr.success('Dodano usługę do pakietu.');
  }

  removeItem(index: number) {
      this.items.splice(index, 1);
  }

  moveItem(item: DraftItem, direction: number) {
      const index = this.items.indexOf(item);
      if (index < 0) return;
      const newIndex = index + direction;
      if (newIndex >= 0 && newIndex < this.items.length) {
          [this.items[index], this.items[newIndex]] = [this.items[newIndex], this.items[index]];
      }
  }

  calculateTotalItemsPrice() {
      return this.items.reduce((acc, curr) => acc + curr.originalPrice, 0);
  }

  createBundle() {
      if (!this.businessId) return;
      this.isSaving = true;

      const payload: CreateServiceBundlePayload = {
          name: this.bundleName,
          description: this.bundleDescription,
          totalPrice: this.bundlePrice,
          items: this.items.map((item, index) => ({
              serviceVariantId: item.serviceVariantId,
              sequenceOrder: index + 1
          }))
      };

      this.serviceBundleService.createBundle(this.businessId, payload).pipe(
          switchMap(bundle => {
              if (this.selectedFile) {
                  return this.photoService.uploadBundlePhoto(bundle.serviceBundleId, this.selectedFile);
              }
              return Promise.resolve(bundle);
          })
      ).subscribe({
          next: () => {
              this.toastr.success('Pakiet został utworzony!');
              this.router.navigate(['/dashboard/bundles']);
          },
          error: (err) => {
              this.toastr.error(err.error || 'Wystąpił błąd podczas tworzenia pakietu.');
              this.isSaving = false;
          }
      });
  }
}
