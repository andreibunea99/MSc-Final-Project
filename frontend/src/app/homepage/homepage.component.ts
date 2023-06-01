import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-homepage',
  templateUrl: './homepage.component.html',
  styleUrls: ['./homepage.component.scss']
})
export class HomepageComponent implements OnInit {

  constructor(private router: Router) {}

  ngOnInit(): void {
    // Check if the user is already authenticated and redirect to the dashboard
    // const isAuthenticated = localStorage.getItem('email') !== null;
    // if (isAuthenticated) {
    //   this.router.navigate(['/dashboard']);
    // }
  }

  goToLogin() {
    this.router.navigate(['/login']);
  }

  goToRegister() {
    this.router.navigate(['/register']);
  }

}
