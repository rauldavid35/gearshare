import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink, Router } from '@angular/router';
import { AuthService } from './core/auth/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink],
  templateUrl: './app.html',
})
export class App {
  private auth = inject(AuthService);
  private router = inject(Router);

  logout(ev: Event) {
    ev.preventDefault();
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }
}
