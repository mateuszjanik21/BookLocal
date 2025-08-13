import { Component, OnInit, OnDestroy, inject, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { BusinessService } from '../../core/services/business-service';
import { CategoryService } from '../../core/services/category';
import { MainCategory, PagedResult, ServiceCategorySearchResult } from '../../types/business.model';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export class HomeComponent implements OnInit, OnDestroy {
  private businessService = inject(BusinessService);
  private categoryService = inject(CategoryService);
  @ViewChild('searchInput') searchInput!: ElementRef<HTMLInputElement>;
  
  Math = Math;
  isLoading = true;
  mainCategories: MainCategory[] = [];
  
  pagedResult: PagedResult<ServiceCategorySearchResult> | null = null;
  pageNumber = 1;
  pageSize = 10;

  activeMainCategoryId: number | null = null;
  activeSortBy = 'rating_desc';

  private searchSubject = new Subject<string>();
  private searchSubscription?: Subscription;
  
  backgroundImages: string[] = [
    'https://cdn.pixabay.com/photo/2022/05/28/02/25/barber-shop-7226341_1280.jpg',
    'https://cdn.pixabay.com/photo/2019/03/15/13/46/hairstyle-4057094_1280.jpg',
    'https://cdn.pixabay.com/photo/2018/02/27/03/36/stones-3184610_960_720.jpg',
    'https://cdn.pixabay.com/photo/2017/11/18/05/02/yoga-2959226_1280.jpg',
    'https://cdn.pixabay.com/photo/2016/10/18/08/52/blood-pressure-monitor-1749577_960_720.jpg'
  ];
  currentBackgroundImage: string = '';
  isImageFading = false;
  private imageInterval: any;

  ngOnInit(): void {
    this.currentBackgroundImage = this.backgroundImages[0];
    this.startImageRotation();
    this.categoryService.getMainCategories().subscribe(data => this.mainCategories = data);
    this.fetchResults();
    this.searchSubscription = this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(() => this.fetchResults());
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
    this.businessService.searchCategoryFeed(params).subscribe({
      next: (data) => {
        this.pagedResult = data;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  onSearchInput(): void {
    this.pageNumber = 1;
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
    window.scrollTo(0, 400);
  }

  get paginationPages(): (number | string)[] {
    if (!this.pagedResult) return [];

    const { totalPages, pageNumber } = this.pagedResult;
    if (totalPages <= 7) {
      return Array.from({ length: totalPages }, (_, i) => i + 1);
    }

    const pages: (number | string)[] = [];
    pages.push(1);
    
    if (pageNumber > 4) {
      pages.push('...');
    }
    
    for (let i = pageNumber - 2; i <= pageNumber + 2; i++) {
      if (i > 1 && i < totalPages) {
        pages.push(i);
      }
    }

    if (pageNumber < totalPages - 3) {
      pages.push('...');
    }

    pages.push(totalPages);

    return pages;
  }
  
  clearSearch(): void {
    if (this.searchInput) {
      this.searchInput.nativeElement.value = '';
    }
    this.onSearchInput();
  }
  
  startImageRotation(): void {
    this.imageInterval = setInterval(() => {
      this.isImageFading = true;
      setTimeout(() => {
        const currentIndex = this.backgroundImages.indexOf(this.currentBackgroundImage);
        let nextIndex;
        do {
          nextIndex = Math.floor(Math.random() * this.backgroundImages.length);
        } while (nextIndex === currentIndex);
        
        this.currentBackgroundImage = this.backgroundImages[nextIndex];
        this.isImageFading = false;
      }, 750);
    }, 7000);
  }

  ngOnDestroy(): void {
    this.searchSubscription?.unsubscribe();
    if (this.imageInterval) {
      clearInterval(this.imageInterval);
    }
  }
}