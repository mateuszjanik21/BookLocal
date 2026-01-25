export interface DailyEmployeePerformance {
  employeeId: number;
  date: string;
  fullName: string;
  totalAppointments: number;
  completedAppointments: number;
  cancelledAppointments: number;
  totalRevenue: number;
  commission: number;
  averageRating: number;
}
