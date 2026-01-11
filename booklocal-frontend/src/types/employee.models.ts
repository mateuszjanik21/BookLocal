import { Service} from "./business.model";

export interface EmployeePayload {
  firstName: string;
  lastName: string;
  position?: string;
  dateOfBirth: string;
  bio?: string;
  specialization?: string;
  instagramProfileUrl?: string;
  portfolioUrl?: string;
  isStudent: boolean;
}

export interface EmployeeDetail {
  id: number;
  firstName: string;
  lastName: string;
  position?: string;
  photoUrl?: string;
  
  bio?: string;
  specialization?: string;
  instagramProfileUrl?: string;
  portfolioUrl?: string;

  estimatedRevenue: number;
  
  assignedServices: Service[];
  workSchedules: WorkScheduleDto[];
}

export interface WorkScheduleDto {
  dayOfWeek: number;
  startTime?: string;
  endTime?: string;
  isDayOff: boolean;
}