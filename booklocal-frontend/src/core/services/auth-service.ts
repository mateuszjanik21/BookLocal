import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { AuthResponse, LoginPayload, RegisterPayload, EntrepreneurRegisterPayload, UserDto } from '../../types/auth.models';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private apiUrl = environment.apiUrl;

  private currentUserSubject = new BehaviorSubject<UserDto | null>(null);
  
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor() {
    this.loadUserFromToken();
  }

  registerCustomer(payload: RegisterPayload) {
    return this.http.post(`${this.apiUrl}/auth/register-customer`, payload);
  }

  registerOwner(payload: EntrepreneurRegisterPayload) {
    return this.http.post<AuthResponse>(`${this.apiUrl}/auth/register-owner`, payload).pipe(
      tap(response => this.handleAuthentication(response))
    );
  }

  login(payload: LoginPayload): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/auth/login`, payload).pipe(
      tap(response => this.handleAuthentication(response))
    );
  }

  logout(): void {
    localStorage.removeItem('authToken');
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  private handleAuthentication(response: AuthResponse): void {
    localStorage.setItem('authToken', response.token);
    this.currentUserSubject.next(response.user);
  }
  
  private loadUserFromToken(): void {
    const token = localStorage.getItem('authToken');
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const user: UserDto = {
          id: payload.nameid,
          email: payload.email,
          firstName: '',
          lastName: '',
          roles: typeof payload.role === 'string' ? [payload.role] : payload.role
        };
        this.currentUserSubject.next(user);
      } catch (error) {
        localStorage.removeItem('authToken');
      }
    }
  }
}