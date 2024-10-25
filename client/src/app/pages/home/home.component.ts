import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';
import { PrimeChackHistory, PrimeChackHistoryService, PrimeChackRequest } from '../../services/prime-chack-history.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink, CommonModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {
  currentUser: any;
  inputNumber: number | null = null; // Variable for input number
  isChecking: boolean = false; 
  progress: number = 0; // Progress of the check
  private progressInterval: any; // Interval for tracking progress
  result: string | null = null; // Result of the check
  taskId = 0; // Task ID
  history: PrimeChackHistory[] = [];
  userRequestHistory: PrimeChackHistory[] = []; 

  constructor(private authService: AuthService, private primeChackService: PrimeChackHistoryService) {
    this.authService.currentUser$.subscribe(x => this.currentUser = x);
  }

  ngOnInit(): void {
    this.loadRequestHistory();
  }

  isLoggedIn(): boolean {
    return this.authService.isLoggedIn();
  }

  onInputChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.inputNumber = parseInt(input.value, 10);
  }

  loadRequestHistory(): void {
    const currentUserId = this.authService.getCurrentUser()?.nameid; // Отримуємо ідентифікатор поточного користувача

    if (currentUserId) {
      this.primeChackService.getRequestsByUserId(currentUserId).subscribe(userRequests => {
        this.userRequestHistory = userRequests;
      });
    } else {
      console.error('Current user ID is undefined.');
    }
  }

  checkPrime(): void {
    if (this.inputNumber === null) {
      alert('Please enter a valid integer.');
      return;
    }

    this.isChecking = true;
    this.progress = 0;
    this.result = null;

    const request: PrimeChackRequest = {
      userId: this.currentUser?.nameid,
      number: this.inputNumber,
    };

    this.primeChackService.createPrimeCheckRequest(request).subscribe({
      next: (response) => {
        console.log('Check initiated:', response);
        this.taskId = response.taskId;
        this.trackProgress();
      },
      error: (error) => {
        console.error('Error during check:', error);
        this.isChecking = false;
      }
    });
  }

  trackProgress(): void {
    if (this.progressInterval) {
      clearInterval(this.progressInterval);
    }

    this.progressInterval = setInterval(() => {
      if (!this.isChecking) {
        clearInterval(this.progressInterval);
        return;
      }

      this.primeChackService.getRequest(this.taskId).subscribe({
        next: (history) => {
          this.progress = history.progress;
          
          // Оновлюємо результат і історію коли процес завершено
          if (this.progress >= 100) {
            this.result = history.isPrime ? 'The number is prime.' : 'The number is not prime.';
            clearInterval(this.progressInterval);
            this.isChecking = false;
            
            // Оновлюємо історію після завершення перевірки
            this.loadRequestHistory();
          }
        },
        error: (error) => {
          console.error('Error getting progress:', error);
          clearInterval(this.progressInterval);
          this.isChecking = false;
        }
      });
    }, 1000);
  }

  cancelCheck(): void {
    if (!this.isChecking) { // Змінено умову перевірки
      alert('No check is currently in progress.');
      return;
    }

    if (this.progressInterval) {
      clearInterval(this.progressInterval);
    }

    this.primeChackService.cancelRequest(this.taskId).subscribe({
      next: (response) => {
        console.log('Check cancelled:', response);
        this.isChecking = false;
        this.progress = 0;
        // Оновлюємо історію після скасування
        this.loadRequestHistory();
      },
      error: (error) => {
        console.error('Error during check cancellation:', error);
        this.isChecking = false;
      }
    });
  }

}


