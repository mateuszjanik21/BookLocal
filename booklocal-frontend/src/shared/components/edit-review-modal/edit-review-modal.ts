import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { ReviewService } from '../../../core/services/review';
import { Review } from '../../../types/review.model';

@Component({
  selector: 'app-edit-review-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './edit-review-modal.html',
})
export class EditReviewModalComponent implements OnInit {
  @Input() businessId!: number;
  @Input() review!: Review;
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

  ngOnInit(): void {
    this.reviewForm.patchValue({
      rating: this.review.rating,
      comment: this.review.comment
    });
  }

  setRating(rating: number): void {
    this.reviewForm.controls.rating.setValue(rating);
  }

  ratingLabels: string[] = ['Słabo', 'Znośnie', 'OK', 'Dobrze', 'Rewelacyjnie!'];

  get currentRatingLabel(): string {
    const rating = this.hoveredRating || this.reviewForm.controls.rating.value || 0;
    return rating > 0 ? this.ratingLabels[rating - 1] : 'Wybierz ocenę';
  }

  onSubmit(): void {
    if (this.reviewForm.invalid) return;
    this.isSaving = true;

    this.reviewService.updateReview(this.businessId, this.review.reviewId, this.reviewForm.value as any)
      .subscribe({
        next: () => {
          this.toastr.success('Twoja opinia została zaktualizowana!');
          this.isSaving = false;
          this.closed.emit(true);
        },
        error: () => {
          this.toastr.error('Wystąpił błąd podczas aktualizacji.');
          this.isSaving = false;
          this.closed.emit(false);
        }
    });
  }
}