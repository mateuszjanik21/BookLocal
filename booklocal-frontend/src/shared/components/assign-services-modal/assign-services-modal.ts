import { Component, Input, OnChanges, SimpleChanges, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, FormControl, ReactiveFormsModule } from '@angular/forms';
import { Service, Employee } from '../../../types/business.model';
import { EmployeeService } from '../../../core/services/employee-service';

@Component({
  selector: 'app-assign-services-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './assign-services-modal.html',
})
export class AssignServicesModalComponent implements OnChanges {
  @Input() employee: Employee | null = null;
  @Input() allServices: Service[] = [];
  @Input() businessId: number | null = null;

  private fb = inject(FormBuilder);
  private employeeService = inject(EmployeeService);

  servicesForm = this.fb.group({
    services: this.fb.array<FormControl>([])
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (this.employee && this.businessId && this.allServices.length > 0) {
      this.employeeService.getAssignedServiceIds(this.businessId, this.employee.id).subscribe(assignedIds => {

        const servicesFormArray = this.servicesForm.get('services') as FormArray;
        servicesFormArray.clear();

        this.allServices.forEach(service => {
          const isAssigned = assignedIds.includes(service.id);
          servicesFormArray.push(new FormControl(isAssigned));
        });
      });
    }
  }

  onSubmit() {
    if (!this.employee || !this.businessId) return;

    const selectedServiceIds = this.servicesForm.value.services
      ?.map((checked, i) => checked ? this.allServices[i].id : null)
      .filter(id => id !== null) as number[];

    this.employeeService.assignServices(this.businessId, this.employee.id, selectedServiceIds).subscribe(() => {
      alert('Zaktualizowano usługi pracownika!');
    });
  }
}