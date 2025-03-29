import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditCustomTrainingRoutineComponent } from './edit-custom-training-routine.component';

describe('EditCustomTrainingRoutineComponent', () => {
  let component: EditCustomTrainingRoutineComponent;
  let fixture: ComponentFixture<EditCustomTrainingRoutineComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [EditCustomTrainingRoutineComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditCustomTrainingRoutineComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
