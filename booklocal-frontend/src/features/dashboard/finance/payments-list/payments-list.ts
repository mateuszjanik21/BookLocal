import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { PaymentDto, PaymentService, UpdatePaymentDto } from '../../../../core/services/payment-service';
import { BusinessService } from '../../../../core/services/business-service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-payments-list',
  standalone: true,
  imports: [CommonModule, DatePipe, RouterModule, FormsModule],
  templateUrl: './payments-list.html',
  styleUrl: './payments-list.css',
  animations: [
    trigger('expandCollapse', [
      state('void', style({ height: '0', opacity: 0, overflow: 'hidden' })),
      state('*', style({ height: '*', opacity: 1, overflow: 'hidden' })),
      transition('void <=> *', [animate('250ms ease-in-out')])
    ])
  ]
})
export class PaymentsListComponent implements OnInit {
  private paymentService = inject(PaymentService);
  private businessService = inject(BusinessService);
  private toastr = inject(ToastrService);
  private route = inject(ActivatedRoute);

  payments: PaymentDto[] = [];
  isLoading = true;
  showSpinner = false;
  private spinnerTimeout: any;
  businessId: number | null = null;

  currentPage = 1;
  pageSize = 15;
  totalCount = 0;
  totalPages = 0;

  sortField: string = 'date';
  sortDir: string = 'desc';

  methodFilter: string = '';
  statusFilter: string = '';

  expandedPaymentId: number | null = null;

  isEditModalOpen = false;
  editingPayment: PaymentDto | null = null;
  editForm = { amount: 0, method: 0, status: 0 };
  editAmountError: string = '';

  paymentMethods: Record<string, string> = {
    'Cash': 'Gotówka',
    'Card': 'Karta',
    'Online': 'Online',
    'Other': 'Inne'
  };

  paymentStatuses: Record<string, string> = {
    'Pending': 'Oczekująca',
    'Completed': 'Zakończona',
    'Failed': 'Nieudana',
    'Refunded': 'Zwrócona'
  };

  statusColors: Record<string, string> = {
    'Pending': 'badge-warning',
    'Completed': 'badge-success',
    'Failed': 'badge-error',
    'Refunded': 'badge-info'
  };

  methodOptions = [
    { value: 0, label: 'Gotówka', key: 'Cash' },
    { value: 1, label: 'Karta', key: 'Card' },
    { value: 2, label: 'Online', key: 'Online' },
    { value: 3, label: 'Inne', key: 'Other' }
  ];

  statusOptions = [
    { value: 0, label: 'Oczekująca', key: 'Pending' },
    { value: 1, label: 'Zakończona', key: 'Completed' },
    { value: 2, label: 'Nieudana', key: 'Failed' },
    { value: 3, label: 'Zwrócona', key: 'Refunded' }
  ];

  private autoExpandId: number | null = null;

  ngOnInit(): void {
    const expandParam = this.route.snapshot.queryParamMap.get('expand');
    if (expandParam) this.autoExpandId = +expandParam;

    const pageParam = this.route.snapshot.queryParamMap.get('page');
    if (pageParam) this.currentPage = +pageParam;

    this.businessService.getMyBusiness().subscribe({
      next: (business) => {
        if (business) {
          this.businessId = business.id;
          this.loadData();
        }
      },
      error: () => this.toastr.error('Nie udało się pobrać danych firmy.')
    });
  }

  loadData(): void {
    if (!this.businessId) return;
    this.isLoading = true;
    this.showSpinner = false;
    clearTimeout(this.spinnerTimeout);
    this.spinnerTimeout = setTimeout(() => {
      if (this.isLoading) this.showSpinner = true;
    }, 500);

    this.paymentService.getBusinessPayments(
      this.businessId,
      this.currentPage,
      this.pageSize,
      this.sortField || undefined,
      this.sortDir || undefined,
      this.methodFilter || undefined,
      this.statusFilter || undefined
    ).subscribe({
      next: (data) => {
        this.payments = data.items;
        this.totalCount = data.totalCount;
        this.totalPages = data.totalPages;
        if (this.autoExpandId) {
          this.expandedPaymentId = this.autoExpandId;
          this.autoExpandId = null;
        }
        this.isLoading = false;
        this.showSpinner = false;
      },
      error: () => {
        this.toastr.error('Nie udało się pobrać listy płatności.');
        this.isLoading = false;
        this.showSpinner = false;
      }
    });
  }

