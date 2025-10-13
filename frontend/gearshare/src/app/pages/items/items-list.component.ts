import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ItemsService } from '../../services/items.service';
import { ItemDto } from '../../models/item.model';

@Component({
  standalone: true,
  selector: 'app-items-list',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './items-list.component.html'
})
export class ItemsListComponent {
  private api = inject(ItemsService);
  items: ItemDto[] = [];
  q = '';

  ngOnInit() { this.load(); }
  load() { this.api.list(this.q).subscribe(r => this.items = r); }
  remove(id: string) {
    if (!confirm('Delete this item?')) return;
    this.api.delete(id).subscribe(() => this.load());
  }
}
