import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ListingsService } from '../../services/listings.service';

@Component({
  standalone: true,
  selector: 'app-browse',
  imports: [CommonModule, RouterLink],
  templateUrl: './browse.component.html'
})
export class BrowseComponent {
  private api = inject(ListingsService);
  listings: any[] = [];

  ngOnInit() { this.load(); }

  private load(): void {
    // Try common method names in order: search(''), list(), getAll()
    const svc = this.api as any;

    const call$ =
      (typeof svc.search === 'function' ? svc.search('') :
      (typeof svc.list === 'function'   ? svc.list() :
      (typeof svc.getAll === 'function' ? svc.getAll() :
        null)));

    if (!call$) {
      // Last-resort: throw a clear error early in dev
      throw new Error('ListingsService needs search(""), list(), or getAll()');
    }

    call$.subscribe((r: any[]) => { this.listings = r || []; });
  }
}
