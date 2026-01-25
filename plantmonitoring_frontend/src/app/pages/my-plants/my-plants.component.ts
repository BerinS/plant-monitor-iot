import { Component, effect } from '@angular/core';
import { inject } from '@angular/core';

import { PlantCardMyPlants } from '../../components/plantCard-my-plants/plant-card-my-plants.component'
import { PlantService } from '../../services/plant.service';
import { toSignal } from '@angular/core/rxjs-interop';



@Component({
  selector: 'app-my-plants',
  imports: [PlantCardMyPlants],
  templateUrl: './my-plants.component.html',
  styleUrl: './my-plants.component.scss',
})
export class MyPlantsComponent {
  private plantService = inject(PlantService);

  // plant signal
  plants = toSignal(this.plantService.getPlants(), { initialValue: [] });

  constructor() {
    effect(() => {
      const plantsList = this.plants();
      
    }, { allowSignalWrites: true });
  }
}
