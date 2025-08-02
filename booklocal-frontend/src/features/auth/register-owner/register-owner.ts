import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/services/auth-service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register-owner',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './register-owner.html',
  styleUrl: './register-owner.css'
})
export class RegisterOwnerComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  registerForm: FormGroup = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    businessName: ['', Validators.required],
    nip: ['', Validators.required],
    address: [''],
    city: [''],
    description: ['']
  });

  onSubmit() {
    if (this.registerForm.invalid) return;

    this.authService.registerOwner(this.registerForm.value).subscribe({
      next: () => {
        this.router.navigate(['/']);
      },
      error: (err) => {
        console.error('Błąd podczas rejestracji właściciela:', err);
      }
    });
  }
}