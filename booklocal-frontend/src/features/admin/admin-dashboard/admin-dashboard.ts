import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService, AdminStats } from '../../../core/services/admin-service';
import { ToastrService } from 'ngx-toastr';

import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-dashboard.html'
})
export class AdminDashboardComponent implements OnInit {
  private adminService = inject(AdminService);
  private toastr = inject(ToastrService);

  stats: AdminStats | null = null;
  isLoading = true;

  ngOnInit() {
    this.reloadStats();
  }

  reloadStats() {
    this.isLoading = true;
    this.adminService.getStats().subscribe({
      next: (data) => {
        this.stats = data;
        this.isLoading = false;
      },
      error: () => {
        this.toastr.error('Nie udało się pobrać statystyk.');
        this.isLoading = false;
      }
    });
  }
}
