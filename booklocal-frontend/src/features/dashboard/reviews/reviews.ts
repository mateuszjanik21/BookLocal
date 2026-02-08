import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReviewService } from '../../../core/services/review';
import { BusinessService } from '../../../core/services/business-service';
import { Review } from '../../../types/review.model';

@Component({
  selector: 'app-reviews',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './reviews.html'
})
export class ReviewsComponent implements OnInit {
  private reviewService = inject(ReviewService);
  private businessService = inject(BusinessService);

  reviews: Review[] = [];
  totalCount = 0;
  pageNumber = 1;
  pageSize = 10;
  isLoading = true;
  businessId: number | null = null;

  ngOnInit() {
    this.businessService.getMyBusiness().subscribe(business => {
      if (business) {
        this.businessId = business.id;
        this.loadReviews();
      }
    });
  }

  loadReviews() {
    if (!this.businessId) return;
    
    this.isLoading = true;
    this.reviewService.getReviews(this.businessId, this.pageNumber, this.pageSize).subscribe({
      next: (result) => {
        this.reviews = result.items;
        this.totalCount = result.totalCount;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  onPageChange(page: number) {
    this.pageNumber = page;
    this.loadReviews();
  }

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  get pages(): number[] {
    const pages = [];
    for (let i = 1; i <= this.totalPages; i++) {
        pages.push(i);
    }
    return pages; 
  }
}
