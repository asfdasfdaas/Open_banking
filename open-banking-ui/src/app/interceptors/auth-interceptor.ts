import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth';
import { catchError, switchMap, throwError } from 'rxjs';
import { Router } from '@angular/router';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const cloned = req.clone({ withCredentials: true });

  return next(cloned).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401
        && !req.url.includes('/refresh')
        && !req.url.includes('/login')
        && !req.url.includes('/register')
        && !req.url.includes('/logout')) {
        console.warn('Unauthorized request, attempting token refresh...', error);

        return authService.refresh().pipe(
          switchMap((success) => {
            if (success) {
              // Retry the original request 
              return next(req.clone({ withCredentials: true }));
            }
            // Refresh also failed — force logout
            authService.logout();
            router.navigate(['/login']);
            return throwError(() => error);
          })
        );
      }
      else {
        console.warn("401,")
      }
      return throwError(() => error);
    })
  );
};
