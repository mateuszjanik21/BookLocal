import { Service } from "./business.model";

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
  dateOfBirth: string;
  isArchived: boolean;
  bio?: string;
  specialization?: string;
  hobbies?: string;
  instagramProfileUrl?: string;
  portfolioUrl?: string;
  isStudent: boolean;
  estimatedRevenue: number;
  completedReservationsCount: number;
  assignedServices: Service[];
  workSchedules: WorkScheduleDto[];
  certificates: EmployeeCertificateDto[];
  contracts: EmploymentContractDto[];
  payrolls: EmployeePayrollDto[];
  scheduleExceptions: ScheduleExceptionDto[];
  upcomingReservations: EmployeeReservationDto[];
  financeSettings?: FinanceSettingsDto;
}

export interface FinanceSettingsDto {
  commissionPercentage?: number;
  hourlyRate: number;
  hasPit2Filed: boolean;
  useMiddleClassRelief: boolean;
  isPensionRetired: boolean;
  voluntarySicknessInsurance: boolean;
  participatesInPPK: boolean;
  ppkEmployeeRate: number;
  ppkEmployerRate: number;
  commuteType: number;
}

export interface WorkScheduleDto {
  dayOfWeek: string;
  startTime?: string;
  endTime?: string;
  isDayOff: boolean;
}

export interface EmployeeCertificateDto {
  certificateId: number;
  name: string;
  institution?: string;
  dateObtained: string;
  imageUrl?: string;
  isVisibleToClient: boolean;
}

export interface EmploymentContractDto {
  contractId: number;
  employeeId: number;
  employeeName: string;
  contractType: string;
  baseSalary: number;
  taxDeductibleExpenses: number;
  startDate: string;
  endDate?: string;
  isActive: boolean;
}

export interface EmployeePayrollDto {
  payrollId: number;
  employeeId: number;
  employeeName: string;
  periodMonth: number;
  periodYear: number;
  grossAmount: number;
  netAmount: number;
  totalEmployerCost: number;
  status: string;
  paidAt?: string;

  baseSalary: number;
  commissionComponent: number;
  bonusComponent: number;
  pensionContribution: number;
  disabilityContribution: number;
  sicknessContribution: number;
  healthInsuranceContribution: number;
  taxAdvance: number;
  ppkAmount: number;
}

export interface ScheduleExceptionDto {
  exceptionId: number;
  dateFrom: string;
  dateTo: string;
  type: string;
  reason?: string;
  isApproved: boolean;
  blocksCalendar: boolean;
}

export interface EmployeeReservationDto {
  reservationId: number;
  startTime: string;
  endTime: string;
  serviceName: string;
  variantName: string;
  customerName?: string;
  agreedPrice: number;
  status: string;
}