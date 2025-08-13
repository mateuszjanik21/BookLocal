export interface Business {
  id: number;
  name: string;
  nip: string;
  city: string | null;
  address: string | null;
  description: string | null;
  photoUrl?: string;
}

export interface ServiceSearchResult {
  serviceId: number;
  serviceName: string;
  price: number;
  durationMinutes: number;
  businessId: number;
  businessName: string;
  businessCity: string;
  mainCategoryName: string;
  averageRating: number;
  reviewCount: number;
}

export interface BusinessSearchResult {
  businessId: number;
  name: string;
  city: string | null;
  photoUrl: string | null;
  averageRating: number;
  reviewCount: number;
  mainCategories: string[];
}

export interface ServiceCategorySearchResult {
  serviceCategoryId: number;
  name: string;
  photoUrl: string | null;
  businessId: number;
  businessName: string;
  businessCity: string | null;
  averageRating: number;
  reviewCount: number;
  businessCreatedAt: string;
  services: Service[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface Service {
  id: number;
  name: string;
  price: number;
  durationMinutes: number;
  serviceCategoryId: number; 
  businessId: number;
  isArchived: boolean;
}

export interface ServiceCategory {
  serviceCategoryId: number;
  name: string;
  photoUrl?: string;
  services: Service[];
}

export interface MainCategory {
  mainCategoryId: number;
  name: string;
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