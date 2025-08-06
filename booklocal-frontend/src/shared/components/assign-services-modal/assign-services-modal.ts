import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Service, Employee } from '../../../types/business.model';
import { EmployeeService } from '../../../core/services/employee-service';
import { ToastrService } from 'ngx-toastr';

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
  @Output() closed = new EventEmitter<void>();
  
  private fb = inject(FormBuilder);
  private employeeService = inject(EmployeeService);
  private toastr = inject(ToastrService);

  isFormReady = false;

  public servicesForm: FormGroup<{
    services: FormArray<FormControl<boolean | null>>;
  }>;

  constructor() {
    this.servicesForm = this.fb.group({
      services: this.fb.array<FormControl<boolean | null>>([])
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    // POPRAWKA: Sprawdzamy, czy `@Input() employee` faktycznie się zmienił i ma nową wartość.
    // To zapobiega ponownemu uruchomieniu logiki przy każdym cyklu detekcji zmian w Angularze.
    if (changes['employee'] && changes['employee'].currentValue) {
      
      // Dodatkowo upewniamy się, że pozostałe dane są dostępne
      if (this.businessId && this.allServices.length > 0) {
        
        this.isFormReady = false;

        this.employeeService.getAssignedServiceIds(this.businessId, this.employee!.id).subscribe(assignedIds => {
          const servicesFormArray = this.servicesForm.get('services') as FormArray;
          servicesFormArray.clear();

          this.allServices.forEach(service => {
            const isAssigned = assignedIds.includes(service.id);
            servicesFormArray.push(new FormControl(isAssigned));
          });

          this.isFormReady = true;
        });
      }
    }
  }

  onSubmit() {
    if (!this.employee || !this.businessId) return;

    const selectedServiceIds = this.servicesForm.value.services
      ?.map((checked, i) => checked ? this.allServices[i].id : null)
      .filter(id => id !== null) as number[];

    this.employeeService.assignServices(this.businessId, this.employee.id, selectedServiceIds).subscribe({
      next: () => {
        this.toastr.success('Zaktualizowano usługi pracownika!');
        this.closed.emit();
      },
      error: (err) => this.toastr.error('Wystąpił błąd podczas zapisywania.')
    });
  }
}