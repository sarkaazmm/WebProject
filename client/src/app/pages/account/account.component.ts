import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-account',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatCardModule],
  templateUrl: './account.component.html',
  styleUrls: ['./account.component.css']
})
export class AccountComponent implements OnInit {
  accountDetails$: Observable<any>;

  constructor(private authService: AuthService, private router: Router) {
    this.accountDetails$ = this.authService.currentUser$;
  }

  ngOnInit(): void {
    // Можемо додати додаткову логіку ініціалізації якщо потрібно
  }
  logout(): void {
    this.authService.logout(); // Clear authentication tokens or session
    this.router.navigate(['/login']); // Redirect to login or register page
  }
}
