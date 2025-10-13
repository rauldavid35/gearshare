import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { ListingDto, CreateListingRequest, UpdateListingRequest } from '../models/listing.model';

@Injectable({ providedIn: 'root' })
export class ListingsService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/listings`;

  list(itemId?: string) {
    const url = itemId ? `${this.base}?itemId=${encodeURIComponent(itemId)}` : this.base;
    return this.http.get<ListingDto[]>(url);
  }
  get(id: string) { return this.http.get<ListingDto>(`${this.base}/${id}`); }
  create(req: CreateListingRequest) { return this.http.post<ListingDto>(this.base, req); }
  update(id: string, req: UpdateListingRequest) { return this.http.put<void>(`${this.base}/${id}`, req); }
  delete(id: string) { return this.http.delete<void>(`${this.base}/${id}`); }
}
