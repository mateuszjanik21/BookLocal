import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { CreateReviewPayload, Review, UpdateReviewPayload } from '../../types/review.model';
import { PagedResult } from '../../types/business.model';

@Injectable({
  providedIn: 'root'
})
export class ReviewService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getReviews(businessId: number, pageNumber: number, pageSize: number): Observable<PagedResult<Review>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<Review>>(`${this.apiUrl}/businesses/${businessId}/reviews`, { params });
  }

  postReviewForReservation(reservationId: number, payload: CreateReviewPayload): Observable<Review> {
    return this.http.post<Review>(`${this.apiUrl}/reservations/${reservationId}/reviews`, payload);
  }

  canUserReview(businessId: number): Observable<{ canReview: boolean }> {
    return this.http.get<{ canReview: boolean }>(`${this.apiUrl}/businesses/${businessId}/reviews/can-review`);
  }

  updateReview(businessId: number, reviewId: number, payload: UpdateReviewPayload): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/businesses/${businessId}/reviews/${reviewId}`, payload);
  }

  deleteReview(businessId: number, reviewId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/businesses/${businessId}/reviews/${reviewId}`);
  }
}