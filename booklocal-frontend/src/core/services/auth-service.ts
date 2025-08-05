import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { AuthResponse, LoginPayload, RegisterPayload, EntrepreneurRegisterPayload, UserDto, ChangePasswordPayload } from '../../types/auth.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private apiUrl = environment.apiUrl;

  private currentUserSubject = new BehaviorSubject<UserDto | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  
  public get currentUserValue(): UserDto | null {
    return this.currentUserSubject.getValue();
  }

  constructor() {
    this.loadUserFromToken();
  }

  login(payload: LoginPayload): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/auth/login`, payload).pipe(
      tap(response => this.handleAuthentication(response.token, response.user))
    );
  }
  
  registerOwner(payload: EntrepreneurRegisterPayload) {
    return this.http.post<AuthResponse>(`${this.apiUrl}/auth/register-owner`, payload).pipe(
      tap(response => this.handleAuthentication(response.token, response.user))
    );
  }

  logout(): void {
    localStorage.removeItem('authToken');
    localStorage.removeItem('userData'); //
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  updateUserProfilePhoto(photoUrl: string) {
    const currentUser = this.currentUserValue;
    if (currentUser) {
      const updatedUser = { ...currentUser, photoUrl: photoUrl };
      this.currentUserSubject.next(updatedUser);
      localStorage.setItem('userData', JSON.stringify(updatedUser));
    }
  }

  private handleAuthentication(token: string, user: UserDto): void {
    localStorage.setItem('authToken', token);
    localStorage.setItem('userData', JSON.stringify(user));
    this.currentUserSubject.next(user);
  }
  
  private loadUserFromToken(): void {
    const token = localStorage.getItem('authToken');
    const userData = localStorage.getItem('userData');

    if (token && userData) {
      try {
        const user: UserDto = JSON.parse(userData);
        this.currentUserSubject.next(user);
      } catch (error) {
        this.logout();
      }
    }
  }
  
  registerCustomer(payload: RegisterPayload) { return this.http.post(`${this.apiUrl}/auth/register-customer`, payload); }
  changePassword(payload: ChangePasswordPayload) { return this.http.post(`${this.apiUrl}/auth/change-password`, payload); }
}