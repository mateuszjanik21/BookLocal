import { Component, Input, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HRService } from '../../../../core/services/hr-service';
import { FinanceService } from '../../../../core/services/finance-service';
import { ToastrService } from 'ngx-toastr';
import { forkJoin, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

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
  private financeService = inject(FinanceService);
  private toastr = inject(ToastrService);

  today = new Date();
  selectedMonth: number = this.today.getMonth() + 1;
  selectedYear: number = this.today.getFullYear();

  isLoading = false;
  isRefreshing = false;

  totalRevenue = 0;
  totalEmployerCost = 0;
  reportsCount = 0;
  payrollsCount = 0;

  history: MonthSlot[] = [];

  get netResult(): number { return this.totalRevenue - this.totalEmployerCost; }
  get noData(): boolean { return this.reportsCount === 0 && this.payrollsCount === 0 && !this.isRefreshing; }

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

    const slots: { month: number; year: number }[] = [];
    for (let i = 5; i >= 0; i--) {
      const d = new Date(this.selectedYear, this.selectedMonth - 1 - i, 1);
      slots.push({ month: d.getMonth() + 1, year: d.getFullYear() });
    }

    const slotRequests = slots.map(({ month, year }) =>
      forkJoin({
        reports: this.financeService.getReports(this.businessId, month, year).pipe(catchError(() => of([]))),
        payrolls: this.hrService.getPayrolls(this.businessId, month, year).pipe(catchError(() => of([]))),
      }).pipe(
        map(({ reports, payrolls }) => ({
          month, year,
          shortLabel: this.monthLabels[month - 1],
          revenue: reports.reduce((s, r) => s + (r.totalRevenue ?? 0), 0),
          employerCost: payrolls
            .filter(p => (p.totalEmployerCost ?? 0) > 0)
            .reduce((s, p) => s + p.totalEmployerCost, 0),
          revenueRatio: 0,
          costRatio: 0,
        }))
      )
    );

    forkJoin(slotRequests).subscribe({
      next: (results) => {
        const maxRev  = Math.max(...results.map(r => r.revenue), 1);
        const maxCost = Math.max(...results.map(r => r.employerCost), 1);
        this.history = results.map(r => ({
          ...r,
          revenueRatio: r.revenue     / maxRev,
          costRatio:    r.employerCost / maxCost,
        }));

        const current = results[results.length - 1];
        this.totalRevenue      = current.revenue;
        this.totalEmployerCost = current.employerCost;

        this.reportsCount  = 0; 
        this.payrollsCount = 0;

        forkJoin({
          reports:  this.financeService.getReports(this.businessId, this.selectedMonth, this.selectedYear).pipe(catchError(() => of([]))),
          payrolls: this.hrService.getPayrolls(this.businessId, this.selectedMonth, this.selectedYear).pipe(catchError(() => of([]))),
        }).subscribe(({ reports, payrolls }) => {
          this.reportsCount  = reports.length;
          this.payrollsCount = payrolls.filter(p => (p.totalEmployerCost ?? 0) > 0).length;
          this.isLoading     = false;
          this.isRefreshing  = false;
        });
      },
      error: () => {
        this.toastr.error('Błąd ładowania podsumowania.');
        this.isLoading = false;
        this.isRefreshing = false;
      }
    });
  }
}
