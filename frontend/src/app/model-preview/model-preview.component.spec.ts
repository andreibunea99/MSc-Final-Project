import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ModelPreviewModalComponent } from './model-preview.component';

describe('ModelPreviewModalComponent', () => {
  let component: ModelPreviewModalComponent;
  let fixture: ComponentFixture<ModelPreviewModalComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ModelPreviewModalComponent]
    });
    fixture = TestBed.createComponent(ModelPreviewModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
