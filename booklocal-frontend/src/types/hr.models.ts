export enum ContractType {
  EmploymentContract = 0,
  B2B = 1,
  MandateContract = 2,
  Apprenticeship = 3
}

export enum PayrollStatus {
  Draft = 0,
  Calculated = 1,
  Approved = 2,
  Paid = 3
}

export interface EmploymentContract {
  contractId: number;
  employeeId: number;
  employeeName: string;
  contractType: ContractType;
  baseSalary: number;
  taxDeductibleExpenses: number;
  startDate: string;
  endDate?: string;
  isActive: boolean;
}

export interface EmploymentContractUpsert {
  employeeId: number;
  contractType: ContractType;
  baseSalary: number;
  taxDeductibleExpenses: number;
  startDate: string;
  endDate?: string;
}

export interface EmployeePayroll {
  payrollId: number;
  employeeId: number;
  employeeName: string;
  periodMonth: number;
  periodYear: number;
  grossAmount: number;
  netAmount: number;
  totalEmployerCost: number;
  status: PayrollStatus;
  paidAt?: string;
}

export interface GeneratePayrollRequest {
  employeeId: number;
  month: number;
  year: number;
}
