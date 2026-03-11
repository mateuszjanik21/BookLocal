import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BusinessService } from '../../../core/services/business-service';
import { HRService } from '../../../core/services/hr-service';
import { EmploymentContractManagerComponent } from './employment-contract-manager/employment-contract-manager';
import { PayrollGeneratorComponent } from './payroll-generator/payroll-generator';
import { MonthlySummaryComponent } from './monthly-summary/monthly-summary';
import { BusinessDetail, Employee } from '../../../types/business.model';

@Component({
  selector: 'app-manage-hr',
  standalone: true,
  imports: [CommonModule, EmploymentContractManagerComponent, PayrollGeneratorComponent, MonthlySummaryComponent],
  templateUrl: './manage-hr.html',
})
export class ManageHrComponent implements OnInit {
  private businessService = inject(BusinessService);
  private hrService = inject(HRService);
  
  business: BusinessDetail | null = null;
  employees: Employee[] = [];
  dataLoaded = false;

  ngOnInit() {
    this.businessService.getMyBusiness().subscribe(b => {
      this.business = b;
      this.hrService.getEmployeesForHr(b.id).subscribe(emps => {
        this.employees = emps;
        setTimeout(() => this.dataLoaded = true, 300);
      });
    });
  }
}
