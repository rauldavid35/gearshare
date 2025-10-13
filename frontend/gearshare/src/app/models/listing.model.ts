export interface ListingDto {
  id: string;
  itemId: string;
  itemTitle?: string | null;   // NEW
  coverImage?: string | null;  // NEW
  pricePerDay: number;
  deposit: number;
  locationCity?: string | null;
  locationLat?: number | null;
  locationLng?: number | null;
  active: boolean;
}

export interface CreateListingRequest {
  itemId: string;
  pricePerDay: number;
  deposit: number;
  locationCity?: string | null;
  locationLat?: number | null;
  locationLng?: number | null;
  active: boolean;
}
export type UpdateListingRequest = Omit<CreateListingRequest, 'itemId'>;
