export interface Reservation {
  reservationId: number;
  customerId: string;
  startTime: string; 
  endTime: string;
  status: string;
  serviceName: string;
  employeeFullName: string;
  customerFullName?: string;
  guestName?: string;
  businessName: string;
  hasReview: boolean;
}

export interface CreateReservationPayload {
  serviceId: number;
  employeeId: number;
  startTime: string; 
}

export interface OwnerCreateReservationPayload {
  serviceId: number;
  employeeId: number;
  startTime: string;
  guestName: string;
  guestPhoneNumber?: string;
}