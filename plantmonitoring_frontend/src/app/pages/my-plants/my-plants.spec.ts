import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyPlants } from './my-plants';

describe('MyPlants', () => {
  let component: MyPlants;
  let fixture: ComponentFixture<MyPlants>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyPlants]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MyPlants);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
