export interface Business {
  id: number;
  name: string;
  nip: string;
  city: string | null;
  address: string | null;
  description: string | null;
  photoUrl?: string;
}

export interface Service {
  id: number;
  name: string;
  price: number;
  durationMinutes: number;
  serviceCategoryId: number; 
  businessId: number;
}

export interface ServiceCategory {
  serviceCategoryId: number;
  name: string;
  photoUrl?: string;
  services: Service[];
}

export interface Employee {
  id: number;
  firstName: string;
  lastName: string;
  position: string | null;
  photoUrl?: string;
}

export interface BusinessDetail extends Business {
  services: Service[];
  employees: Employee[];
  categories: ServiceCategory[];
  averageRating: number;
  reviewCount: number;
  owner: Owner;
}

export interface ServiceCategoryFeed {
  serviceCategoryId: number;
  name: string;
  photoUrl?: string;
  services: Service[];
  businessId: number;
  businessName: string;
  businessCity: string | null;
}

export interface ServicePayload {
  name: string;
  description?: string;
  price: number;
  durationMinutes: number;
  serviceCategoryId: number;
}

export interface Owner {
  firstName: string;
}