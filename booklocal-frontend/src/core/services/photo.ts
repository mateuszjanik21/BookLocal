import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class PhotoService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  uploadUserProfilePhoto(file: File) {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<{ photoUrl: string }>(`${this.apiUrl}/photos/upload-profile-photo`, formData);
  }

  uploadBusinessPhoto(file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ photoUrl: string }>(`${this.apiUrl}/photos/business`, formData);
  }

  uploadEmployeePhoto(employeeId: number, file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ photoUrl: string }>(`${this.apiUrl}/photos/employee/${employeeId}`, formData);
  }

  uploadCategoryPhoto(categoryId: number, file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ photoUrl: string }>(`${this.apiUrl}/photos/category/${categoryId}`, formData);
  }
}