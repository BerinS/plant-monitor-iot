import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlantCardMyPlants } from './plant-card-my-plants';

describe('PlantCardMyPlants', () => {
  let component: PlantCardMyPlants;
  let fixture: ComponentFixture<PlantCardMyPlants>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlantCardMyPlants]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PlantCardMyPlants);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
