import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BusinessService } from '../../../core/services/business-service';
import { EmploymentContractManagerComponent } from './employment-contract-manager/employment-contract-manager';
import { PayrollGeneratorComponent } from './payroll-generator/payroll-generator';
import { BusinessDetail } from '../../../types/business.model';

@Component({
  selector: 'app-manage-hr',
  standalone: true,
  imports: [CommonModule, EmploymentContractManagerComponent, PayrollGeneratorComponent],
  templateUrl: './manage-hr.html',
})
export class ManageHrComponent implements OnInit {
  private businessService = inject(BusinessService);
  business: BusinessDetail | null = null;

  ngOnInit() {
    this.businessService.getMyBusiness().subscribe(b => this.business = b);
  }
}
