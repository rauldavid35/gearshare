import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="app-container">
      <h1 class="text-2xl font-bold mb-4">Welcome to GearShare</h1>
      <p class="text-slate-600">You're logged in. CRUD pages come next.</p>
      <button class="mt-6 underline" (click)="logout()">Logout</button>
    </div>
  `
})
export class HomeComponent {
  logout() { localStorage.removeItem('auth'); location.href = '/login'; }
}
