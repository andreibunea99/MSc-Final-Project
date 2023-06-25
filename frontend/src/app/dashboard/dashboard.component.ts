import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { HttpClient } from '@angular/common/http';
import { FormGroup, FormControl } from '@angular/forms';
import { MatFormFieldControl } from '@angular/material/form-field';
import { API_URL } from '../config';

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

  constructor(private dialog: MatDialog, private http: HttpClient) {}

  ngOnInit(): void {
    
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
    formData.append('email', localStorage.getItem('email') ?? '');
    formData.append('name', this.modelName);
    console.log(this.videoFile);
    console.log(this.previewImageFile);

    this.http.post(API_URL + 'upload/video', formData).subscribe(
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
    formData.append('email', localStorage.getItem('email') ?? '');
    formData.append('name', this.modelName);

    this.http.post(API_URL + 'upload/images', formData).subscribe(
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
