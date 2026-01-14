import { Component, signal, inject, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { toSignal } from '@angular/core/rxjs-interop'; 
import { LucideAngularModule, Sprout } from 'lucide-angular';
import { FormsModule } from '@angular/forms';
import { PlantService } from '../../services/plant.service';
import { SensorService } from '../../services/sensor.service';
import { DashboardWidget, Plant, SensorHealth } from '../../models/dashboard.model';
import { HealthWidgetComponent } from '../../components/health-widget/health-widget.component';
import { PlantCardDashComponent } from '../../components/plantCardDash/plant-card-dash.component';
import { HistoryChartComponent } from '../../components/history-chart/history-chart.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, PlantCardDashComponent, HistoryChartComponent, LucideAngularModule, FormsModule, HealthWidgetComponent], 
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})

export class DashboardComponent {
  readonly Sprout = Sprout;
  private plantService = inject(PlantService);
  private sensorService = inject(SensorService);

  // Signals
  plants = toSignal(this.plantService.getPlants(), { initialValue: [] });
  historyData = signal<any[]>([]);  
  selectedHistoryPlantId = signal<number>(0);
  selectedTimeRange = signal<number>(24);
  sensorStatusData = signal<SensorHealth[]>([]);


  fetchHistory(plantId: number, range: number) {
    this.plantService.getSensorHistory(plantId, range).subscribe(data => {
      this.historyData.set(data);
    });
  }

  // called by dropdown for selecting plant
  onHistoryPlantChanged(newId: number) {
    this.selectedHistoryPlantId.set(Number(newId));
    // fetch with new id but keep existing range
    this.fetchHistory(Number(newId), this.selectedTimeRange());
  }

  onTimeRangeChanged(newRange: number) {
    this.selectedTimeRange.set(newRange);
    // detch with existing id but new range
    this.fetchHistory(this.selectedHistoryPlantId(), newRange);
  }

  constructor() {
    effect(() => {
      const data = this.plants();

      if (data.length > 0 && this.selectedHistoryPlantId() === 0) {
        const firstPlant = data[0];
        
        // fetch history for first plant
        this.selectedHistoryPlantId.set(firstPlant.id);
        this.fetchHistory(firstPlant.id, 24)
      }

      // fetch system health
      this.sensorService.getSensorHealth().subscribe(data => {
        this.sensorStatusData.set(data);
      });
    });
  }

  staticWidgets = signal<DashboardWidget[]>([
    { id: 'hist', title: 'Moisture History', type: 'chart', cols: 8, rows: 2 },
    { id: 'alerts', title: 'Recent Alerts',  type: 'list',  cols: 4, rows: 2 },
    { id: 'health', title: 'System Health',  type: 'chart', cols: 12, rows: 1 },
  ]);

  allWidgets = computed(() => {
    const plantList = this.plants();
    const history = this.historyData(); 
    const healthData = this.sensorStatusData();
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

      if (widget.id === 'health') {
        return { 
          ...widget, 
          data: healthData,
          title: healthData.length > 0 ? 'System Health' : 'System Health (No Data)'
        };
      }
    
      return widget; 
    });

    // combine everything
    return [...plantWidgets, ...transformedStaticWidgets];
  });
}