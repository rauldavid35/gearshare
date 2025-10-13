import { Component, inject } from '@angular/core';
import { CommonModule, NgFor, NgIf } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ItemsService } from '../../services/items.service';
import { ItemCategory, ItemDto } from '../../models/item.model';

@Component({
  standalone: true,
  selector: 'app-item-edit',
  imports: [CommonModule, ReactiveFormsModule, NgFor, NgIf],
  templateUrl: './item-edit.component.html'
})
export class ItemEditComponent {
  private fb = inject(FormBuilder);
  private api = inject(ItemsService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  id = this.route.snapshot.paramMap.get('id');
  item?: ItemDto;

  categories = [
    { value: ItemCategory.Sports, label: 'Sports' },
    { value: ItemCategory.Photo,  label: 'Photo' },
    { value: ItemCategory.DIY,    label: 'DIY' },
    { value: ItemCategory.Other,  label: 'Other' },
  ];

  form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(120)]],
    description: [''],
    category: [ItemCategory.Sports, [Validators.required]],
    condition: ['GOOD', [Validators.required]]
  });

  ngOnInit() {
    if (this.id) this.loadItem();
  }

  private loadItem() {
    this.api.get(this.id!).subscribe(i => {
      this.item = i;
      this.form.patchValue({
        title: i.title,
        description: i.description ?? '',
        category: i.category,
        condition: i.condition
      });
    });
  }

  save() {
    const v = this.form.getRawValue();

    if (this.id) {
      this.api.update(this.id, v).subscribe(() => this.router.navigateByUrl('/app/items'));
    } else {
      this.api.create(v).subscribe(newItem => this.router.navigate(['/app/items', newItem.id, 'edit']));
    }
  }

  onFile(e: Event) {
    const file = (e.target as HTMLInputElement).files?.[0];
    if (!file) return;

    if (!this.id) {
      alert('Save the item first, then upload images.');
      (e.target as HTMLInputElement).value = '';
      return;
    }

    this.api.uploadImage(this.id, file).subscribe({
      next: () => {
        (e.target as HTMLInputElement).value = '';
        this.loadItem(); // refresh to show new image (URLs are absolute from API)
      },
      error: (err) => {
        console.error('Upload failed', err);
        (e.target as HTMLInputElement).value = '';
        alert('Upload failed.');
      }
    });
  }
}
