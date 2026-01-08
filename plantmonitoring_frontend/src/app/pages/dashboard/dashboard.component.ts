import { Component, signal, inject, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { toSignal } from '@angular/core/rxjs-interop'; 
import { LucideAngularModule, Sprout } from 'lucide-angular';
import { FormsModule } from '@angular/forms';
import { PlantService } from '../../services/plant.service';
import { DashboardWidget, Plant } from '../../models/dashboard.model';
import { PlantCardDashComponent } from '../../components/plantCardDash/plant-card-dash.component';
import { HistoryChartComponent } from '../../components/history-chart/history-chart.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, PlantCardDashComponent, HistoryChartComponent, LucideAngularModule, FormsModule], 
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent {
  readonly Sprout = Sprout;
  private plantService = inject(PlantService);

  // Signals
  // toSignal automatically subscribes and unsubscribes
  plants = toSignal(this.plantService.getPlants(), { initialValue: [] });
  historyData = signal<any[]>([]);  
  selectedHistoryPlantId = signal<number>(0);

  constructor() {
    effect(() => {
      const data = this.plants();

      if (data.length > 0 && this.selectedHistoryPlantId() === 0) {
        const firstPlant = data[0];
        
        // fetch history for first plant
        this.selectedHistoryPlantId.set(firstPlant.id);
        this.fetchHistory(firstPlant.id)
      }
    });
  }

  fetchHistory(plantId: number) {
    this.plantService.getSensorHistory(plantId).subscribe(data => {
      console.log('Fetched history for plant id:', plantId, data);
      this.historyData.set(data); 
    });
  }

  // called by dropdown for selecting plant
  onHistoryPlantChanged(newId: number) {
    this.selectedHistoryPlantId.set(Number(newId)); // Ensure it is a number
    this.fetchHistory(Number(newId));
  }

  staticWidgets = signal<DashboardWidget[]>([
    { id: 'hist', title: 'Moisture History', type: 'chart', cols: 8, rows: 2 },
    { id: 'alerts', title: 'Recent Alerts',  type: 'list',  cols: 4, rows: 2 },
    { id: 'health', title: 'System Health',  type: 'chart', cols: 12, rows: 1 },
  ]);

  allWidgets = computed(() => {
    const plantList = this.plants();
    const history = this.historyData(); 
    
    const activePlants = plantList;
    const historyData = history;

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

    // inject history data into static widget
    const transformedStaticWidgets = this.staticWidgets().map(widget => {
      if (widget.id === 'hist') {
        const data = history || [];

        return { 
          ...widget, 
          data: history, // injects fetched array into widget          
          title: data.length > 0 ? 'Moisture History' : 'Moisture History (No Data)'
        };
      }
      return widget; 
    });

    // combine everything
    return [...plantWidgets, ...transformedStaticWidgets];
  });
}