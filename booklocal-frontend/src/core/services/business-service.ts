import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { Business } from '../../types/business.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class BusinessService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getBusinesses(): Observable<Business[]> {
    return this.http.get<Business[]>(`${this.apiUrl}/businesses`);
  }
}