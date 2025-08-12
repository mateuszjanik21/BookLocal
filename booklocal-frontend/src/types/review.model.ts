export interface Review {
  reviewId: number;
  rating: number;
  comment?: string;
  reviewerName: string;
  createdAt: string;
  userId: string;
  reviewerPhotoUrl?: string;
  serviceName?: string;
  employeeFullName?: string;
}

export interface CreateReviewPayload {
  rating: number;
  comment?: string;
}

export interface UpdateReviewPayload {
  rating: number;
  comment?: string;
}