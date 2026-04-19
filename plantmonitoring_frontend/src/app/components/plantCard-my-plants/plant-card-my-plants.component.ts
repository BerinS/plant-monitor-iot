import { Component, computed, input, output } from '@angular/core'; 
import { CommonModule } from '@angular/common'; 
import { LucideAngularModule, Droplets, Pen, Trash2 } from 'lucide-angular';
import { Plant } from '../../models/plant.model'; 
import { NgxGaugeModule } from 'ngx-gauge';

@Component({
  selector: 'app-plant-card-my-plants',
  imports: [LucideAngularModule, CommonModule, NgxGaugeModule], 
  templateUrl: './plant-card-my-plants.component.html', 
  styleUrl: './plant-card-my-plants.component.scss',
})
export class PlantCardMyPlants {
  readonly DropIcon = Droplets;
  readonly PenIcon = Pen;
  readonly TrashIcon = Trash2;

  plant = input.required<Plant>(); 
  isLow = computed(() => this.plant().currentMoisture < 30);
  editRequest = output<Plant>();
  deleteRequest = output<Plant>();

  onEdit() {
    this.editRequest.emit(this.plant());
    console.log('Edit requested for plant:', this.plant());
  }

  onDelete() {
    this.deleteRequest.emit(this.plant());
    console.log('Delete requested for plant:', this.plant());
  }
}