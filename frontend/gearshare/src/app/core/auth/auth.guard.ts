import { Injectable, inject } from '@angular/core';
import {
  CanActivateFn,
  Router,
  UrlTree
} from '@angular/router';
import { AuthService } from './auth.service';
import { catchError, map, of } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthGuardClass {
  private auth = inject(AuthService);
  private router = inject(Router);

  canActivate(returnUrl: string) {
    // Fast path: if we already have a token in storage, allow.
    if (this.auth.token) return of(true);

    // Otherwise, try to hydrate the user (SSR/browser safe).
    return this.auth.me().pipe(
      map(() => true),
      catchError(() =>
        of(this.router.createUrlTree(['/login'], { queryParams: { returnUrl } }))
      )
    );
  }
}

// Angular standalone guard wrapper
export const AuthGuard: CanActivateFn = (route, state): boolean | UrlTree | import('rxjs').Observable<boolean | UrlTree> => {
  const guard = inject(AuthGuardClass);
  return guard.canActivate(state.url);
};
