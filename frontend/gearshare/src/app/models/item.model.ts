export enum ItemCategory { Sports = 0, Photo = 1, DIY = 2, Other = 3 }

export interface ItemDto {
  id: string;
  title: string;
  description?: string | null;
  category: ItemCategory;
  condition: string;
  ownerId: string;
  ratingAvg?: number | null;
  images: string[];         // relative URLs from API (e.g., /uploads/items/...)
  listingsCount: number;
}

export interface CreateItemRequest {
  title: string;
  description?: string | null;
  category: ItemCategory;
  condition: string;
}
export type UpdateItemRequest = CreateItemRequest;
