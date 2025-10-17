import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

export interface CreateBookingRequest {
  listingId: string;
  startDate: string; // YYYY-MM-DD
  endDate: string;   // YYYY-MM-DD
}

export interface BookingDto {
  id: string;
  listingId: string;
  listingTitle: string;
  renterId: string;
  startDate: string;
  endDate: string;
  totalPrice: number;
  status: 'Pending' | 'Accepted' | 'Rejected' | 'Cancelled';
}

@Injectable({ providedIn: 'root' })
export class BookingsService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/bookings`;

  create(req: CreateBookingRequest) {
    return this.http.post<BookingDto>(this.base, req);
  }
  mine() {
    return this.http.get<BookingDto[]>(`${this.base}/me`);
  }
  ownerPending() {
    return this.http.get<BookingDto[]>(`${this.base}/owner`);
  }
  setStatus(id: string, status: 'ACCEPTED' | 'REJECTED' | 'CANCELLED') {
    return this.http.patch<void>(`${this.base}/${id}/status`, { status });
  }
}
