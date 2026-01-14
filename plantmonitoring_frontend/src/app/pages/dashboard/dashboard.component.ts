import { Component, signal, inject, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { toSignal, toObservable } from '@angular/core/rxjs-interop'; 
import { switchMap, filter, tap } from 'rxjs/operators';
import { of } from 'rxjs';

import { LucideAngularModule, Sprout } from 'lucide-angular';
import { FormsModule } from '@angular/forms';

// components and services
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

  // plant signal
  plants = toSignal(this.plantService.getPlants(), { initialValue: [] });

  // automatically manages subscription to health 
  sensorStatusData = toSignal(this.sensorService.getSensorHealth(), { initialValue: [] });

  // user selections
  selectedHistoryPlantId = signal<number>(0);
  selectedTimeRange = signal<number>(24);

  // a stream that emits whenever ID or range changes
  private historyParams$ = toObservable(computed(() => ({
    id: this.selectedHistoryPlantId(),
    range: this.selectedTimeRange()
  })));

  // automatically fetches data when params change
  historyData = toSignal(
    this.historyParams$.pipe(
      switchMap(params => {                        // cancels pending requests if user changes selection quickly
        if (params.id === 0) return of([]); 
        return this.plantService.getSensorHistory(params.id, params.range);
      })
    ),
    { initialValue: [] }
  );

  constructor() {
    effect(() => {
      const plantsList = this.plants();
      const currentId = this.selectedHistoryPlantId();

      if (plantsList.length > 0 && currentId === 0) {
        this.selectedHistoryPlantId.set(plantsList[0].id);
      }
    }, { allowSignalWrites: true });
  }

  onHistoryPlantChanged(newId: number) {
    this.selectedHistoryPlantId.set(Number(newId));
  }

  onTimeRangeChanged(newRange: number) {
    this.selectedTimeRange.set(newRange);
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

    // create plant cards
    const plantWidgets: DashboardWidget[] = plantList.map(plant => ({
      id: `plant-${plant.id}`, 
      type: 'plant',           
      cols: 3,                 
      rows: 1,
      data: plant             
    }));

    // spacer logic
    const usedCols = plantWidgets.reduce((acc, w) => acc + w.cols, 0);
    const remainder = usedCols % 12;

    if (usedCols > 0 && remainder !== 0) {
      plantWidgets.push({
        id: 'spacer-1', 
        type: 'spacer', 
        cols: 12 - remainder, rows: 1, 
        title: '', 
        data: null
      });
    } else if (usedCols === 0) {
      plantWidgets.push({
        id: 'noplants', 
        type: 'noplants', 
        cols: 12, rows: 1, 
        title: '', 
        data: null
      });
    }

    // inject data into static widgets
    const transformedStaticWidgets = this.staticWidgets().map(widget => {
      if (widget.id === 'hist') {
        return { 
          ...widget, 
          data: history,
          title: history?.length > 0 ? 'Moisture History' : 'Moisture History (No Data)'
        };
      }
      if (widget.id === 'health') {
        return { ...widget, data: healthData };
      }
      return widget; 
    });

    return [...plantWidgets, ...transformedStaticWidgets];
  });
}