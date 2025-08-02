export interface Business {
  id: number;
  name: string;
  nip: string;
  city: string | null;
  description: string | null;
}

export interface Service {
  id: number;
  name: string;
  price: number;
  durationMinutes: number;
}

export interface Employee {
  id: number;
  firstName: string;
  lastName: string;
  position: string | null;
}

export interface BusinessDetail extends Business {
  services: Service[];
  employees: Employee[];
}