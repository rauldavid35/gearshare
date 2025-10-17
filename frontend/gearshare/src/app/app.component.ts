// src/app/app.component.ts
import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { AsyncPipe, NgIf} from '@angular/common';
import { AuthService } from './core/auth/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, AsyncPipe, NgIf],
  template: `
    <nav class="w-full bg-slate-100 border-b">
      <div class="app-container flex items-center justify-between py-3">
        <a routerLink="/app" class="font-bold">GearShare</a>

        <div class="space-x-4 text-sm" *ngIf="(auth.currentUser$ | async) as u; else loggedOut">
          <span class="text-slate-600">Hi, {{ u.displayName || u.email }}</span>

          <!-- Common -->
          <a routerLink="/app/browse" class="underline">Browse</a>
          <a routerLink="/app" class="underline">Home</a>

          <!-- RENTER -->
          <a *ngIf="isRenter(u)" routerLink="/app/bookings/me" class="underline">My bookings</a>

          <!-- OWNER/ADMIN management -->
          <ng-container *ngIf="isOwnerOrAdmin(u)">
            <a routerLink="/app/items" class="underline">Items</a>
            <a routerLink="/app/listings" class="underline">Listings</a>
            <a routerLink="/app/bookings/owner" class="underline">Owner requests</a>
          </ng-container>

          <button class="underline" (click)="logout()">Logout</button>
        </div>

        <ng-template #loggedOut>
          <div class="space-x-4 text-sm">
            <a routerLink="/login" class="underline">Login</a>
            <a routerLink="/register" class="underline">Register</a>
          </div>
        </ng-template>
      </div>
    </nav>

    <router-outlet></router-outlet>
  `
})
export class AppComponent {
  auth = inject(AuthService);

  ngOnInit() {
    // hydrate currentUser$ on boot; ignore error if not logged in
    this.auth.me().subscribe({ error: () => {} });
  }

  logout() {
    this.auth.logout();
    location.href = '/login';
  }

  isOwnerOrAdmin(u: { roles?: string[] } | null | undefined): boolean {
    const roles = u?.roles ?? [];
    return roles.includes('OWNER') || roles.includes('ADMIN');
  }

  isRenter(u: { roles?: string[] } | null | undefined): boolean {
    return (u?.roles ?? []).includes('RENTER');
  }
}
