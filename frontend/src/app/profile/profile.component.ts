import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_URL } from '../config';

interface User {
  firstName: string;
  lastName: string;
  email: string;
}

interface Model {
  name: string;
  preview: string;
  // Add other properties if needed
}

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {
  user!: User;
  models: Model[] = [];
  isFollowed = false;
  isAuthenticated: boolean = false;
  loggedUser!: string;

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loggedUser = localStorage.getItem('email') || '';

    const storedUser = localStorage.getItem('selectedUser');
    if (storedUser) {
      this.user = JSON.parse(storedUser);
      console.log('Selected User:', this.user);
      
      // Check if the user is followed
      const isFollowedApiUrl = API_URL + `isFollowed?loggedUser=${encodeURIComponent(this.loggedUser)}&profileUser=${encodeURIComponent(this.user.email)}`;
      this.http.get(isFollowedApiUrl).subscribe(
        (response: any) => {
          this.isFollowed = response.isFollowed;
          console.log('Is Followed:', this.isFollowed);
        },
        error => {
          console.error('Error checking if user is followed:', error);
        }
      );

      const apiUrl = API_URL + `models/${encodeURIComponent(this.user.email)}`;
      this.http.get(apiUrl).subscribe(
        (response: any) => {
          this.models = response.map((model: any) => {
            const modelData: Model = {
              name: model.name,
              preview: API_URL + `files/${encodeURIComponent(this.user.email)}/${model.name}/preview.jpg`,
              // Add other properties if needed
            };
            return modelData;
          });
          console.log('Models:', this.models);
        },
        error => {
          console.error('Error fetching models:', error);
        }
      );
    }
  }

  toggleFollow() {
    // Call the backend API to toggle follow status
    const triggerFollowApiUrl = API_URL + `triggerFollow?loggedUser=${encodeURIComponent(this.loggedUser)}&profileUser=${encodeURIComponent(this.user.email)}&follow=${!this.isFollowed}`;
    this.http.post(triggerFollowApiUrl, {}).subscribe(
      (response: any) => {
        this.isFollowed = response.isFollowed;
        console.log('Is Followed:', this.isFollowed);
      },
      error => {
        console.error('Error toggling follow status:', error);
      }
    );
  }
}
