import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators, ValidatorFn, AbstractControl } from '@angular/forms';
import { NgIf } from '@angular/common';
import { finalize } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../core/auth/auth.service'; // <-- fixed path

function passwordMatchValidator(): ValidatorFn {
  return (group: AbstractControl) => {
    const password = group.get('password')?.value;
    const confirm  = group.get('confirmPassword')?.value;
    return password === confirm ? null : { passwordMismatch: true };
  };
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, NgIf],
  templateUrl: './register.component.html',
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  loading = false;
  apiError = '';

  form = this.fb.nonNullable.group({
    displayName: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    passwordGroup: this.fb.nonNullable.group({
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]],
    }, { validators: passwordMatchValidator() })
  });

  get f() { return this.form.controls; }
  get pg() { return this.form.controls.passwordGroup; }
  get pw() { return this.pg.controls.password; }
  get cpw() { return this.pg.controls.confirmPassword; }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.apiError = '';

    const { displayName, email } = this.form.getRawValue();
    const password = this.pw.value;

    // adapt to your backend DTO: { email, password, displayName, role }
    this.auth.register({ email, password, displayName, role: 'RENTER' as const })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: () => this.router.navigateByUrl('/app'),
        error: (err: HttpErrorResponse) => {
          this.apiError = (err?.error?.message ?? 'Registration failed. Please try again.');
        }
      });
  }
}
