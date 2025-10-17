import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ListingsService } from '../../services/listings.service';
import { ItemsService } from '../../services/items.service';
import { ItemDto } from '../../models/item.model';
import { AuthService } from '../../core/auth/auth.service';

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
  private auth = inject(AuthService);

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
    // Only show items owned by the current user → avoids 403 when creating listings
    this.items.list().subscribe(items => {
      const me = (this.auth as any)._currentUser?.value; // BehaviorSubject snapshot
      const myId = me?.id as string | undefined;
      this.allItems = myId ? items.filter(i => i.ownerId === myId) : [];
    });

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
}
