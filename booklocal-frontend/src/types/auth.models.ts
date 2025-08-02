export interface RegisterPayload {
  email: string;
  password:  string;
  firstName: string;
  lastName: string;
}

export interface LoginPayload {
  email: string;
  password:  string;
}

export interface AuthResponse {
  token: string;
  user: any;
}

export interface EntrepreneurRegisterPayload {
  email: string;
  password:  string;
  firstName: string;
  lastName: string;
  businessName: string;
  nip: string;
  address: string;
  city: string;
  description: string;
}