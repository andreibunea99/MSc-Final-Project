import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { AuthService } from '@auth0/auth0-angular';
import { HttpClient } from '@angular/common/http';
import { FormGroup, FormControl } from '@angular/forms';
import { MatFormFieldControl } from '@angular/material/form-field';

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
  previewImageFile: File | any;
  modelName: string = '';

  constructor(public authService: AuthService, private dialog: MatDialog, private http: HttpClient) {}

  loggedUser: any;

  ngOnInit(): void {
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

  onPreviewImageSelected(event: Event): void {
    const inputElement = event.target as HTMLInputElement;
    this.previewImageFile = inputElement.files?.item(0);
  }

  submitVideo(): void {
    if (!this.videoFile) {
      console.error('No video file selected');
      return;
    }

    const formData = new FormData();
    formData.append('video', this.videoFile);
    formData.append('preview', this.previewImageFile);
    formData.append('email', this.loggedUser.email);
    formData.append('name', this.modelName);

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

    formData.append('preview', this.previewImageFile);
    formData.append('email', this.loggedUser.email);
    formData.append('name', this.modelName);

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

  closeDialog(): void {	
    this.dialog.closeAll();	
  }
}
