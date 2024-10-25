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
  inputNumber: number | null = null;
  isChecking: boolean = false;
  progress: number = 0;
  private progressIntervals: Map<number, any> = new Map(); // Map to store intervals for each task
  result: string | null = null;
  taskId = 0;
  userRequestHistory: PrimeChackHistory[] = [];
  activeTasks: PrimeChackHistory[] = []; // Array to store multiple active tasks

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
    const currentUserId = this.authService.getCurrentUser()?.nameid;
  
    if (currentUserId) {
      this.primeChackService.getRequestsByUserId(currentUserId).subscribe(userRequests => {
        // Separate active and completed tasks
        this.activeTasks = userRequests.filter(req => req.progress >= 0 && req.progress < 100);
        this.userRequestHistory = userRequests
          .filter(req => req.progress === 100 || req.progress === -1)
          .sort((a, b) => new Date(b.requestDateTime).getTime() - new Date(a.requestDateTime).getTime());
        
        // Start tracking progress for all active tasks
        this.activeTasks.forEach(task => {
          if (!this.progressIntervals.has(task.id)) {
            this.trackProgress(task.id);
          }
        });
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

    const newTask: PrimeChackHistory = {
      id: 0,
      userId: this.currentUser?.nameid,
      number: this.inputNumber,
      isPrime: false,
      progress: 0,
      requestDateTime: new Date(),
    };

    const request: PrimeChackRequest = {
      userId: this.currentUser?.nameid,
      number: this.inputNumber,
    };

    this.primeChackService.createPrimeCheckRequest(request).subscribe({
      next: (response) => {
        console.log('Check initiated:', response);
        newTask.id = response.taskId;
        this.activeTasks.push(newTask);
        this.trackProgress(response.taskId);
      },
      error: (error) => {
        console.error('Error during check:', error);
        this.isChecking = false;
      }
    });
  }

  trackProgress(taskId: number): void {
    if (this.progressIntervals.has(taskId)) {
      clearInterval(this.progressIntervals.get(taskId));
    }

    const interval = setInterval(() => {
      this.primeChackService.getRequest(taskId).subscribe({
        next: (history) => {
          const taskIndex = this.activeTasks.findIndex(t => t.id === taskId);
          if (taskIndex !== -1) {
            this.activeTasks[taskIndex].progress = history.progress;
            
            if (history.progress >= 100 || history.progress === -1) {
              this.activeTasks[taskIndex].isPrime = history.isPrime;
              clearInterval(this.progressIntervals.get(taskId));
              this.progressIntervals.delete(taskId);
              // Move task to history
              this.loadRequestHistory();
            }
          }
        },
        error: (error) => {
          console.error('Error getting progress:', error);
          clearInterval(this.progressIntervals.get(taskId));
          this.progressIntervals.delete(taskId);
        }
      });
    }, 1000);

    this.progressIntervals.set(taskId, interval);
  }

  cancelCheck(taskId: number): void {
    const task = this.activeTasks.find(t => t.id === taskId);
    if (!task) {
      alert('Task not found.');
      return;
    }

    this.primeChackService.cancelRequest(taskId).subscribe({
      next: (response) => {
        console.log('Check cancelled:', response);
        if (this.progressIntervals.has(taskId)) {
          clearInterval(this.progressIntervals.get(taskId));
          this.progressIntervals.delete(taskId);
        }
        this.loadRequestHistory();
      },
      error: (error) => {
        console.error('Error during check cancellation:', error);
      }
    });
  }

  ngOnDestroy(): void {
    // Clean up all intervals when component is destroyed
    this.progressIntervals.forEach(interval => clearInterval(interval));
    this.progressIntervals.clear();
  }
}