import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/services/auth-service';
import { PhotoService } from '../../../core/services/photo';
import { BusinessService } from '../../../core/services/business-service';
import { UserDto } from '../../../types/auth.models';
import { BusinessDetail, Employee } from '../../../types/business.model';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { ImageUploadComponent } from '../../../shared/components/image-upload/image-upload';

@Component({
  selector: 'app-manage-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ImageUploadComponent],
  templateUrl: './manage-profile.html',
})
export class ManageProfileComponent implements OnInit {
  authService = inject(AuthService);
  photoService = inject(PhotoService);
  businessService = inject(BusinessService);
  toastr = inject(ToastrService);
  private fb = inject(FormBuilder);

  user$: Observable<UserDto | null> = this.authService.currentUser$;
  business: BusinessDetail | null = null;
  ownerAsEmployee: Employee | null = null;

  isUploadingProfilePhoto = false;
  isUploadingBusinessPhoto = false;

  passwordForm = this.fb.group({
    currentPassword: ['', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(6)]]
  });

  ngOnInit(): void {
    this.businessService.getMyBusiness().subscribe(data => {
      this.business = data;
      const user = this.authService.currentUserValue;
      
      if (user && data.employees) {
        this.ownerAsEmployee = data.employees.find(e => e.id.toString() === user.id) || null;
      }
    });
  }

  onChangePassword(): void {
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

  onProfilePhotoSelected(file: File | null): void {
    if (!file) return;

    this.isUploadingProfilePhoto = true;
    this.photoService.uploadUserProfilePhoto(file).subscribe({
      next: (response) => {
        this.toastr.success('Zdjęcie profilowe zaktualizowane!');
        this.authService.updateUserProfilePhoto(response.photoUrl);
        if(this.ownerAsEmployee) {
          this.ownerAsEmployee.photoUrl = response.photoUrl;
        }
        this.isUploadingProfilePhoto = false;
      },
      error: () => {
        this.toastr.error('Błąd podczas wgrywania zdjęcia.');
        this.isUploadingProfilePhoto = false;
      }
    });
  }

  onBusinessPhotoSelected(file: File | null): void {
    if (!file) return;

    this.isUploadingBusinessPhoto = true;
    this.photoService.uploadBusinessPhoto(file).subscribe({
      next: (response) => {
        this.toastr.success('Zdjęcie firmy zaktualizowane!');
        if (this.business) {
          this.business.photoUrl = response.photoUrl;
        }
        this.isUploadingBusinessPhoto = false;
      },
      error: () => {
        this.toastr.error('Błąd podczas wgrywania zdjęcia.');
        this.isUploadingBusinessPhoto = false;
      }
    });
  }
}