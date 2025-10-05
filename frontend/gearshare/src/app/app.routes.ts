import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { AuthGuard } from './core/auth/auth.guard'; // <-- add this

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: 'login', component: LoginComponent, title: 'Sign in' },
  { path: 'register', component: RegisterComponent, title: 'Create account' },
  {
    path: 'app',
    canActivate: [AuthGuard],                                  // <-- protect it
    loadComponent: () =>
      import('./pages/home/home.component').then(m => m.HomeComponent),
    title: 'Home'
  },
  { path: '**', redirectTo: 'login' },
];
