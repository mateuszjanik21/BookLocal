import { Component, Input, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HRService } from '../../../../core/services/hr-service';
import { EmployeeService } from '../../../../core/services/employee-service';
import { Employee } from '../../../../types/business.model';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs';
import { EmployeePayroll, PayrollStatus } from '../../../../types/hr.models';

@Component({
  selector: 'app-payroll-generator',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './payroll-generator.html',
})
export class PayrollGeneratorComponent implements OnInit {
  @Input() businessId!: number;

  private fb = inject(FormBuilder);
  private hrService = inject(HRService);
  private employeeService = inject(EmployeeService);
  private toastr = inject(ToastrService);

  payrolls: EmployeePayroll[] = [];
  employees: Employee[] = [];
  isLoading = false;
  isGenerating = false;

  today = new Date();
  currentMonth = this.today.getMonth() + 1;
  currentYear = this.today.getFullYear();

  months = [
    { value: 1, label: 'Styczeń' }, { value: 2, label: 'Luty' }, { value: 3, label: 'Marzec' },
    { value: 4, label: 'Kwiecień' }, { value: 5, label: 'Maj' }, { value: 6, label: 'Czerwiec' },
    { value: 7, label: 'Lipiec' }, { value: 8, label: 'Sierpień' }, { value: 9, label: 'Wrzesień' },
    { value: 10, label: 'Październik' }, { value: 11, label: 'Listopad' }, { value: 12, label: 'Grudzień' }
  ];

  generateForm = this.fb.group({
    employeeId: [null as number | null, Validators.required],
    month: [this.currentMonth, Validators.required],
    year: [this.currentYear, Validators.required]
  });

  filterForm = this.fb.group({
    month: [this.currentMonth],
    year: [this.currentYear]
  });

  ngOnInit(): void {
    if(this.businessId) {
        this.loadEmployees();
        this.loadPayrolls();
    }
  }

  loadEmployees() {
    this.employeeService.getEmployees(this.businessId).subscribe(data => this.employees = data);
  }

  loadPayrolls() {
    this.isLoading = true;
    const { month, year } = this.filterForm.value;
    
    this.hrService.getPayrolls(this.businessId, month || undefined, year || undefined)
        .pipe(finalize(() => this.isLoading = false))
        .subscribe({
            next: (data) => this.payrolls = data,
            error: () => this.toastr.error('Błąd ładowania list płac.')
        });
  }

  onGenerate() {
    if (this.generateForm.invalid) return;

    this.isGenerating = true;
    const formValue = this.generateForm.value;

    this.hrService.generatePayroll(this.businessId, {
        employeeId: Number(formValue.employeeId),
        month: Number(formValue.month),
        year: Number(formValue.year)
    })
    .pipe(finalize(() => this.isGenerating = false))
    .subscribe({
        next: (newPayroll) => {
            this.toastr.success('Lista płac wygenerowana!');
            this.loadPayrolls();
        },
        error: (err) => this.toastr.error('Błąd generowania. Sprawdź czy pracownik ma umowę.')
    });
  }

  getStatusLabel(status: PayrollStatus): string {
    const labels = ['Szkic', 'Przeliczone', 'Zatwierdzone', 'Opłacone'];
    return labels[status] || 'Nieznany';
  }
}
