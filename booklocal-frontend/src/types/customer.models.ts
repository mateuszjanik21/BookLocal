export interface CustomerListItem {
  profileId: number;
  userId: string;
  fullName: string;
  phoneNumber?: string;
  email?: string;
  photoUrl?: string;
  lastVisitDate: string; 
  nextVisitDate?: string;
  totalSpent: number;
  isVIP: boolean;
  isBanned: boolean;
  cancelledCount: number;
  pointsBalance: number;
}

export interface CustomerDetail extends CustomerListItem {
  privateNotes?: string;
  allergies?: string;
  formulas?: string;
  noShowCount: number;
  visitCount: number;
  history: ReservationHistory[];
}

export interface ReservationHistory {
  reservationId: number;
  startTime: string;
  serviceName: string;
  employeeName: string;
  price: number;
  status: string;
}

export interface UpdateCustomerNotePayload {
  privateNotes?: string;
  allergies?: string;
  formulas?: string;
}

export interface UpdateCustomerStatusPayload {
  isVIP: boolean;
  isBanned: boolean;
}

export enum CustomerStatusFilter {
    All = 0,
    VIP = 1,
    Banned = 2,
    Standard = 3
}

export enum CustomerHistoryFilter {
    All = 0,
    WithHistory = 1,
    WithoutHistory = 2
}

export enum CustomerSpentFilter {
    All = 0,
    Any = 1,
    Over100 = 2,
    Over500 = 3,
    Over1000 = 4
}
