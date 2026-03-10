import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth-service';
import { PhotoService } from '../../../core/services/photo';
import { BusinessService } from '../../../core/services/business-service';
import { SubscriptionService } from '../../../core/services/subscription-service';
import { UserDto } from '../../../types/auth.models';
import { BusinessDetail } from '../../../types/business.model';
import { CurrentSubscription } from '../../../core/services/subscription-service';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { ImageUploadComponent } from '../../../shared/components/image-upload/image-upload';

@Component({
  selector: 'app-manage-profile',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, FormsModule, ImageUploadComponent],
  templateUrl: './manage-profile.html',
})
export class ManageProfileComponent implements OnInit {
  authService = inject(AuthService);
  photoService = inject(PhotoService);
  businessService = inject(BusinessService);
  subscriptionService = inject(SubscriptionService);
  toastr = inject(ToastrService);
  private fb = inject(FormBuilder);

  user$: Observable<UserDto | null> = this.authService.currentUser$;
  business: BusinessDetail | null = null;
  currentSubscription$: Observable<CurrentSubscription | null> = this.subscriptionService.currentSubscription$;

  isUploadingProfilePhoto = false;
  isUploadingBusinessPhoto = false;
  isSavingBusiness = false;
  isSavingProfile = false;

  isEditingProfile = false;
  editProfileForm = {
    firstName: '',
    lastName: ''
  };

  isEditingBusiness = false;
  editBusinessForm = {
    name: '',
    nip: '',
    address: '',
    city: '',
    description: ''
  };

  isPasswordModalOpen = false;
  isChangingPassword = false;
  passwordForm = this.fb.group({
    currentPassword: ['', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', Validators.required]
  });

  ngOnInit(): void {
    this.businessService.getMyBusiness().subscribe(data => {
      this.business = data;
    });
    this.subscriptionService.getCurrentSubscription().subscribe();
  }

  startEditingProfile(user: UserDto): void {
    this.editProfileForm = {
      firstName: user.firstName,
      lastName: user.lastName
    };
    this.isEditingProfile = true;
  }

  cancelEditingProfile(): void {
    this.isEditingProfile = false;
  }

  saveProfileChanges(): void {
    this.isSavingProfile = true;
    this.authService.updateProfile(this.editProfileForm).subscribe({
      next: () => {
        this.toastr.success('Dane właściciela zaktualizowane.');
        this.isEditingProfile = false;
        this.isSavingProfile = false;
      },
      error: () => {
        this.toastr.error('Nie udało się zaktualizować danych.');
        this.isSavingProfile = false;
      }
    });
  }

  startEditingBusiness(): void {
    if (!this.business) return;
    this.editBusinessForm = {
      name: this.business.name || '',
      nip: this.business.nip || '',
      address: this.business.address || '',
      city: this.business.city || '',
      description: this.business.description || ''
    };
    this.isEditingBusiness = true;
  }

  cancelEditingBusiness(): void {
    this.isEditingBusiness = false;
  }

  saveBusinessChanges(): void {
    if (!this.business) return;
    this.isSavingBusiness = true;
    this.businessService.updateBusiness(this.business.id, this.editBusinessForm).subscribe({
      next: () => {
        this.toastr.success('Dane firmy zostały zaktualizowane.');
        if (this.business) {
          this.business.name = this.editBusinessForm.name;
          this.business.nip = this.editBusinessForm.nip;
          this.business.address = this.editBusinessForm.address;
          this.business.city = this.editBusinessForm.city;
          this.business.description = this.editBusinessForm.description;
        }
        this.isEditingBusiness = false;
        this.isSavingBusiness = false;
      },
      error: () => {
        this.toastr.error('Nie udało się zaktualizować danych firmy.');
        this.isSavingBusiness = false;
      }
    });
  }

  openPasswordModal(): void {
    this.passwordForm.reset();
    this.isPasswordModalOpen = true;
  }

  closePasswordModal(): void {
    this.isPasswordModalOpen = false;
    this.passwordForm.reset();
  }

  onChangePassword(): void {
    if (this.passwordForm.invalid) return;
    const { newPassword, confirmPassword } = this.passwordForm.value;
    if (newPassword !== confirmPassword) {
      this.toastr.error('Hasła nie są identyczne.');
      return;
    }
    this.isChangingPassword = true;
    this.authService.changePassword(this.passwordForm.value as any).subscribe({
      next: () => {
        this.toastr.success('Hasło zostało zmienione!');
        this.closePasswordModal();
        this.isChangingPassword = false;
      },
      error: () => {
        this.toastr.error('Nie udało się zmienić hasła. Sprawdź obecne hasło.');
        this.isChangingPassword = false;
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
        if (this.business) this.business.photoUrl = response.photoUrl;
        this.isUploadingBusinessPhoto = false;
      },
      error: () => {
        this.toastr.error('Błąd podczas wgrywania zdjęcia.');
        this.isUploadingBusinessPhoto = false;
      }
    });
  }
}