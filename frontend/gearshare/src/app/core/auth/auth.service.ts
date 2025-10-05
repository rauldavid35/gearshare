import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export type UserDto = { id: string; email: string; displayName?: string; roles: string[] };
export type AuthResponse = { token: string; expiresAt: string; user: UserDto };

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly isBrowser = isPlatformBrowser(this.platformId);

  private base = `${environment.apiUrl}/auth`;
  private _currentUser = new BehaviorSubject<UserDto | null>(null);
  currentUser$ = this._currentUser.asObservable();

  constructor(private http: HttpClient) {
    // Only touch localStorage in the browser
    if (this.isBrowser) {
      const raw = localStorage.getItem('auth');
      if (raw) {
        try {
          const parsed = JSON.parse(raw) as AuthResponse;
          this._currentUser.next(parsed.user);
        } catch {
          /* ignore */
        }
      }
    }
  }

  register(payload: { email: string; password: string; displayName: string; role: 'OWNER' | 'RENTER' }) {
    return this.http
      .post<AuthResponse>(`${this.base}/register`, payload)
      .pipe(tap(res => this.persist(res)));
  }

  login(payload: { email: string; password: string }) {
    return this.http
      .post<AuthResponse>(`${this.base}/login`, payload)
      .pipe(tap(res => this.persist(res)));
  }

  me() {
    return this.http.get<UserDto>(`${this.base}/me`).pipe(tap(u => this._currentUser.next(u)));
  }

  logout() {
    if (this.isBrowser) localStorage.removeItem('auth');
    this._currentUser.next(null);
  }

  get token(): string | null {
    if (!this.isBrowser) return null;
    const raw = localStorage.getItem('auth');
    if (!raw) return null;
    try {
      return (JSON.parse(raw) as AuthResponse).token;
    } catch {
      return null;
    }
  }

  private persist(res: AuthResponse) {
    if (this.isBrowser) localStorage.setItem('auth', JSON.stringify(res));
    this._currentUser.next(res.user);
  }
}
