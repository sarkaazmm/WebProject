import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';

export interface PrimeChackRequest {
  userId: string;
  number: number;
}

export interface PrimeChackHistory {
  id: number;
  userId: string;
  number: number;
  isPrime: boolean;
  progress: number;
  requestDateTime: Date;
}

@Injectable({
  providedIn: 'root'
})
export class PrimeChackHistoryService {
  isChecking() {
    throw new Error('Method not implemented.');
  }
  private readonly apiUrl = `${environment.apiUrl}PrimeChackHistory`;

  constructor(private http: HttpClient) {}

  createPrimeCheckRequest(request: PrimeChackRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/create`, request);
  }

  cancelRequest(id: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/cancel-request/${id}`, {});
  }
  
  getAllRequests(): Observable<PrimeChackHistory[]> {
    return this.http.get<PrimeChackHistory[]>(`${this.apiUrl}/all-requests`);
  }

  getRequest(id: number): Observable<PrimeChackHistory> {
    return this.http.get<PrimeChackHistory>(`${this.apiUrl}/get-prime-chack-request${id}`);
  }

  getRequestsByUserId(userId: string): Observable<PrimeChackHistory[]> {
    return this.http.get<PrimeChackHistory[]>(`${this.apiUrl}/requests-by-user-id${userId}`);
  }

  getRunningTasksCount(): Observable<{ count: number }> {
    return this.http.get<{ count: number }>(`${this.apiUrl}/running-tasks-count`);
  }
}