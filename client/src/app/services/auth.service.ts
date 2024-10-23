import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { LoginRequest } from '../interfaces/login-request';
import { jwtDecode, JwtPayload } from 'jwt-decode';
import { AuthResponse } from '../interfaces/auth-response';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { RegisterRequest } from '../interfaces/register-request';
import { UserDetails } from '../interfaces/user-details';

interface DecodedToken extends JwtPayload {
  nameid?: string;
  name?: string;
  email?: string;
  role?: string;
  roles?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  getAccountDetails(): DecodedToken | null {
    throw new Error('Method not implemented.');
  }
  private readonly apiUrl: string;
  private readonly tokenKey = 'token';
  
  // Додаємо BehaviorSubject для відслідковування змін в авторизації
  private currentUserSubject = new BehaviorSubject<DecodedToken | null>(null);
  currentUser$ = this.currentUserSubject.asObservable();

  constructor(private readonly http: HttpClient) {
    this.apiUrl = environment.apiUrl;
    // Ініціалізуємо currentUserSubject при створенні сервісу
    this.initializeUserState();
  }

  private initializeUserState(): void {
    try {
      const token = this.getToken();
      if (token) {
        const decodedToken = jwtDecode<DecodedToken>(token);
        this.currentUserSubject.next(decodedToken);
      }
    } catch (error) {
      console.error('Error initializing user state:', error);
      this.logout();
    }
  }

  login(data: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}account/login`, data)
      .pipe(
        tap((response) => {
          if (response?.isSuccess && response?.token) {
            // Очищаємо старі дані перед встановленням нових
            this.clearAuthData();
            // Зберігаємо новий токен
            localStorage.setItem(this.tokenKey, response.token);
            // Оновлюємо стан користувача
            const decodedToken = jwtDecode<DecodedToken>(response.token);
            this.currentUserSubject.next(decodedToken);
          }
        }),
        catchError(this.handleError)
      );
  }

  register(data: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}account/register`, data)
      .pipe(
        tap((response) => {
          if (response?.isSuccess && response?.token) {
            this.clearAuthData();
            localStorage.setItem(this.tokenKey, response.token);
            const decodedToken = jwtDecode<DecodedToken>(response.token);
            this.currentUserSubject.next(decodedToken);
          }
        }),
        catchError(this.handleError)
      );
  }

  logout(): void {
    this.clearAuthData();
    this.currentUserSubject.next(null);
  }

  private clearAuthData(): void {
    try {
      localStorage.removeItem(this.tokenKey);
      sessionStorage.clear(); // Очищаємо також sessionStorage, якщо він використовується
    } catch (error) {
      console.error('Error clearing auth data:', error);
    }
  }

  getCurrentUser(): DecodedToken | null {
    return this.currentUserSubject.value;
  }

  getUserDetails(): DecodedToken | null {
    try {
      const token = this.getToken();
      if (!token) {
        return null;
      }
      
      const decodedToken = jwtDecode<DecodedToken>(token);
      // Оновлюємо currentUserSubject, якщо токен валідний
      if (decodedToken && !this.isTokenExpired(token)) {
        this.currentUserSubject.next(decodedToken);
        return decodedToken;
      }
      return null;
    } catch (error) {
      console.error('Error getting user details:', error);
      this.logout();
      return null;
    }
  }

  isLoggedIn(): boolean {
    try {
      const token = this.getToken();
      const isValid = token ? !this.isTokenExpired(token) : false;
      if (!isValid) {
        this.logout();
      }
      return isValid;
    } catch (error) {
      console.error('Error checking login status:', error);
      this.logout();
      return false;
    }
  }

  private isTokenExpired(token: string): boolean {
    try {
      const decodedToken = jwtDecode<DecodedToken>(token);
      if (!decodedToken.exp) {
        return true;
      }
      
      return Date.now() >= decodedToken.exp * 1000;
    } catch {
      return true;
    }
  }

  private getToken(): string | null {
    try {
      return localStorage.getItem(this.tokenKey);
    } catch (error) {
      console.error('Error getting token:', error);
      return null;
    }
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An error occurred';
    if (error.error instanceof ErrorEvent) {
      errorMessage = error.error.message;
    } else {
      errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`;
    }
    console.error(errorMessage);
    return throwError(() => new Error(errorMessage));
  }
}