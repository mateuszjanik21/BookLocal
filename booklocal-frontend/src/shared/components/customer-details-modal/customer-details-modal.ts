import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { CustomerService } from '../../../core/services/customer-service';
import { CustomerDetail } from '../../../types/customer.models';
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
      },
      error: () => {
        this.toastr.error('Nie udało się pobrać szczegółów клиента.');
        this.closed.emit(false);
      }
    });
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
    this.closed.emit(this.customer);
  }
}
