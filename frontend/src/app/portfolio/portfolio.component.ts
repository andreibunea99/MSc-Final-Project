import { Component, OnInit } from '@angular/core';
import { AuthService } from '@auth0/auth0-angular';
import { HttpClient } from '@angular/common/http';
import * as THREE from 'three';
import {OBJLoader} from 'three/examples/jsm/loaders/OBJLoader.js';
import {OBJExporter} from 'three/examples/jsm/exporters/OBJExporter.js';

interface Model {
  id: number;
  obj_data: string;
}


@Component({
  selector: 'app-portfolio',
  templateUrl: './portfolio.component.html',
  styleUrls: ['./portfolio.component.scss']
})
export class PortfolioComponent implements OnInit {

  loggedUser: any;
  models: Model[] = [];
  objLoader: OBJLoader = new OBJLoader();

  scene: THREE.Scene = new THREE.Scene();	
  camera: THREE.PerspectiveCamera | any;	
  renderer: THREE.WebGLRenderer | any;	
  modelPreviews: HTMLElement[] = [];

  constructor(public authService: AuthService, private http: HttpClient) {}

  ngOnInit(): void {
    // console log user info
    // console log user info
    this.authService.user$.subscribe(user => {
      this.loggedUser = user;
      if (this.loggedUser && this.loggedUser.email) {
        console.log('Email:', this.loggedUser.email);
        
        // Make an HTTP GET request to fetch the models for the user's email
        const apiUrl = `http://127.0.0.1:5000/models/${this.loggedUser.email}`;
        this.http.get<Model[]>(apiUrl).subscribe(models => {
          this.models = models;
          console.log(this.models);

          this.processModels();
        });
      }
    });
    console.log(this.authService.isAuthenticated$);
    this.setupRenderer();
  }

  setupRenderer(): void {	
    this.camera = new THREE.PerspectiveCamera(	
      75,	
      window.innerWidth / window.innerHeight,	
      0.1,	
      1000	
    );	
    this.camera.position.z = 5;	
    this.renderer = new THREE.WebGLRenderer({ antialias: true });	
    this.renderer.setSize(window.innerWidth, window.innerHeight);	
    document.body.appendChild(this.renderer.domElement);
  }

  processModels(): void {
    this.models.forEach(model => {
      const objData = this.base64ToArrayBuffer(model.obj_data);
      const objUrl = this.createObjectURL(objData);

      // Use the objUrl in your 3D rendering library or viewer to display the model
      // For example, if you are using Three.js:
      this.objLoader.load(objUrl, object => {
        // Do something with the loaded 3D object
        console.log(object);
        const previewElement = document.getElementById(	
          `model-preview-${model.id}`	
        );	
        if (previewElement) {	
          this.modelPreviews[model.id] = previewElement;	
          object.scale.set(0.001, 0.001, 0.001);
          this.scene.add(object);	
          const exporter = new OBJExporter();
          const objData = exporter.parse(object);
          this.saveObjFile(objData, `model-${model.id}.obj`);
        }
      });
    });
  }

  saveObjFile(data: string, fileName: string): void {
    const blob = new Blob([data], { type: 'text/plain' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = fileName;
    link.click();
  }

  base64ToArrayBuffer(base64: string): ArrayBuffer {
    const binaryString = window.atob(base64);
    const length = binaryString.length;
    const bytes = new Uint8Array(length);
    for (let i = 0; i < length; ++i) {
      bytes[i] = binaryString.charCodeAt(i);
    }
    return bytes.buffer;
  }

  createObjectURL(data: ArrayBuffer): string {
    const blob = new Blob([data], { type: 'application/octet-stream' });
    return URL.createObjectURL(blob);
  }

  renderScene(): void {	
    requestAnimationFrame(() => this.renderScene());	
    if (this.modelPreviews.length > 0) {	
      this.renderer.render(this.scene, this.camera);	
    }	
  }

}
