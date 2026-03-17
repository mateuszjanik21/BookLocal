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
  isActive: boolean;
  favoritesCount: number;
}

export interface Service {
  id: number;
  name: string;
  description?: string;
  serviceCategoryId: number;
  businessId: number;
  isArchived: boolean;
  variants: ServiceVariant[];
}

export interface ServiceCategory {
  serviceCategoryId: number;
  name: string;
  photoUrl?: string;
  isArchived: boolean;
  services: Service[];
}

export interface EmployeeCertificate {
  certificateId: number;
  name: string;
  institution?: string;
  dateObtained: string;
  imageUrl?: string;
  isVisibleToClient: boolean;
}

export interface Employee {
  id: number;
  firstName: string;
  lastName: string;
  position: string | null;
  photoUrl?: string;
  dateOfBirth: string;
  specialization?: string;
  bio?: string;
  instagramProfileUrl?: string;
  portfolioUrl?: string;
  isArchived: boolean;
  isStudent?: boolean;
  certificates?: EmployeeCertificate[];
  assignedServicesCount?: number;
  completedReservationsCount?: number;
  activeContractType?: string;
  estimatedMonthlyRevenue?: number;
  assignedServiceIds?: number[];
}

export interface Business {
  id: number;
  name: string;
  nip: string;
  city: string | null;
  address: string | null;
  phoneNumber: string | null;
  description: string | null;
  photoUrl?: string;
}

export interface BusinessDetail extends Business {
  isVerified: boolean;
  averageRating: number;
  reviewCount: number;
  owner: {
    firstName?: string;
    lastName?: string;
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
  subscriptionPlanName?: string;
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
  mainCategoryName?: string;
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

export interface RebookSuggestion {
  serviceCategoryId: number;
  categoryName: string;
  categoryPhotoUrl: string | null;
  businessId: number;
  businessName: string;
  businessCity: string | null;
  employeeName: string;
  employeePhotoUrl: string | null;
  lastVisitDate: string;
  visitCount: number;
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