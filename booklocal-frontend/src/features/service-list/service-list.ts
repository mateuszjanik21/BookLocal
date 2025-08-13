import { Component, OnInit, inject, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { BusinessService } from '../../core/services/business-service';
import { CategoryService } from '../../core/services/category';
import { MainCategory, PagedResult, ServiceSearchResult } from '../../types/business.model';

@Component({
  selector: 'app-service-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './service-list.html',
})
export class ServiceListComponent implements OnInit {
  private businessService = inject(BusinessService);
  private categoryService = inject(CategoryService);

  @ViewChild('searchInput') searchInput!: ElementRef<HTMLInputElement>;
  
  Math = Math;
  isLoading = true;
  mainCategories: MainCategory[] = [];
  
  pagedResult: PagedResult<ServiceSearchResult> | null = null;
  pageNumber = 1;
  pageSize = 12; // Możemy wyświetlić więcej mniejszych kart

  activeMainCategoryId: number | null = null;
  activeSortBy = 'rating_desc';

  private searchSubject = new Subject<string>();
  private searchSubscription?: Subscription;

  ngOnInit(): void {
    this.categoryService.getMainCategories().subscribe(data => this.mainCategories = data);
    this.fetchResults();
    this.searchSubscription = this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(() => {
      this.pageNumber = 1;
      this.fetchResults();
    });
  }

  fetchResults(): void {
    this.isLoading = true;
    const params = {
      searchTerm: this.searchInput?.nativeElement.value,
      mainCategoryId: this.activeMainCategoryId ?? undefined,
      sortBy: this.activeSortBy,
      pageNumber: this.pageNumber,
      pageSize: this.pageSize
    };
    this.businessService.searchServices(params).subscribe({
      next: (data) => {
        this.pagedResult = data;
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
}