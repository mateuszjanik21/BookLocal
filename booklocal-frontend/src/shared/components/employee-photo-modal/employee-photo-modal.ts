import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Employee } from '../../../types/business.model';
import { PhotoService } from '../../../core/services/photo';
import { ToastrService } from 'ngx-toastr';
import { ImageUploadComponent } from '../image-upload/image-upload';

@Component({
  selector: 'app-employee-photo-modal',
  standalone: true,
  imports: [CommonModule, ImageUploadComponent],
  templateUrl: './employee-photo-modal.html',
})
export class EmployeePhotoModalComponent {
  @Input() employee: Employee | null = null;
  @Output() closed = new EventEmitter<string | void>();

  private photoService = inject(PhotoService);
  private toastr = inject(ToastrService);

  isLoading = false;

  onPhotoSelected(file: File): void {
    if (!this.employee) return;

    this.isLoading = true;
    this.photoService.uploadEmployeePhoto(this.employee.id, file).subscribe({
      next: (response) => {
        this.toastr.success('Zdjęcie pracownika zaktualizowane!');
        this.closed.emit(response.photoUrl);
        this.isLoading = false; 
      },
      error: () => {
        this.toastr.error('Błąd podczas wgrywania zdjęcia.');
        this.isLoading = false;
      }
    });
  }
}