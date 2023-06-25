import { Component, ElementRef, ViewChild } from '@angular/core';
import * as THREE from 'three';
import { OBJLoader } from 'three/examples/jsm/loaders/OBJLoader';
import { MTLLoader } from 'three/examples/jsm/loaders/MTLLoader';
import { TextureLoader } from 'three/src/loaders/TextureLoader';

@Component({
  selector: 'app-model-preview',
  templateUrl: './model-preview.component.html',
  styleUrls: ['./model-preview.component.scss']
})
export class ModelPreviewComponent {
  @ViewChild('modelContainer', { static: true }) modelContainer!: ElementRef;

  ngAfterViewInit(): void {
    const container = this.modelContainer.nativeElement;
    const scene = new THREE.Scene();
    const camera = new THREE.PerspectiveCamera(
      75,
      window.innerWidth / window.innerHeight,
      0.1,
      1000
    );
    const renderer = new THREE.WebGLRenderer();
    renderer.setSize(window.innerWidth, window.innerHeight);
    container.appendChild(renderer.domElement);

    let model: THREE.Mesh | undefined; // Variable to hold the loaded model
    let loadedTextures: number = 0; // Counter for loaded textures

    const textureLoader = new TextureLoader();

    const checkTexturesLoaded = (totalTextures: number) => {
      loadedTextures++;
      if (loadedTextures === totalTextures) {
        // All textures loaded, proceed with rendering
        animate();
      }
    };

    const animate = function () {
      requestAnimationFrame(animate);

      // Rotate the model on multiple axes
      if (model) {
        model.rotation.x += 0.005; // Rotate around the x-axis
        model.rotation.y += 0.0025; // Rotate around the y-axis
        model.rotation.z += 0.00125; // Rotate around the z-axis
      }

      renderer.render(scene, camera);
    };

    camera.position.z = 5;

    const modelData = localStorage.getItem('model');
    if (modelData) {
      const modelInfo = JSON.parse(modelData);
      const objLoader = new OBJLoader();
      const mtlLoader = new MTLLoader();

      mtlLoader.load(modelInfo.mtl, (materials) => {
        materials.preload();
        objLoader.setMaterials(materials);
        objLoader.load(modelInfo.obj, (object) => {
          object.traverse((node) => {
            if (node instanceof THREE.Mesh) {
              const mesh = node as THREE.Mesh;

              const customMaterial = new THREE.MeshPhongMaterial();

              const totalTextures = modelInfo.textures.length;

              // Load and apply textures to the material
              modelInfo.textures.forEach((textureInfo: any) => {
                const texture = textureLoader.load(
                  textureInfo.path,
                  () => checkTexturesLoaded(totalTextures) // Texture onLoad callback
                );
                const propertyName = textureInfo.propertyName;

                // Set the texture map based on the property name
                switch (propertyName) {
                  case 'map':
                    customMaterial.map = texture;
                    break;
                  case 'normalMap':
                    customMaterial.normalMap = texture;
                    break;
                  // Add more cases for other texture maps (e.g., roughnessMap, metalnessMap, etc.)
                }
              });

              mesh.material = customMaterial;
              model = mesh; // Assign the loaded model to the variable
            }
          });

          scene.add(object);
        });
      });

      // Add directional light to the scene
      const directionalLight = new THREE.DirectionalLight(0xffffff, 1);
      directionalLight.position.set(1, 1, 1);
      scene.add(directionalLight);

      animate();
    }
  }
}
