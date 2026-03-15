import { Component, OnInit, inject } from '@angular/core';
import { Location } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { BusinessService } from '../../core/services/business-service';
import { AuthService } from '../../core/services/auth-service';
import { ReviewService } from '../../core/services/review';
import { ToastrService } from 'ngx-toastr';
import { BusinessDetail, Employee, Service } from '../../types/business.model';
import { Review } from '../../types/review.model';
import { ReservationModalComponent } from '../../shared/components/reservation-modal/reservation-modal';
import { EditReviewModalComponent } from '../../shared/components/edit-review-modal/edit-review-modal';
import { ChatService } from '../../core/services/chat';
import { ServiceBundleService } from '../../core/services/service-bundle';
import { ServiceBundle } from '../../types/service-bundle.model';
import { BookBundleModalComponent } from '../dashboard/service-bundles/book-bundle-modal/book-bundle-modal';
import { FavoriteService } from '../../core/services/favourite-service';

@Component({
  selector: 'app-business-detail',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule, 
    ReservationModalComponent,
    EditReviewModalComponent,
    BookBundleModalComponent
  ],
  templateUrl: './business-detail.html',
  styleUrls: ['./business-detail.css']
})
export class BusinessDetailComponent implements OnInit {
  private router = inject(Router); 
  private location = inject(Location);
  private chatService = inject(ChatService);
  private route = inject(ActivatedRoute);
  private businessService = inject(BusinessService);
  private reviewService = inject(ReviewService);
  private toastr = inject(ToastrService);
  public authService = inject(AuthService);

  isLoading = true;
  isReviewsLoading = true;
  
  business: BusinessDetail | null = null;
  reviews: Review[] = [];
  filteredEmployees: Employee[] = [];
  selectedService: Service | null = null;

  isEditReviewModalVisible = false;
  reviewToEdit: Review | null = null;
  
  reviewsCurrentPage = 1;
  reviewsPageSize = 5;
  totalReviews = 0;
  isLoadingMoreReviews = false;

  bundles: ServiceBundle[] = [];
  selectedBundle: ServiceBundle | null = null;
  isBookBundleModalOpen = false;

  private serviceBundleService = inject(ServiceBundleService);
  private favoriteService = inject(FavoriteService);

  favoriteVariantIds = new Set<number>();

  selectedEmployee: Employee | null = null;

  ngOnInit(): void {
    const businessId = this.route.snapshot.paramMap.get('id');

    if (businessId) {
      this.businessService.getBusinessById(+businessId).subscribe({
        next: (data) => {
          this.business = data;
          this.isLoading = false; 
          this.loadReviews(+businessId);
          this.loadBundles(+businessId);
        },
        error: (err) => {
          console.error('Błąd pobierania szczegółów firmy:', err);
          this.isLoading = false; 
        }
      });
    }
    
    this.authService.currentUser$.subscribe(user => {
        if (user && user.roles.includes('customer')) {
            this.loadFavorites();
        }
    });
  }

  loadFavorites() {
      this.favoriteService.getFavorites().subscribe({
          next: (favorites) => {
              this.favoriteVariantIds.clear();
              favorites.forEach(f => this.favoriteVariantIds.add(f.serviceVariantId));
          },
          error: (err) => console.error('Error loading favorites', err)
      });
  }

  isFavorite(variantId: number): boolean {
      return this.favoriteVariantIds.has(variantId);
  }

  get isCustomer(): boolean {
    return this.authService.isLoggedIn() && this.authService.hasRole('customer');
  }

  toggleFavorite(variant: any, event: Event) {
      event.stopPropagation();
      const variantId = variant.serviceVariantId;

      if (this.favoriteVariantIds.has(variantId)) {
          this.favoriteService.removeFavorite(variantId).subscribe({
              next: () => {
                  this.favoriteVariantIds.delete(variantId);
                  if (variant.favoritesCount > 0) variant.favoritesCount--;
                  this.toastr.info('Usunięto z ulubionych');
              },
              error: () => this.toastr.error('Błąd podczas usuwania z ulubionych')
          });
      } else {
          this.favoriteService.addFavorite(variantId).subscribe({
              next: () => {
                  this.favoriteVariantIds.add(variantId);
                  variant.favoritesCount++;
                  this.toastr.success('Dodano do ulubionych');
              },
              error: () => this.toastr.error('Błąd podczas dodawania do ulubionych')
          });
      }
  }

  loadReviews(businessId: number, loadMore = false): void {
    if (loadMore) {
      this.isLoadingMoreReviews = true;
    } else {
      this.isReviewsLoading = true;
      this.reviewsCurrentPage = 1;
      this.reviews = [];
    }

    this.reviewService.getReviews(businessId, this.reviewsCurrentPage, this.reviewsPageSize).subscribe({
      next: (data) => {
        this.reviews = [...this.reviews, ...data.items];
        this.totalReviews = data.totalCount;

        if (loadMore) {
          this.isLoadingMoreReviews = false;
        } else {
          this.isReviewsLoading = false;
        }
      },
      error: () => {
        this.isReviewsLoading = false;
        this.isLoadingMoreReviews = false;
      }
    });
  }

