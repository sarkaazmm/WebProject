import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { LoginRequest } from '../interfaces/login-request';
import { AuthResponse } from '../interfaces/auth-response'; // Import AuthResponse
import { HttpClient } from '@angular/common/http';

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
}
