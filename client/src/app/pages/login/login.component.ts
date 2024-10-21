import { Component, inject, OnInit } from '@angular/core';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router'; 
import { AuthService } from '../../sevices/auth.service';
import { CommonModule } from '@angular/common'; // Додайте імпорт CommonModule

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, MatInputModule, RouterLink, MatIconModule, ReactiveFormsModule], // Додайте CommonModule сюди
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'] // Зверніть увагу, тут має бути styleUrls, а не styleUrl
})
export class LoginComponent implements OnInit {
  authServise = inject(AuthService);
  hide = true;
  form!: FormGroup;
  fb = inject(FormBuilder);

  ngOnInit(): void {
    this.form = this.fb.group({
      userIdentifier: ['', [Validators.required]],  // Поле для email або username
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  login() {
    this.authServise.login(this.form.value).subscribe((response) => { 
      console.log(response);
    });
  }
}
