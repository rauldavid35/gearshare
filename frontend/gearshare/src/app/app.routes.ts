import { Routes } from '@angular/router';
import { AuthGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'app', pathMatch: 'full' },

  { path: 'login', loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent), title: 'Sign in' },
  { path: 'register', loadComponent: () => import('./pages/register/register.component').then(m => m.RegisterComponent), title: 'Create account' },

  {
    path: 'app',
    canActivate: [AuthGuard],
    children: [
      { path: '', loadComponent: () => import('./pages/home/home.component').then(m => m.HomeComponent), title: 'Home' },

      // Browse (renter)
      { path: 'browse', loadComponent: () => import('./pages/browse/browse.component').then(m => m.BrowseComponent), title: 'Browse' },

      // ITEMS (list / new / edit) — static before params
      { path: 'items', loadComponent: () => import('./pages/items/items-list.component').then(m => m.ItemsListComponent), title: 'Items' },
      { path: 'items/new', loadComponent: () => import('./pages/items/item-edit.component').then(m => m.ItemEditComponent), title: 'New item' },
      { path: 'items/:id', loadComponent: () => import('./pages/items/item-edit.component').then(m => m.ItemEditComponent), title: 'Edit item' },

      // LISTINGS (list / new / edit / details) — static before :id
      { path: 'listings', loadComponent: () => import('./pages/listings/listings-list.component').then(m => m.ListingsListComponent), title: 'Listings' },
      { path: 'listings/new', loadComponent: () => import('./pages/listings/listing-edit.component').then(m => m.ListingEditComponent), title: 'New listing' },
      { path: 'listings/:id/edit', loadComponent: () => import('./pages/listings/listing-edit.component').then(m => m.ListingEditComponent), title: 'Edit listing' },
      { path: 'listings/:id', loadComponent: () => import('./pages/listings/listing-details.component').then(m => m.ListingDetailsComponent), title: 'Listing details' },

      // BOOKINGS
      { path: 'bookings/me', loadComponent: () => import('./pages/bookings/bookings-renter.component').then(m => m.BookingsRenterComponent), title: 'My bookings' },
      { path: 'bookings/owner', loadComponent: () => import('./pages/bookings/bookings-owner.component').then(m => m.BookingsOwnerComponent), title: 'Owner requests' },
    ]
  },

  { path: '**', redirectTo: 'app' }
];
