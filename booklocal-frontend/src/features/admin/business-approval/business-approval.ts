import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminBusinessListDto, AdminService, VerifyBusinessDto } from '../../../core/services/admin-service';
import { ToastrService } from 'ngx-toastr';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-admin-business-approval',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './business-approval.html'
})
export class AdminBusinessApprovalComponent implements OnInit {
  private adminService = inject(AdminService);
  private toastr = inject(ToastrService);

  businesses: AdminBusinessListDto[] = [];
  isLoading = false;
  filterStatus: string = 'Pending'; // Pending, Approved, Rejected

  ngOnInit() {
    this.loadBusinesses();
  }

  loadBusinesses() {
    this.isLoading = true;
    this.adminService.getBusinesses(this.filterStatus).subscribe({
      next: (data) => {
        this.businesses = data;
        this.isLoading = false;
      },
      error: () => {
        this.toastr.error('Nie udało się pobrać listy firm.');
        this.isLoading = false;
      }
    });
  }

  approve(id: number) {
    if(!confirm('Czy na pewno chcesz zatwierdzić tę firmę?')) return;
    
    const dto: VerifyBusinessDto = { isApproved: true };
    this.adminService.verifyBusiness(id, dto).subscribe({
      next: () => {
        this.toastr.success('Firma zatwierdzona.');
        this.loadBusinesses();
      },
      error: () => this.toastr.error('Błąd weryfikacji.')
    });
  }

  reject(id: number) {
    const reason = prompt('Podaj powód odrzucenia:');
    if (reason === null) return; // Cancelled

    const dto: VerifyBusinessDto = { isApproved: false, rejectionReason: reason };
    this.adminService.verifyBusiness(id, dto).subscribe({
        next: () => {
          this.toastr.info('Firma odrzucona.');
          this.loadBusinesses();
        },
        error: () => this.toastr.error('Błąd weryfikacji.')
    });
  }
}
