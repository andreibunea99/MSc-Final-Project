import { Component, OnInit } from '@angular/core';
import { AuthService } from '@auth0/auth0-angular';
import { HttpClient } from '@angular/common/http';


@Component({
  selector: 'app-portfolio',
  templateUrl: './portfolio.component.html',
  styleUrls: ['./portfolio.component.scss']
})
export class PortfolioComponent implements OnInit {

  loggedUser: any;
  models: any[] = [];

  constructor(public authService: AuthService, private http: HttpClient) {}

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

  renameModel(model: any): void {
    // Implement the logic to rename the model
  }

  deleteModel(model: any): void {
    // Implement the logic to delete the model
  } 

}
