import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service'; 
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, MatInputModule, RouterLink, MatIconModule, MatSnackBarModule, ReactiveFormsModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit {
  hide = true;
  authService = inject(AuthService);
  matSnackBar = inject(MatSnackBar);
  fb = inject(FormBuilder);
  registerForm!: FormGroup;
  router = inject(Router);
  errors: string[] = [];

  ngOnInit(): void {
    // Ініціалізація форми з валідаторами
    this.registerForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      username: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  register() {
    // Перевірка валідності форми перед відправкою запиту
    if (this.registerForm.valid) {
      console.log('Form submitted', this.registerForm.value); // Додано лог для перевірки відправлених даних

      // Відправка запиту на реєстрацію через сервіс
      this.authService.register(this.registerForm.value).subscribe({
        next: (response) => {
          console.log(response);
          this.matSnackBar.open(response.message, 'Close', {
            duration: 5000,
            horizontalPosition: 'center'
          });
          this.router.navigate(['/login']); // Перенаправлення після успішної реєстрації
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 400) {
            // Виведення помилок в масиві errors
            this.errors = Array.isArray(error.error) ? error.error : [error.error.message];
            this.matSnackBar.open('Validation error', 'Close', {
              duration: 5000,
              horizontalPosition: 'center'
            });
          }
        },
        complete: () => console.log('Register completed')
      });
    } else {
      // Якщо форма не валідна, виводимо повідомлення про помилку
      this.errors = ['Please fill in all required fields correctly.'];
      this.matSnackBar.open('Form is invalid', 'Close', {
        duration: 5000,
        horizontalPosition: 'center'
      });
    }
  }
}
