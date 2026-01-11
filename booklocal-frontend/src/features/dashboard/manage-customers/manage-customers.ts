import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CustomerService } from '../../../core/services/customer-service';
import { BusinessService } from '../../../core/services/business-service';
import { CustomerListItem } from '../../../types/customer.models';
import { debounceTime, distinctUntilChanged, Subject, switchMap } from 'rxjs';
import { CustomerDetailsModalComponent } from '../../../shared/components/customer-details-modal/customer-details-modal';

@Component({
  selector: 'app-manage-customers',
  standalone: true,
  imports: [CommonModule, FormsModule, CustomerDetailsModalComponent],
  templateUrl: './manage-customers.html',
})
export class ManageCustomersComponent implements OnInit {
  private customerService = inject(CustomerService);
  private businessService = inject(BusinessService);

  customers: CustomerListItem[] = [];
  isLoading = false;
  searchQuery = '';
  private searchSubject = new Subject<string>();
  
  businessId: number | null = null;
  selectedCustomer: CustomerListItem | null = null;
  
  showRealCustomersOnly = false;

  get filteredCustomers() {
    if (!this.showRealCustomersOnly) return this.customers;
    return this.customers.filter(c => c.totalSpent > 0 || (c.lastVisitDate !== '0001-01-01T00:00:00'));
  }

  ngOnInit() {
    this.businessService.getMyBusiness().subscribe(b => {
      this.businessId = b.id;
      this.loadCustomers();
    });

    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(term => {
      this.searchQuery = term;
      this.loadCustomers();
    });
  }

  onSearch(term: string) {
    this.searchSubject.next(term);
  }

  loadCustomers() {
    if (!this.businessId) return;
    this.isLoading = true;
    this.customerService.getCustomers(this.businessId, this.searchQuery).subscribe({
      next: (data) => {
        this.customers = data;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  openDetails(customer: CustomerListItem) {
    this.selectedCustomer = customer;
  }

  closeDetails(updatedCustomer: any) {
    this.selectedCustomer = null;
    if (updatedCustomer && this.businessId) {
      const index = this.customers.findIndex(c => c.profileId === updatedCustomer.profileId);
      if (index !== -1) {
        const existing = this.customers[index];
        this.customers[index] = {
            ...existing,
            isVIP: updatedCustomer.isVIP,
            isBanned: updatedCustomer.isBanned
        };
      }
    }
  }
}
