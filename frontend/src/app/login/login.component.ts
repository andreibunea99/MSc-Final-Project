import { Component } from '@angular/core';
import { API_URL } from '../config';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})

export class LoginComponent {
  email!: string;
  password!: string;
  errorMessage = '';

  constructor(private http: HttpClient, private router: Router) { }

  submitLogin() {
    // Make API request to the login endpoint
    const loginData = {
      email: this.email,
      password: this.password
    };

    this.http.post(API_URL + 'login', loginData)
      .subscribe(
        (response: any) => {
          // Handle successful login response
          console.log('Login successful', response);
          localStorage.setItem('email', response.email);
          localStorage.setItem('firstName', response.firstName);
          localStorage.setItem('lastName', response.lastName);
          this.errorMessage = '';
          this.router.navigate(['/dashboard']);
        },
        (error) => {
          this.errorMessage = 'Invalid email or password';
        }
      );

  }

  cancelLogin() {
    // Clear the email and password fields
    this.email = '';
    this.password = '';
  }
}
