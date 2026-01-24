export interface ServiceBundleItem {
  serviceBundleItemId: number;
  serviceVariantId: number;
  variantName: string;
  serviceName: string;
  durationMinutes: number;
  sequenceOrder: number;
  originalPrice: number;
}

export interface ServiceBundle {
  serviceBundleId: number;
  businessId: number;
  name: string;
  description?: string;
  totalPrice: number;
  photoUrl?: string;
  isActive: boolean;
  items: ServiceBundleItem[];
}

export interface CreateServiceBundleItemPayload {
  serviceVariantId: number;
  sequenceOrder: number;
}

export interface CreateServiceBundlePayload {
  name: string;
  description?: string;
  totalPrice: number;
  items: CreateServiceBundleItemPayload[];
}
