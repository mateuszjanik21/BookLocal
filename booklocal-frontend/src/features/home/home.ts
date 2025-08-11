import { Component, OnInit, OnDestroy, inject, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CategoryService } from '../../core/services/category';
import { Service, ServiceCategoryFeed } from '../../types/business.model';
import { Router, RouterModule } from '@angular/router';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export class HomeComponent implements OnInit, OnDestroy {
  private categoryService = inject(CategoryService);
  private router = inject(Router);

  @ViewChild('searchInput') searchInput!: ElementRef<HTMLInputElement>;

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

  allFeedItems: ServiceCategoryFeed[] = [];
  filteredFeed: ServiceCategoryFeed[] = [];
  isLoading = true;

  private searchSubject = new Subject<string>();
  private searchSubscription?: Subscription;

  ngOnInit(): void {
    this.currentBackgroundImage = this.backgroundImages[0];
    this.startImageRotation();

    this.categoryService.getCategoryFeed().subscribe({
      next: (data) => {
        this.allFeedItems = data;
        this.filteredFeed = data;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        console.error('Błąd podczas pobierania ofert.');
      }
    });

    this.searchSubscription = this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(searchTerm => {
      this.filterFeed(searchTerm);
    });
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

  filterFeed(searchTerm: string): void {
    const lowerCaseSearchTerm = searchTerm.toLowerCase().trim();
    if (!lowerCaseSearchTerm) {
      this.filteredFeed = this.allFeedItems;
    } else {
      this.filteredFeed = this.allFeedItems.filter(item => 
        item.name.toLowerCase().includes(lowerCaseSearchTerm) ||
        item.businessName.toLowerCase().includes(lowerCaseSearchTerm)
      );
    }
  }

  onSearch(searchTerm: string): void {
    this.searchSubject.next(searchTerm);
  }

  clearSearch(): void {
    this.searchInput.nativeElement.value = ''; 
    this.searchSubject.next(''); 
  }

  ngOnDestroy(): void {
    this.searchSubscription?.unsubscribe();
    if (this.imageInterval) {
      clearInterval(this.imageInterval);
    }
  }
}