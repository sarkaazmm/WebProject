import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';
import { PrimeChackHistoryService, PrimeChackRequest } from '../../services/prime-chack-history.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink, CommonModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  currentUser: any;
  inputNumber: number | null = null; // Змінна для вводу числа
  isChecking: boolean = false; // Статус перевірки
  taskId = 0; // ID завдання

  constructor(private authService: AuthService, private primeChackService: PrimeChackHistoryService) {
    this.authService.currentUser$.subscribe(x => this.currentUser = x);
  }

  isLoggedIn(): boolean {
    return this.authService.isLoggedIn();
  }

  onInputChange(event: Event): void {

    const input = event.target as HTMLInputElement;

    this.inputNumber = parseInt(input.value, 10);

  }

  checkPrime(): void {
    if (this.inputNumber === null) {
      alert('Please enter a valid integer.');
      return;
    }
    
    this.isChecking = true;

    const request: PrimeChackRequest = {
      userId: this.currentUser?.nameid,
      number: this.inputNumber,
    };

    this.primeChackService.createPrimeCheckRequest(request).subscribe({
      next: (response) => {
        console.log('Check initiated:', response);
        this.taskId = response.taskId;
      },
      error: (error) => {
        console.error('Error during check:', error);
        this.isChecking = false; 
      },
      complete: () => {
        this.isChecking = false;
      }
    });
  }

  cancelCheck(): void {
    if (!this.primeChackService.isChecking) {
      alert('No check is currently in progress.');
      return;
    }

    this.primeChackService.cancelRequest(this.taskId).subscribe({
      next: (response) => {
        console.log('Check cancelled:', response);
      },
      error: (error) => {
        console.error('Error during check cancellation:', error);
        this.isChecking = false; 
      },
      complete: () => {
        this.isChecking = false;
      }
    });

    alert('Check cancelled.');
  }


}

