import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';

import { ListingsService } from '../../services/listings.service';
import { ItemsService } from '../../services/items.service';
import { BookingsService } from '../../services/bookings.service';

import type { ListingDto } from '../../models/listing.model';
import type { ItemDto } from '../../models/item.model';

@Component({
  selector: 'app-listing-details',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './listing-details.component.html'
})
export class ListingDetailsComponent {
  private route = inject(ActivatedRoute);
  private listings = inject(ListingsService);
  private items = inject(ItemsService);
  private bookings = inject(BookingsService);

  loading = signal(true);
  error = signal<string | null>(null);
  listing = signal<ListingDto | null>(null);
  item = signal<ItemDto | null>(null);

  // for [(ngModel)]
  startDate: string = '';
  endDate: string = '';

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.fetch(id);
  }

  private fetch(id: string) {
    this.loading.set(true);
    this.error.set(null);

    this.listings.get(id).subscribe({
      next: (l) => {
        this.listing.set(l);
        this.items.get(l.itemId).subscribe({
          next: (it) => { this.item.set(it); this.loading.set(false); },
          error: (e: any) => { this.error.set(e?.error ?? 'Failed to load item'); this.loading.set(false); }
        });
      },
      error: (e: any) => { this.error.set(e?.error ?? 'Listing not found'); this.loading.set(false); }
    });
  }

  get days(): number {
  if (!this.startDate || !this.endDate) return 0;

  // Parse "YYYY-MM-DD" without timezone and compute in UTC to avoid DST jumps
  const [ys, ms, ds] = this.startDate.split('-').map(Number);
  const [ye, me, de] = this.endDate.split('-').map(Number);

  // Date.UTC(month is 0-based)
  const sd = Date.UTC(ys, (ms ?? 1) - 1, ds ?? 1);
  const ed = Date.UTC(ye, (me ?? 1) - 1, de ?? 1);

  const diff = Math.floor((ed - sd) / 86_400_000) + 1; // inclusive range
  return Number.isFinite(diff) && diff > 0 ? diff : 0;
}


  get totalPrice(): number {
  const l = this.listing();
  if (!l || this.days <= 0) return 0;
  return this.days * Number(l.pricePerDay) + Number(l.deposit || 0);
}


  requestBooking() {
    const l = this.listing();
    if (!l) return;
    if (!this.startDate || !this.endDate) { alert('Select start & end dates'); return; }

    this.bookings.create({
      listingId: l.id,
      startDate: this.startDate,
      endDate: this.endDate
    }).subscribe({
      next: () => {
        alert('Booking requested! The owner will accept or reject.');
        this.startDate = '';
        this.endDate = '';
      },
      error: (e: any) => alert(e?.error ?? 'Booking failed')
    });
  }
}
