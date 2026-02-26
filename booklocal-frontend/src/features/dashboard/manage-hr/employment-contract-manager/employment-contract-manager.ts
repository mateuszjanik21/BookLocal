import { Component, Input, ViewChild, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HRService } from '../../../../core/services/hr-service';
import { EmploymentContract, ContractType } from '../../../../types/hr.models';
import { Employee } from '../../../../types/business.model';
import { ToastrService } from 'ngx-toastr';
import { ContractWizardModalComponent } from '../contract-wizard-modal/contract-wizard-modal';

@Component({
  selector: 'app-employment-contract-manager',
  standalone: true,
  imports: [CommonModule, RouterModule, ContractWizardModalComponent, FormsModule],
  templateUrl: './employment-contract-manager.html',
})
export class EmploymentContractManagerComponent implements OnInit {
  @Input() businessId!: number;
  @ViewChild('wizardModal') wizardModal!: ContractWizardModalComponent;

  private hrService      = inject(HRService);
  private toastr         = inject(ToastrService);

  allContracts: EmploymentContract[] = [];
  employees:    Employee[]           = [];
  isLoading     = false;
  isArchiving   = false;
  showArchived  = false;

  contractTypes = [
    { value: ContractType.EmploymentContract, label: 'Umowa o Pracę' },
    { value: ContractType.B2B,                label: 'B2B'           },
    { value: ContractType.MandateContract,    label: 'Umowa Zlecenie' },
    { value: ContractType.Apprenticeship,     label: 'Praktyki'       },
  ];

  searchQuery = '';

  get displayedContracts(): EmploymentContract[] {
    let filtered = this.showArchived
      ? this.allContracts
      : this.allContracts.filter(c => c.isActive);

    if (this.searchQuery.trim()) {
      const q = this.searchQuery.toLowerCase().trim();
      filtered = filtered.filter(c => c.employeeName.toLowerCase().includes(q));
    }

    return filtered;
  }

  get archivedCount(): number {
    return this.allContracts.filter(c => !c.isActive).length;
  }

  ngOnInit(): void {
    if (this.businessId) this.loadData();
  }

  loadData() {
    this.isLoading = true;
    this.hrService.getContracts(this.businessId).subscribe({
      next: (data) => { this.allContracts = data; this.isLoading = false; },
      error: () => { this.toastr.error('Błąd ładowania umów.'); this.isLoading = false; }
    });
    this.hrService.getEmployeesForHr(this.businessId).subscribe({
      next: (data) => this.employees = data,
    });
  }

  openWizard() { this.wizardModal.open(); }

  onWizardClosed(refresh: boolean) { if (refresh) this.loadData(); }

  editContract(event: Event, contract: EmploymentContract) {
    event.stopPropagation();
    event.preventDefault();
    this.wizardModal.open(contract);
  }

  archiveContract(event: Event, contractId: number) {
    event.stopPropagation();
    event.preventDefault();
    if (!confirm('Czy na pewno chcesz zarchiwizować tę umowę?')) return;
    this.isArchiving = true;
    this.hrService.archiveContract(this.businessId, contractId).subscribe({
      next: () => { this.toastr.success('Umowa zarchiwizowana.'); this.loadData(); this.isArchiving = false; },
      error: () => { this.toastr.error('Błąd archiwizacji umowy.'); this.isArchiving = false; }
    });
  }

  getContractTypeLabel(type: ContractType | number | string): string {
    let val = type;
    if (typeof type === 'string') {
      const typeMap: Record<string, number> = {
        'EmploymentContract': ContractType.EmploymentContract,
        'B2B':                ContractType.B2B,
        'MandateContract':    ContractType.MandateContract,
        'Apprenticeship':     ContractType.Apprenticeship
      };
      if (type in typeMap) val = typeMap[type];
    }
    const found = this.contractTypes.find(t => t.value == val);
    return found ? found.label : `Nieznany (${type})`;
  }
}
