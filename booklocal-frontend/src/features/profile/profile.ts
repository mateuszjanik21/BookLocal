import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../core/services/auth-service';
import { UserDto } from '../../types/auth.models';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { PhotoService } from '../../core/services/photo';
import { ImageUploadComponent } from '../../shared/components/image-upload/image-upload';
import { RouterModule } from '@angular/router';
import { FavoriteService, FavoriteServiceDto } from '../../core/services/favourite-service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ImageUploadComponent, RouterModule],
  templateUrl: './profile.html',
})
export class ProfileComponent implements OnInit {
  private authService = inject(AuthService);
  private photoService = inject(PhotoService);
  private fb = inject(FormBuilder);
  private toastr = inject(ToastrService);
  private favoriteService = inject(FavoriteService);

  user$: Observable<UserDto | null> = this.authService.currentUser$;
  isUploading = false;
  favorites: FavoriteServiceDto[] = [];

  passwordForm: FormGroup;
  profileForm: FormGroup;

  constructor() {
    this.passwordForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(6)]]
    });

    this.profileForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.user$.subscribe(user => {
      if (user) {
        this.profileForm.patchValue({
          firstName: user.firstName,
          lastName: user.lastName
        });
        this.loadFavorites();
      }
    });
  }

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
  
  onProfileSubmit(): void {
    if (this.profileForm.invalid) return;

    const payload = {
      firstName: this.profileForm.value.firstName!,
      lastName: this.profileForm.value.lastName!
    };
    
    this.authService.updateProfile(payload).subscribe({
      next: (updatedUser) => {
        this.toastr.success('Twoje dane zostały zaktualizowane!');
        this.profileForm.markAsPristine();
      },
      error: () => this.toastr.error('Wystąpił błąd podczas aktualizacji danych.')
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


  loadFavorites() {
      this.favoriteService.getFavorites().subscribe({
          next: (favs) => this.favorites = favs,
          error: () => this.toastr.error('Nie udało się pobrać ulubionych usług.')
      });
  }

  removeFavorite(variantId: number) {
      this.favoriteService.removeFavorite(variantId).subscribe({
          next: () => {
              this.favorites = this.favorites.filter(f => f.serviceVariantId !== variantId);
              this.toastr.success('Usunięto z ulubionych');
          },
          error: () => this.toastr.error('Nie udało się usunąć z ulubionych')
      });
  }
}