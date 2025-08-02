import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { AuthResponse, EntrepreneurRegisterPayload, LoginPayload, RegisterPayload } from '../../types/auth.models';
import { tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  registerCustomer(payload: RegisterPayload) {
    return this.http.post(`${this.apiUrl}/auth/register-customer`, payload);
  }

  registerOwner(payload: EntrepreneurRegisterPayload) {
    return this.http.post<AuthResponse>(`${this.apiUrl}/auth/register-owner`, payload).pipe(
      tap(response => {
        localStorage.setItem('authToken', response.token);
        console.log('Zarejestrowano właściciela i zapisano token!');
      })
    );
  }

  login(payload: LoginPayload) {
    return this.http.post<AuthResponse>(`${this.apiUrl}/auth/login`, payload).pipe(
      tap(response => {
        localStorage.setItem('authToken', response.token);
        console.log('Zalogowano i zapisano token!');
      })
    );
  }
}