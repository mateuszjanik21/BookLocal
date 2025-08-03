import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BusinessService } from '../../core/services/business-service';
import { Business } from '../../types/business.model';
import { RouterModule } from '@angular/router';
import { debounceTime, distinctUntilChanged, Subject, Subscription } from 'rxjs';

@Component({
  selector: 'app-business-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './business-list.html',
  styleUrl: './business-list.css'
})
export class BusinessListComponent implements OnInit {
  private businessService = inject(BusinessService);
  businesses: Business[] = [];

  isLoading = true;
  
  private searchSubject = new Subject<string>();
  private searchSubscription?: Subscription;

  ngOnInit(): void {
    this.loadBusinesses();

    this.searchSubscription = this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(searchTerm => {
      this.loadBusinesses(searchTerm);
    });
  }

  loadBusinesses(searchTerm?: string): void {
    this.isLoading = true;
    this.businessService.getBusinesses(searchTerm).subscribe({
      next: (data) => {
        this.businesses = data;
        this.isLoading = false;
      },
      error: (err: any) => {
        console.error('Błąd podczas pobierania firm:', err);
        this.isLoading = false;
      }
    });
  }

  onSearch(event: Event): void {
    const searchTerm = (event.target as HTMLInputElement).value;
    this.searchSubject.next(searchTerm);
  }

  ngOnDestroy(): void {
    this.searchSubscription?.unsubscribe();
  }
}