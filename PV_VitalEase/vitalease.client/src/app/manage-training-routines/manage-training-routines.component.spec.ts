import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManageTrainingRoutinesComponent } from './manage-training-routines.component';

describe('ManageTrainingRoutinesComponent', () => {
  let component: ManageTrainingRoutinesComponent;
  let fixture: ComponentFixture<ManageTrainingRoutinesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ManageTrainingRoutinesComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ManageTrainingRoutinesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
