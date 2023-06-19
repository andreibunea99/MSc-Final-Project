import { Component, OnInit, ViewChild, TemplateRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MatDialog } from '@angular/material/dialog';
import { API_URL } from '../config';

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

  constructor(private http: HttpClient, private dialog: MatDialog) {}

  ngOnInit(): void {
    const email = localStorage.getItem('email');
    if (email) {
      const apiUrl = API_URL + `models/${encodeURIComponent(email)}`;
      this.http.get(apiUrl).subscribe(
        (response: any) => {
          let modelsWithObj: any[] = [];
          let modelsWithoutObj: any[] = [];
          
          response.forEach((model: any) => {
            model['preview'] = API_URL + `files/${encodeURIComponent(email)}/${model.name}/preview.jpg`;
            
            if (model['obj'] !== 'None') {
              model['obj'] = API_URL + `files/${encodeURIComponent(email)}/${model.name}/texturedMesh.obj`;
              model['mtl'] = API_URL + `files/${encodeURIComponent(email)}/${model.name}/texturedMesh.mtl`;
              model['textures'] = model['textures'].map((texture: any) => {
                texture['path'] = API_URL + `files/${encodeURIComponent(email)}/${model.name}/${texture.filename}`;
                return texture;
              });
              
              modelsWithObj.push(model);
            } else {
              modelsWithoutObj.push(model);
            }
          });
          
          this.models = modelsWithObj.concat(modelsWithoutObj);
          console.log('Models:', this.models);
        },
        error => {
          console.error('Error fetching models:', error);
        }
      );
    }
  }
  

  downloadModel(model: any): void {
    const email = localStorage.getItem('email');
    if (email) {
      const apiUrl = API_URL + `models/${encodeURIComponent(email)}/${encodeURIComponent(model.name)}/download`;
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
  }

  openEditModelDialog(model: any): void {
    this.editedModel = model;
    this.dialog.open(this.editModelDialog);
  }

  submitEditModel(model: any): void {
    if (!this.newModelName) {
      return;
    }

    const email = localStorage.getItem('email');
    if (email) {
      const apiUrl = API_URL + `edit/model`;
      const formData = new FormData();
      formData.append('email', email);
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
    const email = localStorage.getItem('email');
    if (email) {
      const apiUrl = API_URL + `delete/${encodeURIComponent(email)}/${encodeURIComponent(model.name)}`;
      this.http.delete(apiUrl).subscribe(
        () => {
          // Model deleted successfully, perform any necessary actions (e.g., update the models list)
          this.ngOnInit();
          location.reload();
        },
        (error: any) => {
          // console.error('Error deleting model:', error);
          location.reload();
        }
      );
    }
  }
  

}
