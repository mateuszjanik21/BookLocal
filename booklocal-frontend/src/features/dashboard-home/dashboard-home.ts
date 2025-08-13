import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { forkJoin } from 'rxjs';
import { map } from 'rxjs/operators';
import { ReservationService } from '../../core/services/reservation';
import { BusinessService } from '../../core/services/business-service';
import { ReviewService } from '../../core/services/review';
import { Reservation } from '../../types/reservation.model';
import { Review } from '../../types/review.model';
import { BusinessDetail } from '../../types/business.model';

@Component({
  selector: 'app-dashboard-home',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard-home.html',
})
export class DashboardHomeComponent implements OnInit, OnDestroy {
  private reservationService = inject(ReservationService);
  private businessService = inject(BusinessService);
  private reviewService = inject(ReviewService);

  public currentDate = new Date();
  private clockInterval: any;

  isLoading = true;
  business: BusinessDetail | null = null;
  todaysReservations: Reservation[] = [];
  latestReviews: Review[] = [];
  stats = {
    upcomingCount: 0,
    employeeCount: 0,
    serviceCount: 0
  };

  ngOnInit(): void {
    this.clockInterval = setInterval(() => {
      this.currentDate = new Date();
    }, 1000);

    this.businessService.getMyBusiness().subscribe(businessDetails => {
      this.business = businessDetails;

      forkJoin({
        reservations: this.reservationService.getCalendarEvents(),
        reviews: this.reviewService.getReviews(businessDetails.id)
      }).pipe(
        map(({ reservations, reviews }) => {
          const now = new Date();
          const todayStart = new Date(now.setHours(0, 0, 0, 0));
          const todayEnd = new Date(now.setHours(23, 59, 59, 999));

          this.todaysReservations = reservations
            .filter(r => new Date(r.startTime) >= todayStart && new Date(r.startTime) <= todayEnd)
            .sort((a, b) => new Date(a.startTime).getTime() - new Date(b.startTime).getTime());
          
          this.latestReviews = reviews.slice(0, 3);
          
          this.stats.upcomingCount = reservations.filter(r => new Date(r.startTime) >= new Date()).length;
          this.stats.employeeCount = businessDetails.employees.length;
          this.stats.serviceCount = businessDetails.categories.flatMap(c => c.services).length;
        })
      ).subscribe(() => {
        this.isLoading = false;
      });
    });
  }
  
  ngOnDestroy(): void {
    if (this.clockInterval) {
      clearInterval(this.clockInterval);
    }
  }
}