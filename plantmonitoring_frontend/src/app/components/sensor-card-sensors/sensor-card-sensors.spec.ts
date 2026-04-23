import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SensorCardSensors } from './sensor-card-sensors';

describe('SensorCardSensors', () => {
  let component: SensorCardSensors;
  let fixture: ComponentFixture<SensorCardSensors>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SensorCardSensors]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SensorCardSensors);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
