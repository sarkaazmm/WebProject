import { Component, inject } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './users.component.html',
  styleUrl: './users.component.css'
})
export class UsersComponent {
  authService = inject(AuthService);
  
  user$ = this.authService.getAll();

  constructor() {
    this.user$.subscribe({
      next: data => console.log('Users loaded:', data),
      error: err => console.error('Error loading users:', err)
    });
  }

  trackById(index: number, item: any): number {
    return item.id;
  }
}
