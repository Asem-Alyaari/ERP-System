import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  username = '';
  password = '';
  error = '';
  isLoading = false;
  showPassword = false;
  currentYear = new Date().getFullYear();

  constructor(private authService: AuthService, private router: Router) {}

  onLogin() {
    this.error = '';
    this.isLoading = true;

    this.authService.login({ username: this.username, password: this.password }).subscribe({
      next: (res) => {
        localStorage.setItem('token', res.token);
        this.router.navigate(['/']);
      },
      error: () => {
        this.error = 'اسم المستخدم أو كلمة المرور غير صحيحة';
        this.isLoading = false;
      }
    });
  }
}
