import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { CustomerService, PagedResult } from '../../../core/services/customer-service';
import { CustomerDetail, ReservationHistory } from '../../../types/customer.models';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-customer-details-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './customer-details-modal.html',
})
export class CustomerDetailsModalComponent implements OnInit {
  @Input({ required: true }) businessId!: number;
  @Input({ required: true }) customerId!: string;
  @Output() closed = new EventEmitter<any | null>();

  private customerService = inject(CustomerService);
  private fb = inject(FormBuilder);
  private toastr = inject(ToastrService);

  customer: CustomerDetail | null = null;
  isLoading = true;

  history: ReservationHistory[] = [];
  currentPage = 1;
  pageSize = 10;
  totalHistoryCount = 0;
  isLoadingHistory = false;

  notesForm = this.fb.group({
    privateNotes: [''],
    allergies: [''],
    formulas: ['']
  });

  ngOnInit() {
    this.loadDetails();
  }

  loadDetails() {
    this.isLoading = true;
    this.customerService.getCustomerDetails(this.businessId, this.customerId).subscribe({
      next: (data) => {
        this.customer = data;
        this.notesForm.patchValue({
          privateNotes: data.privateNotes,
          allergies: data.allergies,
          formulas: data.formulas
        });
        this.isLoading = false;
        this.loadHistory(1);
      },
      error: () => {
        this.toastr.error('Nie udało się pobrać szczegółów klienta.');
        this.closed.emit(false);
      }
    });
  }

  loadHistory(page: number, append: boolean = false) {
    this.isLoadingHistory = true;
    this.customerService.getCustomerHistory(this.businessId, this.customerId, page, this.pageSize)
      .subscribe({
        next: (res: PagedResult<ReservationHistory>) => {
          if (append) {
            this.history = [...this.history, ...res.items];
          } else {
            this.history = res.items;
          }
          this.totalHistoryCount = res.totalCount;
          this.currentPage = res.pageNumber;
          this.isLoadingHistory = false;
        },
        error: () => {
          this.toastr.error('Nie udało się pobrać historii wizyt.');
          this.isLoadingHistory = false;
        }
      });
  }

  loadMore() {
    if (this.history.length < this.totalHistoryCount && !this.isLoadingHistory) {
      this.loadHistory(this.currentPage + 1, true);
    }
  }

  saveNotes() {
    const payload = {
      privateNotes: this.notesForm.value.privateNotes || '',
      allergies: this.notesForm.value.allergies || '',
      formulas: this.notesForm.value.formulas || ''
    };
    this.customerService.updateNotes(this.businessId, this.customerId, payload)
      .subscribe({
        next: () => {
          this.toastr.success('Notatki zapisane.');
        },
        error: () => this.toastr.error('Błąd zapisu.')
      });
  }

  toggleVIP() {
    if (!this.customer) return;
    const newStatus = !this.customer.isVIP;
    this.customerService.updateStatus(this.businessId, this.customerId, {
      isVIP: newStatus,
      isBanned: this.customer.isBanned
    }).subscribe({
      next: () => {
        if(this.customer) this.customer.isVIP = newStatus;
        this.toastr.success(newStatus ? 'Klient oznaczony jako VIP' : 'Status VIP usunięty');
      }
    });
  }

  toggleBan() {
    if (!this.customer) return;
    const newStatus = !this.customer.isBanned;
    if (newStatus && !confirm('Czy na pewno chcesz zablokować tego klienta?')) return;

    this.customerService.updateStatus(this.businessId, this.customerId, {
      isVIP: this.customer.isVIP,
      isBanned: newStatus
    }).subscribe({
      next: () => {
        if(this.customer) this.customer.isBanned = newStatus;
        this.toastr.warning(newStatus ? 'Klient zablokowany' : 'Blokada zdjęta');
      }
    });
  }

  closeModal() {
    this.closed.emit(null);
  }
}
