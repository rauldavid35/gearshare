// src/app/app.routes.ts
import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { AuthGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },

  { path: 'login', component: LoginComponent, title: 'Sign in' },
  { path: 'register', component: RegisterComponent, title: 'Create account' },

  {
    path: 'app',
    canActivate: [AuthGuard],
    loadComponent: () => import('./pages/home/home.component').then(m => m.HomeComponent),
    title: 'Home'
  },

  { path: 'browse', loadComponent: () => import('./pages/browse/browse.component').then(m => m.BrowseComponent), title: 'Browse' },


  // Items CRUD
  {
    path: 'app/items',
    canActivate: [AuthGuard],
    loadComponent: () => import('./pages/items/items-list.component').then(m => m.ItemsListComponent),
    title: 'Items'
  },
  {
    path: 'app/items/new',
    canActivate: [AuthGuard],
    loadComponent: () => import('./pages/items/item-edit.component').then(m => m.ItemEditComponent),
    title: 'New Item'
  },
  {
    path: 'app/items/:id/edit',
    canActivate: [AuthGuard],
    loadComponent: () => import('./pages/items/item-edit.component').then(m => m.ItemEditComponent),
    title: 'Edit Item'
  },

  // Listings CRUD
  {
    path: 'app/listings',
    canActivate: [AuthGuard],
    loadComponent: () => import('./pages/listings/listings-list.component').then(m => m.ListingsListComponent),
    title: 'Listings'
  },
  {
    path: 'app/listings/new',
    canActivate: [AuthGuard],
    loadComponent: () => import('./pages/listings/listing-edit.component').then(m => m.ListingEditComponent),
    title: 'New Listing'
  },
  {
    path: 'app/listings/:id/edit',
    canActivate: [AuthGuard],
    loadComponent: () => import('./pages/listings/listing-edit.component').then(m => m.ListingEditComponent),
    title: 'Edit Listing'
  },

  { path: '**', redirectTo: 'login' },
];
