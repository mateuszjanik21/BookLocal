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
  user: UserDto;
}

export interface EntrepreneurRegisterPayload {
  email: string;
  password:  string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  businessName: string;
  nip: string;
  address: string;
  city: string;
  description: string;
}

export interface UserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  photoUrl?: string;
  phoneNumber?: string;
}

export interface ChangePasswordPayload {
  currentPassword:  string;
  newPassword: string;
}