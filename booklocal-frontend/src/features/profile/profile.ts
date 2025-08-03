import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../core/services/auth-service';
import { UserDto } from '../../types/auth.models';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile.html',
})
export class ProfileComponent {
  private authService = inject(AuthService);
  private fb = inject(FormBuilder);
  private toastr = inject(ToastrService);

  user$: Observable<UserDto | null> = this.authService.currentUser$;

  passwordForm: FormGroup = this.fb.group({
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
}