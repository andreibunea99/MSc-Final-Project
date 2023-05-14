import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { AuthService } from '@auth0/auth0-angular';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit{
  @ViewChild('videoUploadDialog') videoUploadDialog!: TemplateRef<any>;
  @ViewChild('pictureUploadDialog') pictureUploadDialog!: TemplateRef<any>;

  constructor(public authService: AuthService, private dialog: MatDialog) {}
  loggedUser: any;

  ngOnInit(): void {
    // console log user info
    this.authService.user$.subscribe(user => this.loggedUser = user);
    console.log(this.authService.isAuthenticated$);
  }

  openVideoUploadDialog(): void {
    this.dialog.open(this.videoUploadDialog);
  }

  openPictureUploadDialog(): void {
    this.dialog.open(this.pictureUploadDialog);
  }
}
