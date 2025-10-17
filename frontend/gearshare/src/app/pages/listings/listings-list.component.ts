import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ListingsService } from '../../services/listings.service';

@Component({
  standalone: true,
  selector: 'app-listings-list',
  imports: [CommonModule, RouterLink],
  templateUrl: './listings-list.component.html'
})
export class ListingsListComponent {
  private api = inject(ListingsService);
  listings: any[] = [];

  ngOnInit() { this.load(); }

  load(): void {
    const svc = this.api as any;
    const call$ =
      (typeof svc.list === 'function'   ? svc.list() :
      (typeof svc.search === 'function' ? svc.search('') :
      (typeof svc.getAll === 'function' ? svc.getAll() :
        null)));

    if (!call$) throw new Error('ListingsService needs list(), search(""), or getAll()');

    call$.subscribe((r: any[]) => { this.listings = r || []; });
  }

  remove(id: string): void {
    (this.api as any).delete(id).subscribe({
      next: () => this.load(),
      error: () => alert('Delete failed')
    });
  }
}
