import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CustomTrainingRoutinesComponent } from './custom-training-routines.component';

describe('CustomTrainingRoutinesComponent', () => {
  let component: CustomTrainingRoutinesComponent;
  let fixture: ComponentFixture<CustomTrainingRoutinesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CustomTrainingRoutinesComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CustomTrainingRoutinesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
