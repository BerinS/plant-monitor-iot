import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Sensors } from './sensors';

describe('Sensors', () => {
  let component: Sensors;
  let fixture: ComponentFixture<Sensors>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Sensors]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Sensors);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
