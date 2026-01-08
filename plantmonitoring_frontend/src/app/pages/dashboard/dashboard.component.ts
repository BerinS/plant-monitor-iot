import { Component, signal, inject, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { toSignal } from '@angular/core/rxjs-interop'; 
import { PlantService } from '../../services/plant.service';
import { DashboardWidget, Plant } from '../../models/dashboard.model';
import { PlantCardDashComponent } from '../../components/plantCardDash/plant-card-dash.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, PlantCardDashComponent], 
  template: `
    <div class="dashboard-grid">
  
      @for (widget of allWidgets(); track widget.id) {
        
        @if (widget.type === 'plant') {
          <div class="widget-card" [style.--col-span]="widget.cols" [style.--row-span]="widget.rows">
            <app-plant-card-dash [plant]="widget.data"></app-plant-card-dash>
          </div>
        } 
        
        @else if (widget.type === 'spacer') {
          <div [style.--col-span]="widget.cols" class="spacer"></div>
        }

        @else if (widget.type === 'noplants') {
          <div [style.--col-span]="widget.cols" [style.--row-span]="widget.rows" class="noplants">No plants to show</div>
        }

        @else {
          <div class="widget-card" [style.--col-span]="widget.cols" [style.--row-span]="widget.rows">
            <div class="card-header">
              <h3>{{ widget.title }}</h3>
              <button>...</button>
            </div>
            <div class="card-body">
              {{ widget.type }} content...
            </div>
          </div>
        }

      }

    </div>
  `,
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent {
  private plantService = inject(PlantService);

  // toSignal automatically subscribes and unsubscribes
  plants = toSignal(this.plantService.getPlants(), { initialValue: [] });

  constructor() {
  effect(() => {
      const data = this.plants();
      if (data) {
        console.log('Plant data received:', data);
      }
    });
  }

  // sample data
  staticWidgets = signal<DashboardWidget[]>([
    { id: 'hist', title: 'Growth History', type: 'chart', cols: 8, rows: 2 },
    { id: 'alerts', title: 'Recent Alerts',  type: 'list',  cols: 4, rows: 2 },
    { id: 'health', title: 'System Health',  type: 'chart', cols: 12, rows: 1 },
  ]);

  allWidgets = computed(() => {
    const plantList = this.plants();
    
    const activePlants = plantList;

    const plantWidgets: DashboardWidget[] = activePlants.map(plant => ({
      id: `plant-${plant.id}`, 
      type: 'plant',           
      cols: 3,                 
      rows: 1,
      data: plant             
    }));

    // calculate remaining space for spacers - used to fill row if there's leftover space    
    const usedCols = plantWidgets.reduce((acc, w) => acc + w.cols, 0);
    const remainder = usedCols % 12;

    // spacer logic
    if (usedCols > 0 && remainder !== 0) {
      const spacerWidth = 12 - remainder;
      
      plantWidgets.push({
        id: 'spacer-1',
        type: 'spacer',
        cols: spacerWidth,
        rows: 1,
        title: '',
        data: null
      });
    }

    else if (usedCols === 0) {
      plantWidgets.push({
        id: 'noplants',
        type: 'noplants',
        cols: 12,
        rows: 1,
        title: '',
        data: null
      });
    }

    return [...plantWidgets, ...this.staticWidgets()];
  });
}