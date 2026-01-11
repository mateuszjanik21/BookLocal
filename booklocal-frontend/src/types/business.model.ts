export interface MainCategory {
  mainCategoryId: number;
  name: string;
}

export interface ServiceVariant {
  serviceVariantId: number;
  name: string;
  price: number;
  durationMinutes: number;
  cleanupTimeMinutes: number;
  isDefault: boolean;
}

export interface Service {
  id: number;
  name: string;
  description?: string;
  serviceCategoryId: number;
  businessId: number; // Required by frontend logic
  isArchived: boolean;
  variants: ServiceVariant[];
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
  dateOfBirth: string;
  specialization?: string;
  instagramProfileUrl?: string;
  portfolioUrl?: string;
  isStudent: boolean;
}

export interface Business {
  id: number;
  name: string;
  nip: string;
  city: string | null;
  address: string | null;
  description: string | null;
  photoUrl?: string;
}

export interface BusinessDetail extends Business {
  isVerified: boolean;
  averageRating: number;
  reviewCount: number;
  owner: {
    firstName?: string;
  };
  categories: ServiceCategory[];
  employees: Employee[];
}

export interface BusinessSearchResult {
  businessId: number;
  name: string;
  city: string | null;
  photoUrl: string | null;
  averageRating: number;
  reviewCount: number;
  isVerified: boolean;
  mainCategories: string[];
}

export interface ServiceSearchResult {
  serviceId: number;
  defaultServiceVariantId: number;
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

export interface ServiceCategoryFeed {
  serviceCategoryId: number;
  name: string;
  photoUrl?: string;
  services: Service[];
  businessId: number;
  businessName: string;
  businessCity: string | null;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface ServiceVariantPayload {
  serviceVariantId?: number;
  name: string;
  price: number;
  durationMinutes: number;
  cleanupTimeMinutes?: number;
  isDefault: boolean;
}

export interface ServicePayload {
  name: string;
  description?: string;
  serviceCategoryId: number;
  variants: ServiceVariantPayload[];
}