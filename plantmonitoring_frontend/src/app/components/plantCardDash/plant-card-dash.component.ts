import { Component, input, computed } from '@angular/core';
import { Plant } from '../../models/dashboard.model';
import { LucideAngularModule, Droplets, Info } from 'lucide-angular';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-plant-card-dash',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  template: `
    <div class="plant-card">
      
    <div class="wrapper">
      <div class="stats">
        <div class="moisture-badge" [class.low]="isLow()">
          <lucide-icon [img]="DropIcon" size="30"></lucide-icon>
          <span class="moisture-num">{{ plant().currentMoisture }}%</span>
        </div>        
      </div>
      <div class="corner-icon">        
        <lucide-icon [img]="InfoIcon" size="22"></lucide-icon>
      </div>
    </div>
      

      <h3>{{ plant().name }}</h3>

      <div class="description">
        <p>{{ plant().description }}</p>
      </div>      

      <div class="header">
        <span class="group">{{ plant().groupName }}</span>
        <span class="date">{{ plant().lastUpdate | date:'shortTime' }} | {{ plant().lastUpdate | date:'dd/MM/yyyy' }}</span>        
      </div>

    </div>
  `,
  styleUrls: ['./plant-card-dash.component.scss']
})
export class PlantCardDashComponent {
  plant = input.required<Plant>();
  readonly DropIcon = Droplets;
  readonly InfoIcon = Info;

  isLow = computed(() => this.plant().currentMoisture < 30);
}