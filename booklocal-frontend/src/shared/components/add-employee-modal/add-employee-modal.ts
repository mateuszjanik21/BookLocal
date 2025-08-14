import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { finalize, of, switchMap } from 'rxjs';

import { EmployeeService } from '../../../core/services/employee-service';
import { PhotoService } from '../../../core/services/photo';
import { EmployeePayload } from '../../../types/employee.models';
import { Employee } from '../../../types/business.model';

@Component({
  selector: 'app-add-employee-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './add-employee-modal.html',
  styleUrl: './add-employee-modal.css'
})
export class AddEmployeeModalComponent {
  @Input() businessId!: number;
  @Output() closed = new EventEmitter<boolean>();

  private fb = inject(FormBuilder);
  private employeeService = inject(EmployeeService);
  private photoService = inject(PhotoService);
  private toastr = inject(ToastrService);

  employeeForm: FormGroup;
  photoPreview: string | ArrayBuffer | null = null;
  isSubmitting = false;

  constructor() {
    this.employeeForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      position: [''],
      photo: [null as File | null]
    });
  }

  onFileSelected(event: Event): void {
    const target = event.target as HTMLInputElement;
    const file = target.files?.[0];

    if (file) {
      this.employeeForm.patchValue({ photo: file });
      const reader = new FileReader();
      reader.onload = () => this.photoPreview = reader.result;
      reader.readAsDataURL(file);
    }
  }

  onSubmit() {
    if (this.employeeForm.invalid) return;

    this.isSubmitting = true;

    const employeePayload: EmployeePayload = {
      firstName: this.employeeForm.get('firstName')?.value,
      lastName: this.employeeForm.get('lastName')?.value,
      position: this.employeeForm.get('position')?.value || '',
    };
    
    const photoFile = this.employeeForm.get('photo')?.value as File | null;

    this.employeeService.addEmployee(this.businessId, employeePayload).pipe(
      switchMap((newEmployee: Employee) => {
        if (photoFile && newEmployee) {
          return this.photoService.uploadEmployeePhoto(newEmployee.id, photoFile);
        }
        return of(null); 
      }),
      finalize(() => this.isSubmitting = false)
    ).subscribe({
      next: () => {
        this.toastr.success('Pracownik dodany pomyślnie!');
        this.closed.emit(true);
      },
      error: (err) => {
        const errorMessage = err?.error?.title || 'Wystąpił błąd. Spróbuj ponownie.';
        this.toastr.error(errorMessage);
      }
    });
  }

  closeModal(event: MouseEvent) {
    if ((event.target as HTMLElement).classList.contains('modal')) {
      this.closed.emit(false);
    }
  }
}