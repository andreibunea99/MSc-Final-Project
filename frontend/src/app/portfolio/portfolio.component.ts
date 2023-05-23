import { Component, OnInit, ViewChild, TemplateRef } from '@angular/core';
import { AuthService } from '@auth0/auth0-angular';
import { HttpClient } from '@angular/common/http';
import { MatDialog } from '@angular/material/dialog';


@Component({
  selector: 'app-portfolio',
  templateUrl: './portfolio.component.html',
  styleUrls: ['./portfolio.component.scss']
})
export class PortfolioComponent implements OnInit {

  loggedUser: any;
  models: any[] = [];
  editedModel: any;
  newModelName: string = '';
  previewImage: File | null = null;
  @ViewChild('editModelDialog') editModelDialog!: TemplateRef<any>;

  constructor(public authService: AuthService, private http: HttpClient, private dialog: MatDialog) {}

  ngOnInit(): void {
    this.authService.user$.subscribe(user => {
      this.loggedUser = user;
      if (this.loggedUser && this.loggedUser.email) {
        console.log('Email:', this.loggedUser.email);

        const apiUrl = `http://localhost:5000/models/${encodeURIComponent(this.loggedUser.email)}`;
        this.http.get(apiUrl).subscribe(
          (response: any) => {
            this.models = response.map((model: any) => {
              model['preview'] = `http://localhost:5000/files/${encodeURIComponent(this.loggedUser.email)}/${model.name}/preview.jpg`;
              model['obj'] = `http://localhost:5000/files/${encodeURIComponent(this.loggedUser.email)}/${model.name}/texturedMesh.obj`;
              model['mtl'] = `http://localhost:5000/files/${encodeURIComponent(this.loggedUser.email)}/${model.name}/texturedMesh.mtl`;
              model['textures'] = model['textures'].map((texture: any) => {
                texture['path'] = `http://localhost:5000/files/${encodeURIComponent(this.loggedUser.email)}/${model.name}/${texture.filename}`;
                return texture;
              });
              return model;
            });
            console.log('Models:', this.models);
          },
          error => {
            console.error('Error fetching models:', error);
          }
        );
      }
    });
  }

  downloadModel(model: any): void {
    const apiUrl = `http://localhost:5000/models/${encodeURIComponent(this.loggedUser.email)}/${encodeURIComponent(model.name)}/download`;
    this.http.get(apiUrl, { responseType: 'blob' }).subscribe(
      (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `${model.name}.zip`;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error => {
        console.error('Error downloading model:', error);
      }
    );
  }

  openEditModelDialog(model: any): void {
    this.editedModel = model;
    this.dialog.open(this.editModelDialog);
  }

  submitEditModel(model: any): void {
    if (!this.newModelName) {
      return;
    }

    const apiUrl = `http://localhost:5000/edit/model`;
    const formData = new FormData();
    formData.append('email', this.loggedUser.email);
    formData.append('model_name', this.editedModel.name);
    formData.append('new_name', this.newModelName);
    if (this.previewImage) {
      formData.append('preview', this.previewImage);
    }

    this.http.post(apiUrl, formData, { responseType: 'text' }).subscribe(
      (response: any) => {
        console.log(response); // Log the response for debugging purposes
        if (response === 'Model renamed successfully') {
          // Model renamed successfully, perform any necessary actions (e.g., update the models list)
          // Close the rename model dialog
          this.dialog.closeAll();
          this.ngOnInit();
        } else {
          console.error('Error renaming model:', response);
        }
      },
      (error: any) => {
        console.error('Error renaming model:', error);
      }
    );
    
  }

  onPreviewImageSelected(event: any): void {
    this.previewImage = event.target.files[0];
  }

  closeDialog(): void {
    this.dialog.closeAll();
  }

  renameModel(model: any): void {
    // Implement the logic to rename the model
  }

  deleteModel(model: any): void {
    // Implement the logic to delete the model
  } 

}
