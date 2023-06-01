import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.scss']
})
export class ToolbarComponent implements OnInit{

  isAuthenticated: boolean = false;
  loggedUser!: { firstName: string; lastName: string; };

  ngOnInit(): void {
    this.isAuthenticated = !!localStorage.getItem('email');
    this.loggedUser = {
      firstName: localStorage.getItem('firstName') ?? '',
      lastName: localStorage.getItem('lastName') ?? ''
    };
  }

  logout(): void {
    // Clear localStorage and perform any other logout operations
    localStorage.clear();
    // Redirect to the login page
    // Replace '/login' with your actual login route path
    window.location.href = '/login';
  }
  
}
