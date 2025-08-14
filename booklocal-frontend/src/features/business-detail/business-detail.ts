import { Component, OnInit, inject } from '@angular/core';
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

@Component({
  selector: 'app-business-detail',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule, 
    ReservationModalComponent,
    EditReviewModalComponent
  ],
  templateUrl: './business-detail.html',
  styleUrls: ['./business-detail.css']
})
export class BusinessDetailComponent implements OnInit {
  private router = inject(Router); 
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

  ngOnInit(): void {
    const businessId = this.route.snapshot.paramMap.get('id');

    if (businessId) {
      this.businessService.getBusinessById(+businessId).subscribe({
        next: (data) => {
          this.business = data;
          this.isLoading = false; 
          this.loadReviews(+businessId);
        },
        error: (err) => {
          console.error('Błąd pobierania szczegółów firmy:', err);
          this.isLoading = false; 
        }
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

  openReservationModal(service: Service, modal: any): void {
    this.selectedService = service;

    if (this.business) {
      this.businessService.getEmployeesForService(this.business.id, service.id).subscribe(employees => {
        this.filteredEmployees = employees;
        modal.showModal(); 
      });
    }
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
}