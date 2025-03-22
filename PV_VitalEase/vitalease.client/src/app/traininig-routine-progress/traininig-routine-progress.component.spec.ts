import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TraininigRoutineProgressComponent } from './traininig-routine-progress.component';

describe('TraininigRoutineProgressComponent', () => {
  let component: TraininigRoutineProgressComponent;
  let fixture: ComponentFixture<TraininigRoutineProgressComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TraininigRoutineProgressComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TraininigRoutineProgressComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
