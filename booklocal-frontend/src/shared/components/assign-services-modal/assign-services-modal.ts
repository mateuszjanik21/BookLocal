import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Service, Employee, ServiceCategory } from '../../../types/business.model';
import { EmployeeService } from '../../../core/services/employee-service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-assign-services-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './assign-services-modal.html',
})
export class AssignServicesModalComponent implements OnInit {
  @Input() categories: ServiceCategory[] = [];
  @Input() employee: Employee | null = null;
  @Input() businessId: number | null = null;
  @Output() closed = new EventEmitter<boolean>();
  
  private fb = inject(FormBuilder);
  private employeeService = inject(EmployeeService);
  private toastr = inject(ToastrService);

  public activeCategoryId: number | null = null;

  private allServices: Service[] = [];

  isFormReady = false;
  public servicesForm: FormGroup<{
    services: FormArray<FormControl<boolean | null>>;
  }>;

  constructor() {
    this.servicesForm = this.fb.group({
      services: this.fb.array<FormControl<boolean | null>>([])
    });
  }

  ngOnInit(): void {
    if (!this.employee || !this.businessId || this.categories.length === 0) {
      return;
    }

    this.allServices = this.categories.flatMap(category => category.services);

    this.employeeService.getAssignedServiceIds(this.businessId, this.employee.id).subscribe({
      next: (assignedIds) => {
        const servicesFormArray = this.servicesForm.get('services') as FormArray;
        servicesFormArray.clear();

        this.allServices.forEach(service => {
          const isAssigned = assignedIds.includes(service.id);
          servicesFormArray.push(new FormControl(isAssigned));
        });

        this.isFormReady = true;
      },
      error: () => {
        this.toastr.error('Nie udało się załadować przypisanych usług.');
      }
    });
  }
  
  selectCategory(categoryId: number | null): void {
    this.activeCategoryId = categoryId;
  }

  getFormControlIndex(serviceId: number): number {
    return this.allServices.findIndex(s => s.id === serviceId);
  }

  onSubmit() {
    if (!this.employee || !this.businessId) return;

    const selectedServiceIds = this.servicesForm.value.services
      ?.map((checked, i) => checked ? this.allServices[i].id : null)
      .filter(id => id !== null) as number[];

    this.employeeService.assignServices(this.businessId, this.employee.id, selectedServiceIds).subscribe({
      next: () => {
        this.toastr.success('Zaktualizowano usługi pracownika!');
        this.closed.emit(true);
      },
      error: () => this.toastr.error('Wystąpił błąd podczas zapisywania.')
    });
  }

  cancel() {
    this.closed.emit(false);
  }

  onBackdropClick(event: MouseEvent) {
    if ((event.target as HTMLElement).classList.contains('modal')) {
      this.closed.emit(false);
    }
  }
}