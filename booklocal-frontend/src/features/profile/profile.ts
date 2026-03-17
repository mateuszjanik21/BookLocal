import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../core/services/auth-service';
import { UserDto } from '../../types/auth.models';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { PhotoService } from '../../core/services/photo';
import { ImageUploadComponent } from '../../shared/components/image-upload/image-upload';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { FavoriteService, FavoriteServiceDto } from '../../core/services/favourite-service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

export interface CustomerStats {
  totalVisits: number;
  totalSpent: number;
  uniqueBusinesses: number;
  favoriteBusinessName: string | null;
}

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ImageUploadComponent, RouterModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css'
})
export class ProfileComponent implements OnInit {
  private authService = inject(AuthService);
  private photoService = inject(PhotoService);
  private fb = inject(FormBuilder);
  private toastr = inject(ToastrService);
  private favoriteService = inject(FavoriteService);
  private http = inject(HttpClient);
  private route = inject(ActivatedRoute);

  user$: Observable<UserDto | null> = this.authService.currentUser$;
  isUploading = false;
  favorites: FavoriteServiceDto[] = [];
  stats: CustomerStats | null = null;
  isStatsLoading = false;

  activeTab: 'dane' | 'logowanie' | 'ulubione' | 'statystyki' = 'dane';

  passwordForm: FormGroup;
  profileForm: FormGroup;

  constructor() {
    this.passwordForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(6)]]
    });

    this.profileForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      phoneNumber: ['']
    });
  }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (params['tab']) {
        const tab = params['tab'];
        if (['dane', 'logowanie', 'ulubione', 'statystyki'].includes(tab)) {
          this.setTab(tab as any);
        }
      }
    });

    this.user$.subscribe(user => {
      if (user) {
        this.profileForm.patchValue({
          firstName: user.firstName,
          lastName: user.lastName,
          phoneNumber: user.phoneNumber || ''
        });
      }
    });
  }

  setTab(tab: 'dane' | 'logowanie' | 'ulubione' | 'statystyki'): void {
    this.activeTab = tab;
    if (tab === 'ulubione' && this.favorites.length === 0) {
      this.loadFavorites();
    }
    if (tab === 'statystyki' && !this.stats) {
      this.loadStats();
    }
  }

  onProfileSubmit(): void {
    if (this.profileForm.invalid) return;

    const payload = {
      firstName: this.profileForm.value.firstName!,
      lastName: this.profileForm.value.lastName!,
      phoneNumber: this.profileForm.value.phoneNumber || null
    };
    
    this.authService.updateProfile(payload).subscribe({
      next: () => {
        this.toastr.success('Twoje dane zostały zaktualizowane!');
        this.profileForm.markAsPristine();
      },
      error: () => this.toastr.error('Wystąpił błąd podczas aktualizacji danych.')
    });
  }

  onPasswordSubmit(): void {
    if (this.passwordForm.invalid) return;

    this.authService.changePassword(this.passwordForm.value as any).subscribe({
      next: () => {
        this.toastr.success('Hasło zostało zmienione!', 'Sukces');
        this.passwordForm.reset();
      },
      error: () => {
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
      error: () => {
        this.toastr.error('Błąd podczas wgrywania zdjęcia.');
        this.isUploading = false;
      }
    });
  }

  loadFavorites(): void {
    this.favoriteService.getFavorites().subscribe({
      next: (favs) => this.favorites = favs,
      error: () => this.toastr.error('Nie udało się pobrać ulubionych usług.')
    });
  }

  removeFavorite(variantId: number): void {
    this.favoriteService.removeFavorite(variantId).subscribe({
      next: () => {
        this.favorites = this.favorites.filter(f => f.serviceVariantId !== variantId);
        this.toastr.success('Usunięto z ulubionych');
      },
      error: () => this.toastr.error('Nie udało się usunąć z ulubionych')
    });
  }

  loadStats(): void {
    this.isStatsLoading = true;
    this.http.get<CustomerStats>(`${environment.apiUrl}/reservations/my-stats`).subscribe({
      next: (data) => {
        this.stats = data;
        this.isStatsLoading = false;
      },
      error: () => {
        this.isStatsLoading = false;
      }
    });
  }
}