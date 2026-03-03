export interface CreateReservationPayload {
  serviceVariantId: number; 
  employeeId: number;
  startTime: string;
}

export interface OwnerCreateReservationPayload {
  serviceVariantId: number;
  employeeId: number;
  startTime: string;
  guestName: string;
  guestPhoneNumber?: string;
}

export interface Reservation {
  reservationId: number;
  businessId: number;
  customerId?: string;
  startTime: string; 
  endTime: string;
  status: string;

  serviceVariantId: number;
  serviceName: string;
  variantName: string;
  agreedPrice: number;

  employeeFullName: string;
  employeePhotoUrl?: string;
  employeePhoneNumber?: string;
  employeeId: number;

  customerFullName?: string;
  customerPhotoUrl?: string;
  customerPhoneNumber?: string;
  guestName?: string;
  businessName: string;

  isServiceArchived: boolean;
  hasReview: boolean;
  paymentMethod: string;
}