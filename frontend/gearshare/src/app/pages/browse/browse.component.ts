import { Component, inject } from '@angular/core';
import { CommonModule, NgFor } from '@angular/common';
import { ListingsService } from '../../services/listings.service';
import { ListingDto } from '../../models/listing.model';
import { environment } from '../../../environments/environment';

@Component({
  standalone: true,
  selector: 'app-browse',
  imports: [CommonModule, NgFor],
  templateUrl: './browse.component.html'
})
export class BrowseComponent {
  private api = inject(ListingsService);
  listings: ListingDto[] = [];

  // http://localhost:5131  (strip trailing /api if present)
  apiOrigin = environment.apiUrl.replace(/\/api\/?$/, '');

  ngOnInit() {
    this.api.list().subscribe(r => {
      this.listings = (r ?? []).filter(x => x.active);
    });
  }

  toImg(path?: string | null): string {
    if (!path) return '';
    if (/^https?:\/\//i.test(path)) return path;          // already absolute
    return `${this.apiOrigin}${path.startsWith('/') ? path : '/' + path}`; // prefix API origin
  }
}
