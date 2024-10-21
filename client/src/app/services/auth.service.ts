import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { LoginRequest } from '../interfaces/login-request';
import { AuthResponse } from '../interfaces/auth-response'; // Import AuthResponse
import { HttpClient } from '@angular/common/http';
import { RegisterRequest } from '../interfaces/register-request';
import { UserDetails } from '../interfaces/user-details';


@Injectable({
  providedIn: 'root'
})
export class AuthService {
  apoUrl:string = environment.apiUrl;
  private tokenkey = 'token';

  constructor(private http:HttpClient) { }
  login(data: LoginRequest): Observable<AuthResponse> {
    return this.http
    .post<AuthResponse>(`${this.apoUrl}account/login`, data)
    .pipe(
      map((response) => {
        if(response.isSuccess) {
          localStorage.setItem(this.tokenkey, response.token);
        }
        return response;
      })
    );
  }
  register(data: RegisterRequest): Observable<AuthResponse> {
    return this.http
    .post<AuthResponse>(`${this.apoUrl}account/register`, data)
    .pipe(
      map((response) => {
        if(response.isSuccess) {
          localStorage.setItem(this.tokenkey, response.token);
        }
        return response;
      })
    );
  }

  getDetail = (): Observable<UserDetails> => {
    return this.http.get<UserDetails>('${this.apiUrl}/account/detail');
  };

  // getToken(): string | null {
  //   return localStorage.getItem(this.tokenkey);
  // }

  // getUserDetail = () => {
  //   // Отримання JWT токену
  //   const token = this.getToken();
  
  //   // Перевірка наявності токену
  //   if (!token) return null;
  
  //   // Декодування токену
  //   const decodedToken: any = jwtDecode(token);
  
  //   // Формування об'єкта з деталями користувача
  //   const userDetail = {
  //     id: decodedToken.nameid,
  //     fullName: decodedToken.name,
  //     email: decodedToken.email,
  //     roles: decodedToken.role || [],
  //   };
  
  //   // Повернення об'єкта з деталями користувача
  //   return userDetail;
  // };
}
