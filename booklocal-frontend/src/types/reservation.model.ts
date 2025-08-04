export interface Reservation {
  reservationId: number;
  customerId: string;
  startTime: string; 
  endTime: string;
  status: string;
  serviceName: string;
  employeeFullName: string;
  customerFullName: string;
}