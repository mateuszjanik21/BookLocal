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
    phoneNumber: ['', [Validators.required, Validators.pattern(/^\d{9}$/)]],
    businessName: ['', Validators.required],
    nip: ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
    street: ['', Validators.required],
    streetNumber: ['', Validators.required],
    postalCode: ['', [Validators.required, Validators.pattern(/^\d{2}-\d{3}$/)]],
    city: ['', Validators.required],
    description: ['']
  });

  onSubmit() {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    const v = this.registerForm.value;
    const payload = {
      firstName: v.firstName,
      lastName: v.lastName,
      email: v.email,
      password: v.password,
      phoneNumber: v.phoneNumber,
      businessName: v.businessName,
      nip: v.nip,
      address: `${v.street} ${v.streetNumber}`,
      city: `${v.postalCode} ${v.city}`,
      description: v.description
    };

    this.authService.registerOwner(payload).subscribe({
      next: () => {
        this.router.navigate(['/']);
      },
      error: (err) => {
        console.error('Błąd podczas rejestracji właściciela:', err);
      }
    });
  }
}
