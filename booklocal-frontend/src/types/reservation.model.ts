export interface Reservation {
  reservationId: number;
  startTime: string; 
  endTime: string;
  status: string;
  serviceName: string;
  employeeFullName: string;
  customerFullName: string;
}