import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ListingsService } from '../../services/listings.service';
import { ItemsService } from '../../services/items.service';
import { ItemDto } from '../../models/item.model';

@Component({
  standalone: true,
  selector: 'app-listing-edit',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './listing-edit.component.html'
})
export class ListingEditComponent {
  private fb = inject(FormBuilder);
  private listings = inject(ListingsService);
  private items = inject(ItemsService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  id = this.route.snapshot.paramMap.get('id');
  allItems: ItemDto[] = [];

  form = this.fb.nonNullable.group({
    itemId: ['', [Validators.required]],
    pricePerDay: [0, [Validators.required, Validators.min(0)]],
    deposit: [0, [Validators.required, Validators.min(0)]],
    locationCity: [''],
    locationLat: [null as number | null],
    locationLng: [null as number | null],
    active: [true]
  });

  ngOnInit() {
    // load items for dropdown
    this.items.list().subscribe(r => (this.allItems = r));

    if (this.id) {
      this.listings.get(this.id).subscribe(l =>
        this.form.patchValue({
          itemId: l.itemId,
          pricePerDay: l.pricePerDay,
          deposit: l.deposit,
          locationCity: l.locationCity ?? '',
          locationLat: l.locationLat ?? null,
          locationLng: l.locationLng ?? null,
          active: l.active
        })
      );
    }
  }

  save() {
    const v = this.form.getRawValue();

    if (this.id) {
      // update
      this.listings
        .update(this.id, {
          pricePerDay: v.pricePerDay,
          deposit: v.deposit,
          locationCity: v.locationCity || null,
          locationLat: v.locationLat,
          locationLng: v.locationLng,
          active: v.active
        })
        .subscribe(() => this.router.navigateByUrl('/app/listings'));
    } else {
      // create → go to edit page of new listing
      this.listings
        .create({
          itemId: v.itemId,
          pricePerDay: v.pricePerDay,
          deposit: v.deposit,
          locationCity: v.locationCity || null,
          locationLat: v.locationLat,
          locationLng: v.locationLng,
          active: v.active
        })
        .subscribe(newL => this.router.navigate(['/app/listings', newL.id, 'edit']));
    }
  }

  // Stub to satisfy (change)="onFile($event)" if it's present in your template.
  // Listings don't support file uploads; images belong to Items.
  onFile(e: Event) {
    (e.target as HTMLInputElement).value = '';
    alert('Listings don’t support file upload. Upload images on the Item edit page.');
  }
}
