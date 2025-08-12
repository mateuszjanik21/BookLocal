import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { CreateReviewPayload, Review, UpdateReviewPayload } from '../../types/review.model';

@Injectable({
  providedIn: 'root'
})
export class ReviewService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getReviews(businessId: number): Observable<Review[]> {
    return this.http.get<Review[]>(`${this.apiUrl}/businesses/${businessId}/reviews`);
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