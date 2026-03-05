import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  // 1. Dig into the browser's vault to see if we have a token saved
  const token = localStorage.getItem('jwt_token');

  // 2. If a token exists, clone the request and attach the "Bearer" header
  if (token) {
    const clonedRequest = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    
    // Send the modified request to the .NET backend
    return next(clonedRequest); 
  }

  // 3. If there is no token (like when they are logging in), just send the normal request
  return next(req);
};
