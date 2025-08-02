import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { EmployeeService } from '../../../core/services/employee-service';

@Component({
  selector: 'app-add-employee-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './add-employee-modal.html'
})
export class AddEmployeeModalComponent {
  @Input() businessId: number | null = null;
  @Output() employeeAdded = new EventEmitter<void>();

  private fb = inject(FormBuilder);
  private employeeService = inject(EmployeeService);

  employeeForm = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    position: ['']
  });

  onSubmit(modal: any) {
    if (this.employeeForm.invalid || !this.businessId) return;

    this.employeeService.addEmployee(this.businessId, this.employeeForm.value as any)
      .subscribe({
        next: () => {
          alert('Pracownik dodany pomyślnie!');
          this.employeeAdded.emit();
          this.employeeForm.reset();
          modal.close();
        },
        error: (err) => alert(`Błąd: ${err.error.title}`)
      });
  }
}