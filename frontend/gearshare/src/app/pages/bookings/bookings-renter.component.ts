import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BookingsService, BookingDto } from '../../services/bookings.service';

@Component({
  standalone: true,
  selector: 'app-bookings-renter',
  imports: [CommonModule],
  templateUrl: `./bookings-renter.component.html`
})
export class BookingsRenterComponent {
  private api = inject(BookingsService);
  list: BookingDto[] = [];

  ngOnInit() {
    this.api.mine().subscribe(r => this.list = r);
  }
}
