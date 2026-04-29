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
      // Only attempt refresh on 401, and don't loop on the refresh/login endpoints
      if (error.status === 401
        && (!req.url.includes('/refresh')
        && !req.url.includes('/login'))) {
        console.warn('Unauthorized request, attempting token refresh...', error);

        return authService.refresh().pipe(
          switchMap((success) => {
            if (success) {
              // Retry the original request now that we have a fresh JWT
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
