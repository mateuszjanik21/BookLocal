import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ServiceBundleService } from '../../../../core/services/service-bundle';
import { BusinessService } from '../../../../core/services/business-service';
import { ServiceBundle } from '../../../../types/service-bundle.model';
import { ToastrService } from 'ngx-toastr';
import { switchMap } from 'rxjs';

@Component({
  selector: 'app-service-bundles-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './service-bundles-list.html'
})
export class ServiceBundlesListComponent implements OnInit {
  private serviceBundleService = inject(ServiceBundleService);
  private businessService = inject(BusinessService);
  private toastr = inject(ToastrService);

  bundles: ServiceBundle[] = [];
  isLoading = true;

  ngOnInit() {
    this.loadBundles();
  }

  loadBundles() {
    this.isLoading = true;
    this.businessService.getMyBusiness().pipe(
        switchMap(business => this.serviceBundleService.getBundles(business.id))
    ).subscribe({
        next: (data) => {
            this.bundles = data;
            this.isLoading = false;
        },
        error: () => {
            this.toastr.error('Nie udało się pobrać pakietów.');
            this.isLoading = false;
        }
    });
  }

  deleteBundle(bundle: ServiceBundle) {
    if(!confirm(`Czy na pewno chcesz usunąć pakiet "${bundle.name}"?`)) return;

    this.serviceBundleService.deleteBundle(bundle.businessId, bundle.serviceBundleId).subscribe({
        next: () => {
            this.toastr.success('Pakiet został usunięty.');
            this.loadBundles();
        },
        error: () => this.toastr.error('Wystąpił błąd podczas usuwania.')
    });
  }
}
