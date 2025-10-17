import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ItemsService } from '../../services/items.service';

@Component({
  standalone: true,
  selector: 'app-items-list',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './items-list.component.html'
})
export class ItemsListComponent {
  private api = inject(ItemsService);
  items: any[] = [];
  q = '';

  ngOnInit() { this.load(); }

  load(): void {
    const svc = this.api as any;

    // Prefer search(q) if available; otherwise list() or getAll()
    const call$ =
      (typeof svc.search === 'function' ? svc.search(this.q || '') :
      (typeof svc.list === 'function'   ? svc.list() :
      (typeof svc.getAll === 'function' ? svc.getAll() :
        null)));

    if (!call$) throw new Error('ItemsService needs search(q), list(), or getAll()');

    call$.subscribe((r: any[]) => { this.items = r || []; });
  }

  remove(id: string): void {
    (this.api as any).delete(id).subscribe({
      next: () => this.load(),
      error: () => alert('Delete failed')
    });
  }
}
