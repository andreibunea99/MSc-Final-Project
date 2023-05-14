import { Component, OnInit } from '@angular/core';
import { AuthService } from '@auth0/auth0-angular';

@Component({
  selector: 'app-toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.scss']
})
export class ToolbarComponent implements OnInit{

  constructor(public authService: AuthService) {}
  loggedUser: any;

  ngOnInit(): void {
    // console log user info
    this.authService.user$.subscribe(user => this.loggedUser = user);
    console.log(this.authService.isAuthenticated$);
  }
}
