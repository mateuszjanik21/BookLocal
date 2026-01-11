import { Component, Input, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HRService } from '../../../../core/services/hr-service';
import { EmployeeService } from '../../../../core/services/employee-service';
import { EmploymentContract, ContractType } from '../../../../types/hr.models';
import { Employee } from '../../../../types/business.model';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-employment-contract-manager',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './employment-contract-manager.html',
})
export class EmploymentContractManagerComponent implements OnInit {
  @Input() businessId!: number;
  
  private fb = inject(FormBuilder);
  private hrService = inject(HRService);
  private employeeService = inject(EmployeeService);
  private toastr = inject(ToastrService);

  contracts: EmploymentContract[] = [];
  employees: Employee[] = [];
  isLoading = false;
  isSubmitting = false;

  contractTypes = [
    { value: ContractType.EmploymentContract, label: 'Umowa o Pracę' },
    { value: ContractType.B2B, label: 'B2B' },
    { value: ContractType.MandateContract, label: 'Umowa Zlecenie' },
    { value: ContractType.Apprenticeship, label: 'Praktyki' }
  ];

  contractForm = this.fb.group({
    employeeId: [null as number | null, Validators.required],
    contractType: [ContractType.EmploymentContract, Validators.required],
    baseSalary: [null, [Validators.required, Validators.min(0)]],
    taxDeductibleExpenses: [250, [Validators.required, Validators.min(0)]],
    startDate: ['', Validators.required],
    endDate: ['']
  });

  ngOnInit(): void {
    if(this.businessId) {
        this.loadData();
    }
  }

  loadData() {
    this.isLoading = true;
    this.hrService.getContracts(this.businessId).subscribe({
        next: (data) => this.contracts = data,
        error: () => this.toastr.error('Błąd ładowania umów.')
    });

    this.employeeService.getEmployees(this.businessId).subscribe({
        next: (data) => this.employees = data,
        complete: () => this.isLoading = false
    });
  }

  onSubmit() {
    if (this.contractForm.invalid) return;

    this.isSubmitting = true;
    const formValue = this.contractForm.value;

    const payload = {
        employeeId: Number(formValue.employeeId),
        contractType: Number(formValue.contractType),
        baseSalary: formValue.baseSalary!,
        taxDeductibleExpenses: formValue.taxDeductibleExpenses!,
        startDate: formValue.startDate!,
        endDate: formValue.endDate || undefined
    };

    this.hrService.createContract(this.businessId, payload)
        .pipe(finalize(() => this.isSubmitting = false))
        .subscribe({
            next: (newContract) => {
                this.toastr.success('Umowa dodana pomyślnie!');
                this.contracts = [...this.contracts, newContract];
                this.contractForm.reset({ contractType: ContractType.EmploymentContract });
                this.loadData();
            },
            error: (err) => this.toastr.error('Błąd dodawania umowy.')
        });
  }

  getContractTypeLabel(type: ContractType | number | string): string {
    let val = type;
    if (typeof type === 'string') {
        const typeMap: Record<string, number> = {
            'EmploymentContract': ContractType.EmploymentContract,
            'B2B': ContractType.B2B,
            'MandateContract': ContractType.MandateContract,
            'Apprenticeship': ContractType.Apprenticeship
        };
        if (type in typeMap) {
            val = typeMap[type];
        }
    }
    const found = this.contractTypes.find(t => t.value == val); 
    return found ? found.label : `Nieznany (${type})`;
  }
}
