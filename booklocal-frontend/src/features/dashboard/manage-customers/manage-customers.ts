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

  page = 1;
  pageSize = 20;
  totalCount = 0;

  get totalPages() {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  get filteredCustomers() {
      return this.customers;
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
      this.page = 1;
      this.loadCustomers();
    });
  }

  onSearch(term: string) {
    this.searchSubject.next(term);
  }

  loadCustomers() {
    if (!this.businessId) return;
    this.isLoading = true;
    this.customerService.getCustomers(this.businessId, this.searchQuery, this.page, this.pageSize).subscribe({
      next: (data) => {
        this.customers = data.items;
        this.totalCount = data.totalCount;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  nextPage() {
    if (this.page * this.pageSize < this.totalCount) {
        this.page++;
        this.loadCustomers();
    }
  }

  prevPage() {
    if (this.page > 1) {
        this.page--;
        this.loadCustomers();
    }
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
