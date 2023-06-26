import { Component, ElementRef, ViewChild } from '@angular/core';
import * as THREE from 'three';
import { OBJLoader } from 'three/examples/jsm/loaders/OBJLoader';
import { MTLLoader } from 'three/examples/jsm/loaders/MTLLoader';
import { TextureLoader } from 'three/src/loaders/TextureLoader';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls';

@Component({
  selector: 'app-model-preview',
  templateUrl: './model-preview.component.html',
  styleUrls: ['./model-preview.component.scss']
})
export class ModelPreviewComponent {
  @ViewChild('modelContainer', { static: true }) modelContainer!: ElementRef;

  private container!: HTMLElement;
  private scene!: THREE.Scene;
  private camera!: THREE.PerspectiveCamera;
  private renderer!: THREE.WebGLRenderer;
  private controls!: OrbitControls;
  private model!: THREE.Group;

  ngAfterViewInit(): void {
    this.container = this.modelContainer.nativeElement;

    this.initScene();
    this.loadModel();
    this.addLights();
    this.addControls();
    this.animate();
  }

  private initScene(): void {
    this.scene = new THREE.Scene();
    this.scene.background = new THREE.Color(0xa28aeb);
    this.camera = new THREE.PerspectiveCamera(
      75,
      window.innerWidth / window.innerHeight,
      0.1,
      1000
    );

    this.renderer = new THREE.WebGLRenderer();
    this.renderer.setSize(window.innerWidth, window.innerHeight);
    this.container.appendChild(this.renderer.domElement);

    this.camera.position.z = 5;

    window.addEventListener('resize', this.handleResize.bind(this));
  }

  private loadModel(): void {
    const objLoader = new OBJLoader();
    const mtlLoader = new MTLLoader();
    const textureLoader = new TextureLoader();
    const modelData = localStorage.getItem('model');
    if (modelData) {
      const modelInfo = JSON.parse(modelData);

      mtlLoader.load(modelInfo.mtl, (materials) => {
        materials.preload();
        objLoader.setMaterials(materials);

        objLoader.load(modelInfo.obj, (object) => {
          this.model = object as THREE.Group;
          this.scene.add(this.model);

          // Traverse through the loaded model to assign textures
          this.model.traverse((node) => {
            if (node instanceof THREE.Mesh) {
              const mesh = node;
              const material = mesh.material as THREE.MeshPhongMaterial;

              if (material.map) {
                const texturePath = material.map.image.src;
                const texture = textureLoader.load(texturePath, () => {
                  material.needsUpdate = true;
                  this.renderer.render(this.scene, this.camera);
                });
                material.map = texture;
              }
            }
          });
        });
      });
    }
  }

  private addLights(): void {
    const directionalLight = new THREE.DirectionalLight(0xffffff, 1);
    directionalLight.position.set(1, 1, 1);
    this.scene.add(directionalLight);
  }

  private addControls(): void {
    this.controls = new OrbitControls(this.camera, this.renderer.domElement);
    this.controls.enableDamping = true;
    this.controls.dampingFactor = 0.05;
    this.controls.rotateSpeed = 0.5;
    this.controls.zoomSpeed = 1.2;
    this.controls.addEventListener('change', this.render.bind(this));
  }

  private handleResize(): void {
    const width = window.innerWidth;
    const height = window.innerHeight;

    this.renderer.setSize(width, height);
    this.camera.aspect = width / height;
    this.camera.updateProjectionMatrix();

    this.render();
  }

  private animate(): void {
    requestAnimationFrame(this.animate.bind(this));
    this.controls.update();
    this.render();
  }

  private render(): void {
    this.renderer.render(this.scene, this.camera);
  }
}
