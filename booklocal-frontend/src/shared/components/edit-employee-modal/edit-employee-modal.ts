import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { EmployeeService } from '../../../core/services/employee-service';
import { Employee } from '../../../types/business.model';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-edit-employee-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './edit-employee-modal.html',
})
export class EditEmployeeModalComponent implements OnChanges {
  @Input() employee: Employee | null = null;
  businessId = input.required<number>();
  @Output() closed = new EventEmitter<boolean>();

  private fb = inject(FormBuilder);
  private employeeService = inject(EmployeeService);
  private toastr = inject(ToastrService);

  employeeForm = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    dateOfBirth: ['', Validators.required],
    position: [''],
    bio: [''],
    specialization: [''],
    instagramProfileUrl: [''],
    portfolioUrl: ['']
  });
  
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['employee'] && this.employee) {
      this.employeeForm.patchValue(this.employee);
    }
  }

  onSubmit() {
    if (this.employeeForm.invalid || !this.businessId() || !this.employee) return;

    this.employeeService.updateEmployee(this.businessId(), this.employee.id, this.employeeForm.value as any)
      .subscribe({
        next: () => {
          this.toastr.success('Dane pracownika zaktualizowane!');
          this.closed.emit(true);
        },
        error: (err) => this.toastr.error(`Błąd: ${err.error.title || 'Sprawdź dane.'}`)
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