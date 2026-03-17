import { Component, OnInit, OnDestroy, inject, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { BusinessService } from '../../core/services/business-service';
import { CategoryService } from '../../core/services/category';
import { AuthService } from '../../core/services/auth-service';
import { MainCategory, PagedResult, RebookSuggestion, ServiceCategorySearchResult } from '../../types/business.model';

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
  private authService = inject(AuthService);
  @ViewChild('searchInput') searchInput!: ElementRef<HTMLInputElement>;
  @ViewChild('locationInput') locationInput!: ElementRef<HTMLInputElement>;
  @ViewChild('chipsContainer') chipsContainer!: ElementRef<HTMLDivElement>;
  @ViewChild('rebookContainer') rebookContainer!: ElementRef<HTMLDivElement>;
  
  locationTerm: string = '';
  Math = Math;
  isSkeletonVisible = false;
  private skeletonTimeout: any;
  mainCategories: MainCategory[] = [];
  
  pagedResult: PagedResult<ServiceCategorySearchResult> | null = null;
  pageNumber = 1;
  pageSize = 12;

  activeMainCategoryId: number | null = null;
  activeSortBy = 'rating_desc';
  isLocationLoading = false;
  isLoading = false;
  rebookSuggestions: RebookSuggestion[] = [];

  private searchSubject = new Subject<void>();
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
    this.loadRebookSuggestions();
    this.fetchResults();
    this.searchSubscription = this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(() => this.fetchResults());
  }

  fetchResults(): void {
    this.isLoading = true;
    this.isSkeletonVisible = false;
    
    if (this.skeletonTimeout) {
      clearTimeout(this.skeletonTimeout);
    }
    
    this.skeletonTimeout = setTimeout(() => {
      if (this.isLoading) {
        this.isSkeletonVisible = true;
      }
    }, 250); // Pokaż szkielet tylko jeśli ładowanie trwa dłużej niż 250ms

    const params = {
      searchTerm: this.searchInput?.nativeElement.value,
      locationTerm: this.locationTerm,
      mainCategoryId: this.activeMainCategoryId ?? undefined,
      sortBy: this.activeSortBy,
      pageNumber: this.pageNumber,
      pageSize: this.pageSize
    };

    this.businessService.searchCategoryFeed(params).subscribe({
      next: (data) => {
        this.pagedResult = data;
        this.cleanupLoadingState();
      },
      error: () => {
        this.cleanupLoadingState();
      }
    });
  }

  private cleanupLoadingState(): void {
    if (this.skeletonTimeout) {
      clearTimeout(this.skeletonTimeout);
    }
    this.isLoading = false;
    this.isSkeletonVisible = false;
  }

  onSearchInput(): void {
    this.pageNumber = 1;
    this.searchSubject.next();
  }

  onLocationInput(event: Event): void {
    const val = (event.target as HTMLInputElement).value;
    this.locationTerm = val;
    this.pageNumber = 1;
    this.searchSubject.next();
  }

  useCurrentLocation(): void {
    if (this.isLocationLoading) return;
    
    if (navigator.geolocation) {
      this.isLocationLoading = true;
      navigator.geolocation.getCurrentPosition(
        (position) => {
          const lat = position.coords.latitude;
          const lng = position.coords.longitude;
          
          fetch(`https://api.bigdatacloud.net/data/reverse-geocode-client?latitude=${lat}&longitude=${lng}&localityLanguage=pl`)
            .then(res => res.json())
            .then(data => {
              this.locationTerm = data.city || data.locality || 'Moja lokalizacja';
              if (this.locationInput) this.locationInput.nativeElement.value = this.locationTerm;
              this.isLocationLoading = false;
              this.onFilterChange();
            })
            .catch(() => {
              this.locationTerm = 'Moja lokalizacja';
              if (this.locationInput) this.locationInput.nativeElement.value = this.locationTerm;
              this.isLocationLoading = false;
              this.onFilterChange();
            });
        },
        (error) => {
          console.error('Błąd pobierania lokalizacji', error);
          this.isLocationLoading = false;
        },
        {
          enableHighAccuracy: true,
          timeout: 10000,
          maximumAge: 0
        }
      );
    } else {
      console.warn('Geolokalizacja nie jest wspierana przez tę przeglądarkę.');
    }
  }

  onFilterChange(): void {
    this.pageNumber = 1;
    this.fetchResults();
  }

  getLowestPrice(item: ServiceCategorySearchResult): string {
    const price = item.services?.[0]?.variants?.[0]?.price;
    return price !== undefined ? `${price}` : '---';
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
  
  loadRebookSuggestions(): void {
    if (this.authService.isLoggedIn() && this.authService.hasRole('customer')) {
      this.businessService.getRebookSuggestions().subscribe({
        next: (data) => this.rebookSuggestions = data,
        error: () => {} // Silently ignore (e.g. 401)
      });
    }
  }

  clearSearch(): void {
    if (this.searchInput) {
      this.searchInput.nativeElement.value = '';
    }
    if (this.locationInput) {
      this.locationInput.nativeElement.value = '';
    }
    this.locationTerm = '';
    this.onSearchInput();
  }

  scrollChips(amount: number): void {
    if (this.chipsContainer) {
      this.chipsContainer.nativeElement.scrollBy({ left: amount, behavior: 'smooth' });
    }
  }

  scrollRebook(amount: number): void {
    if (this.rebookContainer) {
      this.rebookContainer.nativeElement.scrollBy({ left: amount, behavior: 'smooth' });
    }
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
    if (this.skeletonTimeout) {
      clearTimeout(this.skeletonTimeout);
    }
  }
}