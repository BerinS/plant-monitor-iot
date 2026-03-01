import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { LucideAngularModule, Sprout } from 'lucide-angular';

import { PlantCardMyPlants } from '../../components/plantCard-my-plants/plant-card-my-plants.component'
import { PlantService } from '../../services/plant.service';

@Component({
  selector: 'app-my-plants',
  imports: [PlantCardMyPlants, LucideAngularModule],
  templateUrl: './my-plants.component.html',
  styleUrl: './my-plants.component.scss',
})
export class MyPlantsComponent {
  readonly Sprout = Sprout;
  private plantService = inject(PlantService);

  plants = toSignal(this.plantService.getPlants(), { initialValue: [] });

}