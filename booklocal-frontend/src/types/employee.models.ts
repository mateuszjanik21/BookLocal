import { Service } from "./business.model";

export interface EmployeePayload {
  firstName: string;
  lastName: string;
  position?: string;
}

export interface EmployeeDetail {
  id: number;
  firstName: string;
  lastName: string;
  position?: string;
  photoUrl?: string;
  estimatedRevenue: number;
  assignedServices: Service[];
  workSchedules: WorkSchedule[];
}

export interface WorkSchedule {
  dayOfWeek: number;
  startTime?: string;
  endTime?: string;
  isDayOff: boolean;
}