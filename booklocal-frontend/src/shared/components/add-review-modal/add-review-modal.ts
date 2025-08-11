import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { ReviewService } from '../../../core/services/review';

@Component({
  selector: 'app-add-review-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './add-review-modal.html',
})
export class AddReviewModalComponent {
  @Input() businessId: number | null = null;
  @Output() closed = new EventEmitter<boolean>(); 

  private fb = inject(FormBuilder);
  private reviewService = inject(ReviewService);
  private toastr = inject(ToastrService);

  reviewForm = this.fb.group({
    rating: [0, [Validators.required, Validators.min(1)]],
    comment: ['']
  });

  hoveredRating = 0;
  isSaving = false;

  ratingLabels: string[] = ['Słabo', 'Znośnie', 'OK', 'Dobrze', 'Rewelacyjnie!'];

  get currentRatingLabel(): string {
    const rating = this.hoveredRating || this.reviewForm.controls.rating.value || 0;
    return rating > 0 ? this.ratingLabels[rating - 1] : 'Wybierz ocenę';
  }

  setRating(rating: number): void {
    this.reviewForm.controls.rating.setValue(rating);
  }

  onSubmit(): void {
    if (this.reviewForm.invalid || !this.businessId) {
      this.toastr.error('Wybierz ocenę w gwiazdkach, aby dodać opinię.');
      return;
    }
    this.isSaving = true;
    this.reviewService.postReview(this.businessId, this.reviewForm.value as any).subscribe({
      next: () => {
        this.toastr.success('Dziękujemy za Twoją opinię!');
        this.isSaving = false;
        this.closed.emit(true); 
      },
      error: (err) => {
        this.toastr.error('Wystąpił błąd podczas dodawania opinii.');
        this.isSaving = false;
        this.closed.emit(false);
      }
    });
  }
}