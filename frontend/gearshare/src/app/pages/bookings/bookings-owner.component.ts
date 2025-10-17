import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BookingsService, BookingDto } from '../../services/bookings.service';

@Component({
  standalone: true,
  selector: 'app-bookings-owner',
  imports: [CommonModule],
  templateUrl: `./bookings-owner.component.html`
})
export class BookingsOwnerComponent {
  private api = inject(BookingsService);
  list: BookingDto[] = [];

  ngOnInit() { this.load(); }
  load() { this.api.ownerPending().subscribe(r => this.list = r); }

  act(b: BookingDto, status: 'ACCEPTED' | 'REJECTED') {
    this.api.setStatus(b.id, status).subscribe(() => this.load());
  }
}
