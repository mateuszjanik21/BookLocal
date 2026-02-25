import { Component, Input, Output, EventEmitter, ViewChild, ElementRef, inject, OnInit, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HRService } from '../../../../core/services/hr-service';
import { EmploymentContract, ContractType, EmploymentContractUpsert } from '../../../../types/hr.models';
import { Employee } from '../../../../types/business.model';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs';

interface WizardForm {
  contractType: ContractType;
  baseSalary: number;
  hourlyRate: number;
  taxDeductibleExpenses: number;
  startDate: string;
  endDate: string;
  isIndefinite: boolean;
}

@Component({
  selector: 'app-contract-wizard-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './contract-wizard-modal.html',
})
export class ContractWizardModalComponent implements OnInit, OnChanges {
  @Input() businessId!: number;
  @Input() employees: Employee[] = [];
  @Input() existingContracts: EmploymentContract[] = [];
  @Input() preselectedEmployeeId?: number;

  @Output() closed = new EventEmitter<boolean>();

  @ViewChild('wizardDialog') dialogRef!: ElementRef<HTMLDialogElement>;

  private hrService = inject(HRService);
  private toastr = inject(ToastrService);

  currentStep = 1;
  isLoading = false;
  isSubmitting = false;
  salaryMode: 'monthly' | 'hourly' = 'monthly';

  selectedEmployeeId: number | null = null;

  form: WizardForm = {
    contractType: ContractType.EmploymentContract,
    baseSalary: 0,
    hourlyRate: 0,
    taxDeductibleExpenses: 250,
    startDate: '',
    endDate: '',
    isIndefinite: true,
  };

  contractTypes = [
    { value: ContractType.EmploymentContract, label: 'Umowa o Pracę', hint: 'ZUS pełny' },
    { value: ContractType.B2B,                label: 'B2B',           hint: 'Własna dz.' },
    { value: ContractType.MandateContract,    label: 'Zlecenie',      hint: 'ZUS częśc.' },
    { value: ContractType.Apprenticeship,     label: 'Praktyki',      hint: 'Bez ZUS' },
  ];

  ngOnInit() {}

  ngOnChanges() {
    if (this.preselectedEmployeeId) {
      this.selectedEmployeeId = this.preselectedEmployeeId;
    }
  }

  open() {
    this.resetWizard();
    if (this.preselectedEmployeeId) {
      this.selectedEmployeeId = this.preselectedEmployeeId;
      this.currentStep = 2;
    }
    this.dialogRef.nativeElement.showModal();
  }

  close() {
    this.dialogRef.nativeElement.close();
    this.closed.emit(false);
  }

  resetWizard() {
    this.currentStep = 1;
    this.selectedEmployeeId = null;
    this.salaryMode = 'monthly';
    this.form = {
      contractType: ContractType.EmploymentContract,
      baseSalary: 0,
      hourlyRate: 0,
      taxDeductibleExpenses: 250,
      startDate: new Date().toISOString().split('T')[0],
      endDate: '',
      isIndefinite: true,
    };
  }

  selectEmployee(id: number) {
    this.selectedEmployeeId = id;
  }

  selectContractType(type: ContractType) {
    this.form.contractType = type;
    if (type === ContractType.B2B || type === ContractType.Apprenticeship) {
      this.form.taxDeductibleExpenses = 0;
    } else {
      this.form.taxDeductibleExpenses = 250;
    }
  }

  setSalaryMode(mode: 'monthly' | 'hourly') {
    this.salaryMode = mode;
    if (mode === 'hourly' && this.form.baseSalary > 0) {
      this.form.hourlyRate = Math.round((this.form.baseSalary / 168) * 100) / 100;
    }
    if (mode === 'monthly' && this.form.hourlyRate > 0) {
      this.form.baseSalary = Math.round(this.form.hourlyRate * 168 * 100) / 100;
    }
  }

  recalcFromHourly() {
    this.form.baseSalary = Math.round(this.form.hourlyRate * 168 * 100) / 100;
  }

  get isApprentice(): boolean {
    return this.form.contractType === ContractType.Apprenticeship;
  }

  canProceed(): boolean {
    if (this.currentStep === 1) return this.selectedEmployeeId !== null;
    if (this.currentStep === 2) return this.isApprentice || this.form.baseSalary > 0;
    return true;
  }

  canSubmit(): boolean {
    const salaryOk = this.isApprentice || this.form.baseSalary > 0;
    return !!this.form.startDate && salaryOk && this.selectedEmployeeId !== null;
  }

  getEmployeePhoto(id: number | null): string | null {
    if (!id) return null;
    const emp = this.employees.find(e => e.id === id);
    return emp?.photoUrl ?? null;
  }

  nextStep() {
    if (this.canProceed() && this.currentStep < 3) this.currentStep++;
  }

  prevStep() {
    if (this.currentStep > 1) this.currentStep--;
  }

  getActiveContract(employeeId: number): EmploymentContract | undefined {
    return this.existingContracts.find(c => c.employeeId === employeeId && c.isActive);
  }

  getContractTypeLabel(type: ContractType | number | string): string {
    if (typeof type === 'string') {
      const typeMap: Record<string, number> = {
        'EmploymentContract': ContractType.EmploymentContract,
        'B2B':                ContractType.B2B,
        'MandateContract':    ContractType.MandateContract,
        'Apprenticeship':     ContractType.Apprenticeship,
      };
      type = typeMap[type] ?? type;
    }
    const found = this.contractTypes.find(t => t.value == type);
    return found ? found.label : String(type);
  }

  getEmployeeName(id: number | null): string {
    if (!id) return '—';
    const emp = this.employees.find(e => e.id === id);
    return emp ? `${emp.firstName} ${emp.lastName}` : '—';
  }

  submit() {
    if (!this.canSubmit() || !this.selectedEmployeeId) return;
    this.isSubmitting = true;

    const payload: EmploymentContractUpsert = {
      employeeId: this.selectedEmployeeId,
      contractType: this.form.contractType,
      baseSalary: this.form.baseSalary,
      taxDeductibleExpenses: this.form.taxDeductibleExpenses,
      startDate: this.form.startDate,
      endDate: this.form.isIndefinite ? undefined : (this.form.endDate || undefined),
    };

    this.hrService.createContract(this.businessId, payload)
      .pipe(finalize(() => this.isSubmitting = false))
      .subscribe({
        next: () => {
          this.toastr.success('Umowa została utworzona!');
          this.dialogRef.nativeElement.close();
          this.closed.emit(true);
        },
        error: () => this.toastr.error('Błąd tworzenia umowy.')
      });
  }
}
