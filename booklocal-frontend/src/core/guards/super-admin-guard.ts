import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth-service';
import { map, take } from 'rxjs';

export const superAdminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.currentUser$.pipe(
    take(1),
    map(user => {
      // Assuming 'superadmin' is one of the roles.
      // Need to verify if AuthService exposes roles or if we check user properties.
      // Current User model: id, email, firstName, lastName, roles: string[].
      if (user && user.roles && user.roles.includes('superadmin')) {
        return true;
      }
      return router.createUrlTree(['/login']);
    })
  );
};
