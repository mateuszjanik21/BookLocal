import { Reservation } from "./reservation.model";
import { Review } from "./review.model";

export interface DashboardStats {
  upcomingReservationsCount: number;
  clientCount: number;
  employeeCount: number;
  serviceCount: number;
  hasVariants: boolean;
}

export interface DashboardData {
  stats: DashboardStats;
  todaysReservations: Reservation[];
  latestReviews: Review[];
}
