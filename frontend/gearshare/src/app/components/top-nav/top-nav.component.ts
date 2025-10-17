import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  standalone: true,
  selector: 'app-top-nav',
  imports: [CommonModule, RouterLink],
  template: `
  <header class="w-full border-b bg-white">
    <nav class="app-container max-w-6xl flex items-center gap-4 py-3">
      <a routerLink="/app/home" class="font-semibold">GearShare</a>

      <a routerLink="/app/browse" class="ml-2">Browse</a>

      <!-- USER-ONLY LINKS -->
      <ng-container *ngIf="auth.currentUser$ | async as user">
        <a *ngIf="user.roles?.includes('RENTER')" routerLink="/app/bookings/me">My bookings</a>
        <a *ngIf="user.roles?.includes('OWNER')" routerLink="/app/bookings/owner">Owner requests</a>

        <span class="ml-auto text-sm text-slate-600">
          {{ user.displayName || user.email }}
        </span>
        <button class="border rounded px-3 py-1" (click)="logout()">Logout</button>
      </ng-container>

      <!-- GUEST -->
      <ng-container *ngIf="!(auth.currentUser$ | async)">
        <span class="ml-auto"></span>
        <a routerLink="/register" class="border rounded px-3 py-1">Register</a>
        <a routerLink="/login" class="border rounded px-3 py-1">Login</a>
      </ng-container>
    </nav>
  </header>
  `
})
export class TopNavComponent {
  auth = inject(AuthService);
  private router = inject(Router);

  logout() {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }
}
