import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ListingsService } from '../../services/listings.service';
import { ListingDto } from '../../models/listing.model';

@Component({
  standalone: true,
  selector: 'app-listings-list',
  imports: [CommonModule, RouterLink],
  templateUrl: './listings-list.component.html'
})
export class ListingsListComponent {
  private api = inject(ListingsService);
  listings: ListingDto[] = [];

  ngOnInit() { this.load(); }
  load() { this.api.list().subscribe(r => this.listings = r); }
  remove(id: string) {
    if (!confirm('Delete this listing?')) return;
    this.api.delete(id).subscribe(() => this.load());
  }
}