  onLoadMoreReviews(): void {
    this.reviewsCurrentPage++;
    if (this.business) {
      this.loadReviews(this.business.id, true);
    }
  }

  get hasMoreReviews(): boolean {
    return this.reviews.length < this.totalReviews;
  }

  openEditReviewModal(review: Review): void {
    this.reviewToEdit = review;
    this.isEditReviewModalVisible = true;
  }

  closeEditReviewModal(reviewUpdated: boolean): void {
    this.isEditReviewModalVisible = false;
    this.reviewToEdit = null;
    if (reviewUpdated && this.business) {
      this.loadReviews(this.business.id);
    }
  }

  onDeleteReview(review: Review): void {
    if (confirm('Czy na pewno chcesz usunąć swoją opinię?')) {
      this.reviewService.deleteReview(this.business!.id, review.reviewId).subscribe({
        next: () => {
          this.toastr.success('Opinia została usunięta.');
          this.loadReviews(this.business!.id);
        },
        error: () => this.toastr.error('Nie udało się usunąć opinii.')
      });
    }
  }

  openReservationModal(service: Service, pVariant: any, modal: any, employeeId?: number): void {
    if (this.business) {
      this.selectedService = { 
          ...service, 
          businessId: this.business.id,
          variants: [pVariant] 
      };
      
      this.businessService.getEmployeesForService(this.business.id, service.id).subscribe(employees => {
        this.filteredEmployees = employees;
        
        setTimeout(() => {
          modal.resetModal();
          
          if (employeeId) {
            modal.reservationForm.get('employeeId')?.setValue(employeeId.toString());
            modal.currentStep = 2; 
            modal.onDateChange();
          } else if (employees.length === 1) {
            modal.reservationForm.get('employeeId')?.setValue(employees[0].id.toString());
            modal.currentStep = 2;
          }

          modal.showModal();
        });
      });
    }
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('pl-PL', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(price) + ' zł';
  }

  getBundleOriginalPrice(bundle: ServiceBundle): number {
    return bundle.items.reduce((sum, item) => sum + (item.originalPrice || 0), 0);
  }

  getBundleDiscount(bundle: ServiceBundle): number {
    const original = this.getBundleOriginalPrice(bundle);
    if (!original || original <= bundle.totalPrice) return 0;
    return Math.round(100 - (bundle.totalPrice / original * 100));
  }

  scrollToServices(): void {
    const element = document.getElementById('services');
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }

  getInitials(name: string): string {
    if (!name) return '';
    const names = name.split(' ');
    const firstNameInitial = names[0] ? names[0][0] : '';
    const lastNameInitial = names.length > 1 ? names[names.length - 1][0] : '';
    return `${firstNameInitial}${lastNameInitial}`.toUpperCase();
  }

  startChatWithBusiness(): void {
    if (!this.business) return;

    this.chatService.startConversation(this.business.id).subscribe({
      next: () => {
        this.router.navigate(['/chat']);
      },
      error: (err) => {
        this.toastr.error('Nie udało się rozpocząć konwersacji. Spróbuj ponownie.');
        console.error(err);
      }
    });
  }

  loadBundles(businessId: number) {
      this.serviceBundleService.getBundles(businessId).subscribe({
        next: (bundles) => {
          this.bundles = bundles;
          console.log('Loaded bundles:', bundles);
        },
        error: (err) => console.error('Error loading bundles:', err)
      });
  }
  
  openBookBundleModal(bundle: ServiceBundle, modal: any) {
      this.selectedBundle = bundle;
      setTimeout(() => {
          modal.showModal();
      });
  }

  closeBookBundleModal() {
      this.isBookBundleModalOpen = false;
      this.selectedBundle = null;
  }

  openEmployeeModal(employee: Employee): void {
    this.selectedEmployee = employee;
  }

  closeEmployeeModal(): void {
    this.selectedEmployee = null;
  }

  startReservationWithEmployee(employee: Employee, modal: any): void {
    if (!this.business) return;
    
    this.filteredEmployees = this.business.employees;
    
    setTimeout(() => {
      modal.resetModal();
      modal.reservationForm.get('employeeId')?.setValue(employee.id.toString());
      modal.currentStep = 2;
      modal.showModal();
    });
    
    this.closeEmployeeModal();
  }

  callBusiness(): void {
    if (this.business && this.business.phoneNumber) {
      window.open(`tel:${this.business.phoneNumber}`, '_self');
    } else {
      this.toastr.info('Numer telefonu nie jest dostępny.');
    }
  }

  openRoute(): void {
    if (this.business) {
      const address = `${this.business.address || ''} ${this.business.city || ''}`;
      window.open(`https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(address)}`, '_blank');
    }
  }

  shareBusiness(): void {
    const url = window.location.href;
    if (navigator.share) {
      navigator.share({
        title: this.business?.name || 'BookLocal',
        url: url
      }).catch(err => console.error('Error sharing:', err));
    } else {
      navigator.clipboard.writeText(url).then(() => {
        this.toastr.success('Link do profilu skopiowany do schowka!');
      });
    }
  }

  goBack(): void {
    this.location.back();
  }
}
