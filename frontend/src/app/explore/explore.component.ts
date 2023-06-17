import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_URL } from '../config';
import { Router } from '@angular/router';

interface User {
  firstName: string;
  lastName: string;
  email: string;
}

@Component({
  selector: 'app-explore',
  templateUrl: './explore.component.html',
  styleUrls: ['./explore.component.scss']
})
export class ExploreComponent {
  searchInput: string = '';
  users: User[] = [];
  selectedUser: User | null = null;

  constructor(private http: HttpClient, private router: Router) { }

  searchUsers() {
    // if (this.searchInput.trim() === '') {
    //   // If the search input is empty, reset the 'users' array
    //   this.users = [];
    //   return;
    // }

    const searchParams = new URLSearchParams();
    searchParams.set('name', this.searchInput);

    const apiUrl = API_URL + `users/search?${searchParams.toString()}`;

    this.http.get<User[]>(apiUrl).subscribe(
      (response: User[]) => {
        this.users = response;
      },
      (error) => {
        console.error('Error fetching users:', error);
        // Handle the error case
      }
    );
  }

  selectUser(user: User) {
    this.selectedUser = user;
    console.log('Selected User:', this.selectedUser);
    localStorage.setItem('selectedUser', JSON.stringify(user));
    this.router.navigate(['/profile']);
  }

  ngOnInit() {
    const apiUrl = API_URL + 'users/search';

    this.http.get<User[]>(apiUrl).subscribe(
      (response: User[]) => {
        this.users = response.slice(0, 5);
      },
      (error) => {
        console.error('Error fetching initial users:', error);
        // Handle the error case
      }
    );
  }
}
