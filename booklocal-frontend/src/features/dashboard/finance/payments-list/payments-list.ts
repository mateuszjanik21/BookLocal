import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, DatePipe, CurrencyPipe } from '@angular/common';
import { RouterModule } from '@angular/router';
import { PaymentDto, PaymentService } from '../../../../core/services/payment-service';
import { BusinessService } from '../../../../core/services/business-service';
import { ToastrService } from 'ngx-toastr';
import { finalize, switchMap } from 'rxjs';

@Component({
  selector: 'app-payments-list',
  standalone: true,
  imports: [CommonModule, DatePipe, CurrencyPipe, RouterModule],
  templateUrl: './payments-list.html',
})
export class PaymentsListComponent implements OnInit {
  private paymentService = inject(PaymentService);
  private businessService = inject(BusinessService);
  private toastr = inject(ToastrService);

  payments: PaymentDto[] = [];
  isLoading = true;

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;
    
    this.businessService.getMyBusiness().pipe(
      switchMap(business => {
        return this.paymentService.getBusinessPayments(business.id);
      }),
      finalize(() => this.isLoading = false)
    ).subscribe({
      next: (data) => {
        this.payments = data;
      },
      error: () => {
        this.toastr.error('Nie udało się pobrać listy płatności.');
      }
    });
  }
}
