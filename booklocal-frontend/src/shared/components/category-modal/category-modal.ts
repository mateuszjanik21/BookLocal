import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ServiceCategory, MainCategory } from '../../../types/business.model';
import { ImageUploadComponent } from '../image-upload/image-upload';
import { CategoryService } from '../../../core/services/category';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-category-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ImageUploadComponent],
  templateUrl: './category-modal.html',
})
export class CategoryModalComponent implements OnInit {
  @Input() category: ServiceCategory | null = null;
  @Input() businessId!: number;
  @Output() closed = new EventEmitter<void>();
  @Output() save = new EventEmitter<{ payload: { name: string }, file: File | null }>();
  @Input() isVisible: boolean = false;
  
  private fb = inject(FormBuilder);
  private categoryService = inject(CategoryService);

  mainCategories$!: Observable<MainCategory[]>;

  categoryForm = this.fb.group({
    name: ['', Validators.required],
    mainCategoryId: [null as number | null, Validators.required]
  });
  selectedFile: File | null = null;
  isEditMode = false;
  
  @Input() isLoading = false;

  ngOnInit(): void {
    this.mainCategories$ = this.categoryService.getMainCategories();

    if (this.category) {
      this.isEditMode = true;
      this.categoryForm.patchValue(this.category as any);
    }
  }

  onImageSelected(file: File | null) {
    this.selectedFile = file;
  }

  onSubmit() {
    if (this.categoryForm.invalid) return;
    const payload = {
      name: this.categoryForm.value.name!,
      mainCategoryId: this.categoryForm.value.mainCategoryId!
    };
    this.save.emit({ payload, file: this.selectedFile }); 
  } 
}