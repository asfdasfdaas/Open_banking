import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  // Automatically attach cookies to every outgoing request
  const cloned = req.clone({ withCredentials: true });

  return next(cloned).pipe(
    catchError((error: HttpErrorResponse) => {

      // If the backend rejects the request because the 15-minute JWT died...
      if (error.status === 401
        && !req.url.includes('/refresh')
        && !req.url.includes('/login')
        && !req.url.includes('/register')
        && !req.url.includes('/logout')) {

        console.warn('Unauthorized request, session expired. Enforcing logout...', error);

        // log them out
        authService.logout();
      }
      else if (error.status === 401) {
        console.warn("401 on an auth route, ignoring interceptor logic.");
      }

      // Always pass the error along so the component UI can react if needed
      return throwError(() => error);
    })
  );
};
