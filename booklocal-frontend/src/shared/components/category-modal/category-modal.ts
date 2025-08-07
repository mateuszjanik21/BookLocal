import { Component, EventEmitter, Input, OnInit, Output, inject, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ServiceCategory } from '../../../types/business.model';
import { ImageUploadComponent } from '../image-upload/image-upload';

@Component({
  selector: 'app-category-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ImageUploadComponent],
  templateUrl: './category-modal.html',
})
export class CategoryModalComponent implements OnInit {
  @Input() category: ServiceCategory | null = null;
  @Output() closed = new EventEmitter<void>();
  @Output() save = new EventEmitter<{ payload: { name: string }, file: File | null }>();

  businessId = input.required<number>();
  
  private fb = inject(FormBuilder);

  categoryForm = this.fb.group({ name: ['', Validators.required] });
  selectedFile: File | null = null;
  isEditMode = false;
  
  @Input() isLoading = false;

  ngOnInit(): void {
    if (this.category) {
      this.isEditMode = true;
      this.categoryForm.patchValue(this.category);
    }
  }

  onImageSelected(file: File | null) {
    this.selectedFile = file;
  }

  onSubmit() {
    if (this.categoryForm.invalid) return;
    const payload = { name: this.categoryForm.value.name! };
    this.save.emit({ payload, file: this.selectedFile }); 
  } 
}