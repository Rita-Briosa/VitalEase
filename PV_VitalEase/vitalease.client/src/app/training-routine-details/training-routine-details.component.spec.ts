import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TrainingRoutineDetailsComponent } from './training-routine-details.component';

describe('TrainingRoutineDetailsComponent', () => {
  let component: TrainingRoutineDetailsComponent;
  let fixture: ComponentFixture<TrainingRoutineDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TrainingRoutineDetailsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TrainingRoutineDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
