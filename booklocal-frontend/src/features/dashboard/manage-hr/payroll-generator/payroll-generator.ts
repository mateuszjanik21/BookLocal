import { Component, Input, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HRService } from '../../../../core/services/hr-service';
import { Employee } from '../../../../types/business.model';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs';
import { EmployeePayroll, PayrollStatus } from '../../../../types/hr.models';

@Component({
  selector: 'app-payroll-generator',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './payroll-generator.html',
})
export class PayrollGeneratorComponent implements OnInit {
  @Input() businessId!: number;

  private hrService = inject(HRService);
  private toastr = inject(ToastrService);

  payrolls: EmployeePayroll[] = [];
  employees: Employee[] = [];
  isLoading = false;
  isGenerating = false;
  isDeleting: { [id: number]: boolean } = {};

  today = new Date();
  filterMonth: number | undefined = undefined;
  filterYear: number = this.today.getFullYear();
  filterDay: number | undefined = undefined;
  selectedEmployeeId: number | undefined = undefined;

  private lastMonth = this.today.getMonth() === 0
    ? { month: 12, year: this.today.getFullYear() - 1 }
    : { month: this.today.getMonth(), year: this.today.getFullYear() };

  get lastMonthLabel(): string {
    const m = this.months.find(x => x.value === this.lastMonth.month);
    return `${m?.label} ${this.lastMonth.year}`;
  }

  months = [
    { value: 1, label: 'Styczeń' }, { value: 2, label: 'Luty' }, { value: 3, label: 'Marzec' },
    { value: 4, label: 'Kwiecień' }, { value: 5, label: 'Maj' }, { value: 6, label: 'Czerwiec' },
    { value: 7, label: 'Lipiec' }, { value: 8, label: 'Sierpień' }, { value: 9, label: 'Wrzesień' },
    { value: 10, label: 'Październik' }, { value: 11, label: 'Listopad' }, { value: 12, label: 'Grudzień' }
  ];

  get availableMonths() {
    if (this.filterYear < this.today.getFullYear()) {
      return this.months;
    } else if (this.filterYear === this.today.getFullYear()) {
      return this.months.filter(m => m.value <= this.today.getMonth() + 1);
    } else {
      return [];
    }
  }

  ngOnInit(): void {
    if (this.businessId) {
      this.loadEmployees();
      this.loadPayrolls();
    }
  }

  loadEmployees() {
    this.hrService.getEmployeesForHr(this.businessId).subscribe(data => this.employees = data);
  }

  loadPayrolls() {
    this.isLoading = true;
    this.hrService.getPayrolls(this.businessId, this.filterMonth, this.filterYear)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (data) => this.payrolls = data,
        error: () => this.toastr.error('Błąd ładowania list płac.')
      });
  }

  generatePayroll() {
    if (!this.filterMonth || !this.filterYear) {
      this.toastr.warning('Wybierz miesiąc i rok, dla których chcesz wygenerować listę płac.');
      return;
    }

    let targetEmployeeIds: number[] = [];
    
    if (this.selectedEmployeeId) {
      targetEmployeeIds = [Number(this.selectedEmployeeId)];
    } else {
      if (this.employees.length === 0) {
        this.toastr.warning('Brak pracowników do wygenerowania płac.');
        return;
      }
      targetEmployeeIds = this.employees.map(e => e.id);
    }

    this.isGenerating = true;

    this.hrService.generatePayrollForAll(
      this.businessId,
      targetEmployeeIds,
      this.filterMonth,
      this.filterYear,
      this.filterDay
    )
    .pipe(finalize(() => this.isGenerating = false))
    .subscribe({
      next: (results) => {
        const generatedCount = results.filter(r => r !== null).length;
        if (generatedCount > 0) {
          this.toastr.success(`Wygenerowano ${generatedCount} nowych list płac za ${this.filterMonth}/${this.filterYear}!`);
        } else {
          this.toastr.info(`Wszystkie możliwe płace za ten miesiąc zostały już wygenerowane.`);
        }
        this.loadPayrolls();
      },
      error: () => this.toastr.error('Wystąpił nieoczekiwany błąd serwera.')
    });
  }

  deletePayroll(event: Event, payroll: EmployeePayroll) {
    event.stopPropagation();
    if (!confirm('Czy na pewno chcesz usunąć tę listę płac? Będziesz mógł wygenerować ją ponownie.')) return;
    
    this.isDeleting[payroll.payrollId] = true;
    this.hrService.deletePayroll(this.businessId, payroll.payrollId).pipe(
      finalize(() => delete this.isDeleting[payroll.payrollId])
    ).subscribe({
      next: () => {
        this.toastr.success('Płaca została pomyślnie usunięta.');
        this.loadPayrolls();
      },
      error: () => {
        this.toastr.error('Wystąpił błąd podczas usuwania płacy.');
      }
    });
  }

  getStatusLabel(status: PayrollStatus): string {
    const labels = ['Szkic', 'Przeliczone', 'Zatwierdzone', 'Opłacone'];
    return labels[status] || 'Nieznany';
  }
}
