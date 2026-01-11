export interface CustomerListItem {
  profileId: number;
  userId: string;
  fullName: string;
  phoneNumber?: string;
  email?: string;
  lastVisitDate: string; // ISO Date
  nextVisitDate?: string;
  totalSpent: number;
  cancelledCount: number;
  pointsBalance: number;
  isVIP: boolean;
  isBanned: boolean;
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
