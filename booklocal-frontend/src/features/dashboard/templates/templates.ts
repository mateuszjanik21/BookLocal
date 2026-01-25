import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BusinessService } from '../../../core/services/business-service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-templates',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './templates.html'
})
export class TemplatesComponent {
  private businessService = inject(BusinessService);
  private toastr = inject(ToastrService);

  isUploading = false;
  selectedFile: File | null = null;

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) {
      if (!file.name.endsWith('.docx')) {
        this.toastr.error('Dozwolone są tylko pliki .docx');
        return;
      }
      this.selectedFile = file;
    }
  }

  uploadTemplate(templateName: string) {
    if (!this.selectedFile) return;

    this.isUploading = true;
    const formData = new FormData();
    formData.append('File', this.selectedFile);
    formData.append('TemplateName', templateName);

    this.businessService.uploadTemplate(formData).subscribe({
      next: () => {
        this.toastr.success(`Szablon ${templateName} wgrany pomyślnie.`);
        this.isUploading = false;
        this.selectedFile = null;
      },
      error: () => {
        this.toastr.error('Błąd podczas wgrywania szablonu.');
        this.isUploading = false;
      }
    });
  }
}
