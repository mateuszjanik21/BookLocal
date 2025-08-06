import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../core/services/auth-service';
import { UserDto } from '../../types/auth.models';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { PhotoService } from '../../core/services/photo';
import { ImageUploadComponent } from '../../shared/components/image-upload/image-upload';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ImageUploadComponent],
  templateUrl: './profile.html',
})
export class ProfileComponent {
  private authService = inject(AuthService);
  private photoService = inject(PhotoService);
  private fb = inject(FormBuilder);
  private toastr = inject(ToastrService);

  user$: Observable<UserDto | null> = this.authService.currentUser$;
  isUploading = false;

  passwordForm = this.fb.group({
    currentPassword: ['', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(6)]]
  });

  onSubmit(): void {
    if (this.passwordForm.invalid) return;

    this.authService.changePassword(this.passwordForm.value as any).subscribe({
      next: () => {
        this.toastr.success('Hasło zostało zmienione!', 'Sukces');
        this.passwordForm.reset();
      },
      error: (err) => {
        this.toastr.error('Nie udało się zmienić hasła. Sprawdź obecne hasło.', 'Błąd');
      }
    });
  }

  onPhotoSelected(file: File | null): void {
    if (!file) return;

    this.isUploading = true;
    this.photoService.uploadUserProfilePhoto(file).subscribe({
      next: (response) => {
        this.toastr.success('Zdjęcie profilowe zaktualizowane!');
        this.authService.updateUserProfilePhoto(response.photoUrl);
        this.isUploading = false;
      },
      error: (err) => {
        this.toastr.error('Błąd podczas wgrywania zdjęcia.');
        this.isUploading = false;
      }
    });
  }
}