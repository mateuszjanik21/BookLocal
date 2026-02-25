import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CustomerService } from '../../../core/services/customer-service';
import { BusinessService } from '../../../core/services/business-service';
import { CustomerListItem, CustomerStatusFilter, CustomerHistoryFilter, CustomerSpentFilter } from '../../../types/customer.models';
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
  
  statusFilter: CustomerStatusFilter = CustomerStatusFilter.All;
  historyFilter: CustomerHistoryFilter = CustomerHistoryFilter.All;
  spentFilter: CustomerSpentFilter = CustomerSpentFilter.All;
  
  statusFilterOptions = [
    { value: CustomerStatusFilter.All, label: 'Wszyscy' },
    { value: CustomerStatusFilter.Standard, label: 'Standardowi' },
    { value: CustomerStatusFilter.VIP, label: 'VIP' },
    { value: CustomerStatusFilter.Banned, label: 'Zablokowani' }
  ];

  historyFilterOptions = [
    { value: CustomerHistoryFilter.All, label: 'Każda historia' },
    { value: CustomerHistoryFilter.WithHistory, label: 'Z historią' },
    { value: CustomerHistoryFilter.WithoutHistory, label: 'Bez historii' }
  ];

  spentFilterOptions = [
    { value: CustomerSpentFilter.All, label: 'Dowolny obrót' },
    { value: CustomerSpentFilter.Any, label: '> 0 zł' },
    { value: CustomerSpentFilter.Over100, label: '> 100 zł' },
    { value: CustomerSpentFilter.Over500, label: '> 500 zł' },
    { value: CustomerSpentFilter.Over1000, label: '> 1000 zł' }
  ];

  page = 1;
  pageSize = 20;
  totalCount = 0;

  get totalPages() {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  get paginationPages(): (number | string)[] {
    const total = this.totalPages;
    if (total <= 7) {
      return Array.from({ length: total }, (_, i) => i + 1);
    }

    const current = this.page;
    const pages: (number | string)[] = [];

    pages.push(1);

    if (current > 3) {
      pages.push('...');
    }

    const start = Math.max(2, current - 1);
    const end = Math.min(total - 1, current + 1);

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }

    if (current < total - 2) {
      pages.push('...');
    }

    if (total > 1) {
      pages.push(total);
    }

    return pages;
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

  onFilterChange() {
    this.page = 1;
    this.loadCustomers();
  }

  loadCustomers() {
    if (!this.businessId) return;
    this.isLoading = true;
    this.customerService.getCustomers(this.businessId, this.searchQuery, this.statusFilter, this.historyFilter, this.spentFilter, this.page, this.pageSize).subscribe({
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

  onPageInput(event: any) {
    const val = parseInt(event.target.value, 10);
    if (!isNaN(val)) {
        let newPage = Math.max(1, Math.min(val, this.totalPages));
        if (this.page !== newPage) {
            this.page = newPage;
            this.loadCustomers();
        } else {
            event.target.value = this.page;
        }
    } else {
        event.target.value = this.page;
    }
  }

  goToPage(p: number | string) {
    if (typeof p === 'number' && p !== this.page) {
        this.page = p;
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
