import { Component } from '@angular/core';
import {AuthService} from '@auth0/auth0-angular';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent {
  constructor(public auth: AuthService) {}

  // display to console info about user
  ngOnInit(): void {
    this.auth.user$.subscribe(user => console.log(user));
  }
}
