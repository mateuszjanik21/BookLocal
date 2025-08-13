import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { Business, BusinessDetail, BusinessSearchResult, Employee, PagedResult, ServiceCategorySearchResult, ServiceSearchResult } from '../../types/business.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class BusinessService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getBusinesses(searchQuery?: string): Observable<Business[]> {
    let params = new HttpParams();
    if (searchQuery) {
      params = params.append('searchQuery', searchQuery);
    }

    return this.http.get<Business[]>(`${this.apiUrl}/businesses`, { params });
  }

  getBusinessById(id: number): Observable<BusinessDetail> {
    return this.http.get<BusinessDetail>(`${this.apiUrl}/businesses/${id}`);
  }

  getEmployeesForService(businessId: number, serviceId: number): Observable<Employee[]> {
    return this.http.get<Employee[]>(`${this.apiUrl}/businesses/${businessId}/services/${serviceId}/employees`);
  }

  getMyBusiness(): Observable<BusinessDetail> {
    return this.http.get<BusinessDetail>(`${this.apiUrl}/businesses/my-business`);
  }

  searchServices(params: { searchTerm?: string, mainCategoryId?: number, sortBy?: string, pageNumber: number, pageSize: number }): Observable<PagedResult<ServiceSearchResult>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString());

    if (params.searchTerm) { httpParams = httpParams.set('searchTerm', params.searchTerm); }
    if (params.mainCategoryId) { httpParams = httpParams.set('mainCategoryId', params.mainCategoryId.toString()); }
    if (params.sortBy) { httpParams = httpParams.set('sortBy', params.sortBy); }

    return this.http.get<PagedResult<ServiceSearchResult>>(`${this.apiUrl}/search/services`, { params: httpParams });
  }

  searchBusinesses(params: { searchTerm?: string, mainCategoryId?: number, sortBy?: string, pageNumber: number, pageSize: number }): Observable<PagedResult<BusinessSearchResult>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString());

    if (params.searchTerm) { httpParams = httpParams.set('searchTerm', params.searchTerm); }
    if (params.mainCategoryId) { httpParams = httpParams.set('mainCategoryId', params.mainCategoryId.toString()); }
    if (params.sortBy) { httpParams = httpParams.set('sortBy', params.sortBy); }

    return this.http.get<PagedResult<BusinessSearchResult>>(`${this.apiUrl}/search/businesses`, { params: httpParams });
  }

  searchCategoryFeed(params: { 
    searchTerm?: string, 
    mainCategoryId?: number, 
    sortBy?: string,
    pageNumber: number,
    pageSize: number
  }): Observable<PagedResult<ServiceCategorySearchResult>> {
    
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString());
    
    if (params.searchTerm) { httpParams = httpParams.set('searchTerm', params.searchTerm); }
    if (params.mainCategoryId) { httpParams = httpParams.set('mainCategoryId', params.mainCategoryId.toString()); }
    if (params.sortBy) { httpParams = httpParams.set('sortBy', params.sortBy); }
    
    return this.http.get<PagedResult<ServiceCategorySearchResult>>(`${this.apiUrl}/search/category-feed`, { params: httpParams });
  }
}