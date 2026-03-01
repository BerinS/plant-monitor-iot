import { Component, computed, input } from '@angular/core'; 
import { CommonModule } from '@angular/common'; 
import { LucideAngularModule, Droplets, Pen, Trash2 } from 'lucide-angular';
import { Plant } from '../../models/my-plants.model'; 

@Component({
  selector: 'app-plant-card-my-plants',
  imports: [LucideAngularModule, CommonModule], 
  templateUrl: './plant-card-my-plants.component.html', 
  styleUrl: './plant-card-my-plants.component.scss',
})
export class PlantCardMyPlants {
  readonly DropIcon = Droplets;
  readonly PenIcon = Pen;
  readonly TrashIcon = Trash2;

  plant = input.required<Plant>(); 
  isLow = computed(() => this.plant().currentMoisture < 30);
}