  toggleSort(field: string): void {
    if (this.sortField === field) {
      this.sortDir = this.sortDir === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortField = field;
      this.sortDir = 'desc';
    }
    this.currentPage = 1;
    this.loadData();
  }

  getSortIcon(field: string): string {
    if (this.sortField !== field) return '';
    return this.sortDir === 'asc' ? '↑' : '↓';
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadData();
  }

  clearFilters(): void {
    this.methodFilter = '';
    this.statusFilter = '';
    this.currentPage = 1;
    this.loadData();
  }

  toggleDetails(paymentId: number): void {
    this.expandedPaymentId = this.expandedPaymentId === paymentId ? null : paymentId;
  }

  changePage(newPage: number): void {
    if (newPage < 1 || newPage > this.totalPages) return;
    this.currentPage = newPage;
    this.loadData();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxVisible = 5;
    let start = Math.max(1, this.currentPage - Math.floor(maxVisible / 2));
    let end = Math.min(this.totalPages, start + maxVisible - 1);
    if (end - start + 1 < maxVisible) {
      start = Math.max(1, end - maxVisible + 1);
    }
    for (let i = start; i <= end; i++) pages.push(i);
    return pages;
  }

  get rangeStart(): number {
    return (this.currentPage - 1) * this.pageSize + 1;
  }

  get rangeEnd(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalCount);
  }

  openEditModal(payment: PaymentDto, event?: Event): void {
    event?.stopPropagation();
    this.editingPayment = payment;
    this.editAmountError = '';
    const methodIdx = this.methodOptions.findIndex(m => m.key === payment.method);
    const statusIdx = this.statusOptions.findIndex(s => s.key === payment.status);
    this.editForm = {
      amount: payment.amount,
      method: methodIdx >= 0 ? methodIdx : 0,
      status: statusIdx >= 0 ? statusIdx : 0
    };
    this.isEditModalOpen = true;
  }

  closeEditModal(): void {
    this.isEditModalOpen = false;
    this.editingPayment = null;
    this.editAmountError = '';
  }

  validateAmount(): boolean {
    if (!this.editingPayment) return false;
    if (this.editForm.amount <= 0) {
      this.editAmountError = 'Kwota musi być większa niż 0.';
      return false;
    }
    if (this.editForm.amount > this.editingPayment.reservationAmount) {
      this.editAmountError = `Kwota nie może przekraczać wartości rezerwacji (${this.editingPayment.reservationAmount.toFixed(2)} zł).`;
      return false;
    }
    this.editAmountError = '';
    return true;
  }

  saveEdit(): void {
    if (!this.editingPayment || !this.validateAmount()) return;
    const dto: UpdatePaymentDto = {
      amount: this.editForm.amount,
      method: this.editForm.method,
      status: this.editForm.status
    };
    this.paymentService.updatePayment(this.editingPayment.paymentId, dto).subscribe({
      next: () => {
        this.toastr.success('Płatność zaktualizowana.');
        this.closeEditModal();
        this.loadData();
      },
      error: () => this.toastr.error('Nie udało się zaktualizować płatności.')
    });
  }

  confirmDelete(payment: PaymentDto, event?: Event): void {
    event?.stopPropagation();
    if (confirm(`Czy na pewno chcesz trwale usunąć płatność #${payment.paymentId} na kwotę ${payment.amount.toFixed(2)} ${payment.currency}?`)) {
      this.paymentService.deletePayment(payment.paymentId).subscribe({
        next: () => {
          this.toastr.success('Płatność została usunięta.');
          this.loadData();
        },
        error: () => this.toastr.error('Nie udało się usunąć płatności.')
      });
    }
  }
}
