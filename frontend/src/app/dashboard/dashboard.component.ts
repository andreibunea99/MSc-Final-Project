import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { AuthService } from '@auth0/auth0-angular';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  @ViewChild('videoUploadDialog') videoUploadDialog!: TemplateRef<any>;
  @ViewChild('pictureUploadDialog') pictureUploadDialog!: TemplateRef<any>;

  videoFile: File | any;
  pictureFiles: File[] = [];

  constructor(public authService: AuthService, private dialog: MatDialog, private http: HttpClient) {}

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

  onVideoFileSelected(event: Event): void {
    const inputElement = event.target as HTMLInputElement;
    this.videoFile = inputElement.files?.item(0);
  }
  
  onPictureFilesSelected(event: Event): void {
    const inputElement = event.target as HTMLInputElement;
    this.pictureFiles = inputElement.files ? Array.from(inputElement.files) : [];
  }

  submitVideo(): void {
    if (!this.videoFile) {
      console.error('No video file selected');
      return;
    }

    const formData = new FormData();
    formData.append('video', this.videoFile);

    this.http.post('http://127.0.0.1:5000/upload/video', formData).subscribe(
      () => {
        console.log('Video uploaded successfully');
        // Handle success here
      },
      (error) => {
        console.error('Video upload failed:', error);
        // Handle error here
      }
    );

    this.dialog.closeAll();
  }

  submitPictures(): void {
    if (!this.pictureFiles || this.pictureFiles.length === 0) {
      console.error('No picture files selected');
      return;
    }

    const formData = new FormData();
    for (let i = 0; i < this.pictureFiles.length; i++) {
      formData.append('images', this.pictureFiles[i]);
    }

    this.http.post('http://127.0.0.1:5000/upload/images', formData).subscribe(
      () => {
        console.log('Pictures uploaded successfully');
        // Handle success here
      },
      (error) => {
        console.error('Picture upload failed:', error);
        // Handle error here
      }
    );

    this.dialog.closeAll();
  }
}
