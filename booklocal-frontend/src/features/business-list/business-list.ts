import { Component, OnInit, OnDestroy, inject, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { BusinessService } from '../../core/services/business-service';
import { CategoryService } from '../../core/services/category';
import { MainCategory, BusinessSearchResult, PagedResult } from '../../types/business.model';

@Component({
  selector: 'app-business-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './business-list.html',
  styleUrls: ['./business-list.css'] 
})
export class BusinessListComponent implements OnInit, OnDestroy {
  private businessService = inject(BusinessService);
  private categoryService = inject(CategoryService);

  @ViewChild('searchInput') searchInput!: ElementRef<HTMLInputElement>;
  
  Math = Math;
  isLoading = true;
  mainCategories: MainCategory[] = [];
  
  pagedResult: PagedResult<BusinessSearchResult> | null = null;
  pageNumber = 1;
  pageSize = 12;

  activeMainCategoryId: number | null = null;
  activeSortBy = 'rating_desc';
  activeLocation = '';
  onlyVerified = false;

  promotedBusinesses: BusinessSearchResult[] = [];
  regularBusinesses: BusinessSearchResult[] = [];
  
  activePromotedIndex = 0;
  private promotedRotationInterval: any;

  selectedBusinessQuickView: BusinessSearchResult | null = null;

  private searchSubject = new Subject<string>();
  private searchSubscription?: Subscription;

  ngOnInit(): void {
    this.categoryService.getMainCategories().subscribe(data => this.mainCategories = data);
    this.loadPromotedBusinesses();
    this.fetchResults();
    this.searchSubscription = this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(() => {
      this.pageNumber = 1;
      this.fetchResults();
    });
  }

  loadPromotedBusinesses(): void {
    this.businessService.searchBusinesses({
      sortBy: 'rating_desc',
      pageNumber: 1,
      pageSize: 20
    }).subscribe({
      next: (data) => {
        if (data && data.items && data.items.length > 0) {
           const eligible = data.items.filter(b => b.isVerified && b.subscriptionPlanName === 'Gold' && b.averageRating >= 4.5);
           const finalEligible = eligible.length > 0 ? eligible : data.items.filter(b => b.isVerified && b.averageRating >= 4.5);
           const shuffled = finalEligible.sort(() => 0.5 - Math.random());
           this.promotedBusinesses = shuffled.slice(0, 4);
           this.startPromotedRotation();
        }
      }
    });
  }

  fetchResults(): void {
    this.isLoading = true;
    const params = {
      searchTerm: this.searchInput?.nativeElement.value,
      locationTerm: this.activeLocation || undefined,
      mainCategoryId: this.activeMainCategoryId ?? undefined,
      sortBy: this.activeSortBy,
      onlyVerified: this.onlyVerified,
      pageNumber: this.pageNumber,
      pageSize: this.pageSize
    };
    this.businessService.searchBusinesses(params).subscribe({
      next: (data) => {
        this.pagedResult = data;
        this.regularBusinesses = data.items || [];
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  onSearchInput(): void {
    this.searchSubject.next(this.searchInput.nativeElement.value);
  }

  onFilterChange(): void {
    this.pageNumber = 1;
    this.fetchResults();
  }

  selectCategory(id: number | null): void {
    this.activeMainCategoryId = id;
    this.onFilterChange();
  }

  openQuickView(business: BusinessSearchResult): void {
    this.selectedBusinessQuickView = business;
    document.body.style.overflow = 'hidden';
  }

  closeQuickView(): void {
    this.selectedBusinessQuickView = null;
    document.body.style.overflow = '';
  }
  
  pageChanged(newPage: number): void {
    if (this.pageNumber === newPage) return;
    this.pageNumber = newPage;
    this.fetchResults();
    window.scrollTo(0, 0);
  }

  get paginationPages(): (number | string)[] {
    if (!this.pagedResult) return [];
    const { totalPages, pageNumber } = this.pagedResult;
    if (totalPages <= 7) {
      return Array.from({ length: totalPages }, (_, i) => i + 1);
    }
    const pages: (number | string)[] = [1];
    if (pageNumber > 4) pages.push('...');
    for (let i = pageNumber - 2; i <= pageNumber + 2; i++) {
      if (i > 1 && i < totalPages) pages.push(i);
    }
    if (pageNumber < totalPages - 3) pages.push('...');
    pages.push(totalPages);
    return pages;
  }
  
  startPromotedRotation(): void {
    this.stopPromotedRotation();
    if (this.promotedBusinesses.length > 1) {
      this.promotedRotationInterval = setInterval(() => {
        this.activePromotedIndex = (this.activePromotedIndex + 1) % this.promotedBusinesses.length;
      }, 5000);
    }
  }

  stopPromotedRotation(): void {
    if (this.promotedRotationInterval) {
      clearInterval(this.promotedRotationInterval);
    }
  }
  
  setPromotedIndex(index: number): void {
      this.activePromotedIndex = index;
      this.startPromotedRotation();
  }

  ngOnDestroy(): void {
    this.searchSubscription?.unsubscribe();
    this.stopPromotedRotation();
    document.body.style.overflow = '';
  }
}
