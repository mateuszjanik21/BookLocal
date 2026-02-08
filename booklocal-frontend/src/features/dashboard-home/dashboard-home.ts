import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReservationService } from '../../core/services/reservation';
import { BusinessService } from '../../core/services/business-service';
import { ReviewService } from '../../core/services/review';
import { CustomerService } from '../../core/services/customer-service';
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
  private businessService = inject(BusinessService);

  public currentDate = new Date();
  private clockInterval: any;

  isLoading = true;
  business: BusinessDetail | null = null;
  todaysReservations: Reservation[] = [];
  latestReviews: Review[] = [];
  hasVariants = false;

  stats = {
    upcomingReservationsCount: 0,
    employeeCount: 0,
    serviceCount: 0,
    clientCount: 0
  };

  ngOnInit(): void {
    this.clockInterval = setInterval(() => {
      this.currentDate = new Date();
    }, 1000);

    this.businessService.getMyBusiness().subscribe(businessDetails => {
      this.business = businessDetails;
    });

    this.businessService.getDashboardData().subscribe((data: any) => {
        this.stats = data.stats;
        this.todaysReservations = data.todaysReservations;
        this.latestReviews = data.latestReviews;
        this.hasVariants = data.stats.hasVariants;
        this.isLoading = false;
    });
  }
  
  ngOnDestroy(): void {
    if (this.clockInterval) {
      clearInterval(this.clockInterval);
    }
  }
}