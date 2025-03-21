import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TrainingRoutineExerciseDetailsComponent } from './training-routine-exercise-details.component';

describe('TrainingRoutineExerciseDetailsComponent', () => {
  let component: TrainingRoutineExerciseDetailsComponent;
  let fixture: ComponentFixture<TrainingRoutineExerciseDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TrainingRoutineExerciseDetailsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TrainingRoutineExerciseDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
