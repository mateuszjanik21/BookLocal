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
  isStudent: boolean;
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
  contractToEdit: EmploymentContract | null = null;

  form: WizardForm = {
    contractType: ContractType.EmploymentContract,
    baseSalary: 0,
    hourlyRate: 0,
    taxDeductibleExpenses: 250,
    startDate: '',
    endDate: '',
    isIndefinite: true,
    isStudent: false
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

  open(contract?: EmploymentContract) {
    this.resetWizard();
    if (contract) {
      this.contractToEdit = contract;
      this.selectedEmployeeId = contract.employeeId;
      this.currentStep = 2;

      let ctype = ContractType.EmploymentContract;
      if (typeof contract.contractType === 'string') {
        const typeMap: Record<string, number> = {
          'EmploymentContract': ContractType.EmploymentContract,
          'B2B':                ContractType.B2B,
          'MandateContract':    ContractType.MandateContract,
          'Apprenticeship':     ContractType.Apprenticeship,
        };
        ctype = typeMap[contract.contractType] ?? ContractType.EmploymentContract;
      } else {
        ctype = contract.contractType;
      }

      this.form = {
        contractType: ctype,
        baseSalary: contract.baseSalary,
        hourlyRate: Math.round((contract.baseSalary / 168) * 100) / 100,
        taxDeductibleExpenses: contract.taxDeductibleExpenses,
        startDate: contract.startDate,
        endDate: contract.endDate || '',
        isIndefinite: !contract.endDate,
        isStudent: false, // will let user re-check if needed, or backend preserves it
      };
    } else if (this.preselectedEmployeeId) {
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
    this.contractToEdit = null;
    this.form = {
      contractType: ContractType.EmploymentContract,
      baseSalary: 0,
      hourlyRate: 0,
      taxDeductibleExpenses: 250,
      startDate: new Date().toISOString().split('T')[0],
      endDate: '',
      isIndefinite: true,
      isStudent: false,
    };
  }

  selectEmployee(id: number) {
    this.selectedEmployeeId = id;
    if (!this.isStudentEligible()) {
      this.form.isStudent = false;
    }
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

  isStudentEligible(): boolean {
    if (!this.selectedEmployeeId) return false;
    const emp = this.employees.find(e => e.id === this.selectedEmployeeId);
    if (!emp || !emp.dateOfBirth) return false;

    const dob = new Date(emp.dateOfBirth);
    const today = new Date();
    let age = today.getFullYear() - dob.getFullYear();
    const m = today.getMonth() - dob.getMonth();
    if (m < 0 || (m === 0 && today.getDate() < dob.getDate())) {
      age--;
    }
    return age < 26;
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
      isStudent: this.form.isStudent,
    };

    const request = this.contractToEdit
      ? this.hrService.updateContract(this.businessId, this.contractToEdit.contractId, payload)
      : this.hrService.createContract(this.businessId, payload);

    request
      .pipe(finalize(() => this.isSubmitting = false))
      .subscribe({
        next: () => {
          this.toastr.success(this.contractToEdit ? 'Umowa zaktualizowana!' : 'Umowa została utworzona!');
          this.dialogRef.nativeElement.close();
          this.closed.emit(true);
        },
        error: () => this.toastr.error(this.contractToEdit ? 'Błąd aktualizacji umowy.' : 'Błąd tworzenia umowy.')
      });
  }
}
