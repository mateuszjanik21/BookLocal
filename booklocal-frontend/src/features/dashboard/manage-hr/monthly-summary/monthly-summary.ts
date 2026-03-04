import { Component, Input, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HRService } from '../../../../core/services/hr-service';
import { FinanceService } from '../../../../core/services/finance-service';
import { ToastrService } from 'ngx-toastr';

interface MonthSlot {
  month: number;
  year: number;
  shortLabel: string;
  revenue: number;
  employerCost: number;
  revenueRatio: number;
  costRatio: number;
}

@Component({
  selector: 'app-monthly-summary',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './monthly-summary.html',
})
export class MonthlySummaryComponent implements OnInit {
  @Input() businessId!: number;

  private hrService = inject(HRService);
  private toastr = inject(ToastrService);

  today = new Date();
  selectedMonth: number = this.today.getMonth() + 1;
  selectedYear: number = this.today.getFullYear();

  isLoading = false;
  isRefreshing = false;

  totalRevenue = 0;
  totalEmployerCost = 0;

  history: MonthSlot[] = [];

  get netResult(): number { return this.totalRevenue - this.totalEmployerCost; }
  get noData(): boolean { return this.totalRevenue === 0 && this.totalEmployerCost === 0 && !this.isRefreshing; }

  monthLabels = ['Sty','Lut','Mar','Kwi','Maj','Cze','Lip','Sie','Wrz','Paź','Lis','Gru'];
  monthLabelsFull = ['Styczeń','Luty','Marzec','Kwiecień','Maj','Czerwiec','Lipiec','Sierpień','Wrzesień','Październik','Listopad','Grudzień'];

  get selectedMonthLabel() { return this.monthLabelsFull[this.selectedMonth - 1]; }

  ngOnInit(): void { if (this.businessId) this.loadAll(); }

  changeMonth(delta: number) {
    let m = this.selectedMonth + delta;
    let y = this.selectedYear;
    if (m < 1)  { m = 12; y--; }
    if (m > 12) { m = 1;  y++; }
    this.selectedMonth = m;
    this.selectedYear  = y;
    this.loadAll();
  }

  loadAll() {
    if (this.history.length > 0) {
      this.isRefreshing = true;
    } else {
      this.isLoading = true;
    }

    this.hrService.getMonthlySummary(this.businessId, this.selectedMonth, this.selectedYear, 6).subscribe({
      next: (results) => {
        const maxRev  = Math.max(...results.map(r => r.revenue), 1);
        const maxCost = Math.max(...results.map(r => r.employerCost), 1);
        
        this.history = results.map(r => ({
          month: r.month,
          year: r.year,
          shortLabel: this.monthLabels[r.month - 1],
          revenue: r.revenue,
          employerCost: r.employerCost,
          revenueRatio: r.revenue / maxRev,
          costRatio: r.employerCost / maxCost
        }));

        const current = this.history[this.history.length - 1];
        if (current) {
          this.totalRevenue = current.revenue;
          this.totalEmployerCost = current.employerCost;
        }

        this.isLoading = false;
        this.isRefreshing = false;
      },
      error: () => {
        this.toastr.error('Błąd ładowania podsumowania.');
        this.isLoading = false;
        this.isRefreshing = false;
      }
    });
  }
}
