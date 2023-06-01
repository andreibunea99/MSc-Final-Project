import { Component } from '@angular/core';
import { API_URL } from '../config';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})

export class RegisterComponent {
  firstName!: string;
  lastName!: string;
  email!: string;
  password!: string;
  errorMessage = '';

  constructor(private http: HttpClient, private router: Router) { }

  submitRegister() {
    // Make API request to the register endpoint
    const registerData = {
      first_name: this.firstName,
      last_name: this.lastName,
      email: this.email,
      password: this.password
    };

    this.http.post(API_URL + 'register', registerData)
      .subscribe(
        (response: any) => {
          // Handle successful registration response
          console.log('Registration successful', response);
          this.errorMessage = '';
          this.router.navigate(['/login']);
        },
        (error) => {
          this.errorMessage = 'Error registering user';
        }
      );

  }

  cancelRegister() {
    // Clear the form fields
    this.firstName = '';
    this.lastName = '';
    this.email = '';
    this.password = '';
  }
}
