import { Component, signal, inject, computed } from '@angular/core';
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
        
        <div class="widget-card" 
             [style.--col-span]="widget.cols" 
             [style.--row-span]="widget.rows">

          @if (widget.type === 'plant') {
            <app-plant-card-dash [plant]="widget.data"></app-plant-card-dash>
          } 
          
          @else {
            <div class="card-header">
              <h3>{{ widget.title }}</h3>
              <button>...</button>
            </div>
            <div class="card-body">
              {{ widget.type }} content here...
            </div>
          }

        </div>
      }

    </div>
  `,
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent {
  private plantService = inject(PlantService);

  // toSignal automatically subscribes and unsubscribes
  plants = toSignal(this.plantService.getPlants(), { initialValue: [] });

  // sample data
  staticWidgets = signal<DashboardWidget[]>([
    { id: 'hist', title: 'Growth History', type: 'chart', cols: 8, rows: 2 },
    { id: 'alerts', title: 'Recent Alerts',  type: 'list',  cols: 4, rows: 2 },
    { id: 'health', title: 'System Health',  type: 'chart', cols: 12, rows: 1 },
  ]);

  allWidgets = computed(() => {
    const plantList = this.plants();
    
    // transform API data into Widget format
    const plantWidgets: DashboardWidget[] = plantList.map(plant => ({
      id: `plant-${plant.id}`, 
      type: 'plant',           
      cols: 3,                 
      rows: 1,
      data: plant             
    }));

    return [...plantWidgets, ...this.staticWidgets()];
  });
}