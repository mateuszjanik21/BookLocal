export interface Reservation {
  reservationId: number;
  customerId: string;
  startTime: string; 
  endTime: string;
  status: string;
  serviceName: string;
  employeeFullName: string;
  customerFullName: string;
  businessName: string;
}

export interface CreateReservationPayload {
  serviceId: number;
  employeeId: number;
  startTime: string; 
}