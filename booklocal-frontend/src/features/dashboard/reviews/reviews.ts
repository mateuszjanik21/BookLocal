import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ReviewService } from '../../../core/services/review';
import { BusinessService } from '../../../core/services/business-service';
import { Review } from '../../../types/review.model';

@Component({
  selector: 'app-reviews',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reviews.html'
})
export class ReviewsComponent implements OnInit, OnDestroy {
  private reviewService = inject(ReviewService);
  private businessService = inject(BusinessService);

  reviews: Review[] = [];
  totalCount = 0;
  pageNumber = 1;
  pageSize = 12;
  isLoading = true;
  businessId: number | null = null;

  searchQuery = '';
  filterRating: number | null = null;
  sortBy = 'newest';
  searchSubject = new Subject<string>();
  private searchSubscription?: Subscription;

  ngOnInit() {
    this.searchSubscription = this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe(() => {
      this.pageNumber = 1;
      this.loadReviews();
    });

    this.businessService.getMyBusiness().subscribe(business => {
      if (business) {
        this.businessId = business.id;
        this.loadReviews();
      }
    });
  }

  ngOnDestroy() {
    this.searchSubscription?.unsubscribe();
  }

  onSearchChange(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.searchQuery = value;
    this.searchSubject.next(value);
  }

  onFilterChange() {
    this.pageNumber = 1;
    this.loadReviews();
  }

  loadReviews() {
    if (!this.businessId) return;
    
    this.isLoading = true;
    this.reviewService.getReviews(this.businessId, this.pageNumber, this.pageSize, this.filterRating, this.searchQuery, this.sortBy).subscribe({
      next: (result) => {
        this.reviews = result.items;
        this.totalCount = result.totalCount;
        setTimeout(() => this.isLoading = false, 300);
      },
      error: () => {
        setTimeout(() => this.isLoading = false, 300);
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

  get visiblePages(): (number | string)[] {
    const total = this.totalPages;
    const current = this.pageNumber;
    
    if (total <= 5) {
      return Array.from({ length: total }, (_, i) => i + 1);
    }
    
    const pages: (number | string)[] = [];
    
    pages.push(1);
    
    if (current > 3) {
      pages.push('...');
    }
    
    let start = Math.max(2, current - 1);
    let end = Math.min(total - 1, current + 1);
    
    if (current <= 3) {
      end = 4;
    } else if (current >= total - 2) {
      start = total - 3;
    }
    
    for (let i = start; i <= end; i++) {
        pages.push(i);
    }
    
    if (current < total - 2) {
      pages.push('...');
    }
    
    pages.push(total);
    
    return pages;
  }
}
