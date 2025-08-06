import { Component, EventEmitter, Input, Output, SimpleChanges, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-image-upload',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './image-upload.html',
})
export class ImageUploadComponent {
  @Input() currentImageUrl: string | null | undefined = null;
  @Input() isLoading = false;
  @Input() showUploadButton: boolean = true;
  @Input() displayStyle: 'circle' | 'rectangle' = 'circle';
  
  @Output() imageSelected = new EventEmitter<File | null>();

  previewUrl: string | ArrayBuffer | null = null;
  selectedFile: File | null = null;
  
  private toastr = inject(ToastrService); 

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['currentImageUrl'] && !changes['currentImageUrl'].firstChange) {
      this.selectedFile = null;
      this.previewUrl = null;
    }
  }

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      if (!file.type.startsWith('image/')) {
        this.toastr.error('Proszę wybrać plik graficzny.');
        return;
      }
      this.selectedFile = file;
      const reader = new FileReader();
      reader.onload = () => this.previewUrl = reader.result;
      reader.readAsDataURL(file);
      
      if (!this.showUploadButton) {
        this.imageSelected.emit(this.selectedFile);
      }
    }
  }

  onUpload(): void {
    if (this.selectedFile) {
      this.imageSelected.emit(this.selectedFile);
    }
  }
}