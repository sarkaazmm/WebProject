import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

export const roleGuard: CanActivateFn = (route, state) => {
  const roles = route.data['roles'] as string[];
  const authService = inject(AuthService);
  const matSnapBar = inject(MatSnackBar);
  const router = inject(Router);
  const userRoles = authService.getRoles();

  if (!authService.isLoggedIn()) {
    router.navigate(['/login']);
    matSnapBar.open('You must be logged in to view this page', 'Close', {
      duration: 5000,
    });
    return false;
  }

  if (roles.some((role) => userRoles?.includes(role))) return true;
  router.navigate(['/']);
  matSnapBar.open('You do not have permission to view this page', 'Close', {
    duration: 5000,
  });
  return false;
};
