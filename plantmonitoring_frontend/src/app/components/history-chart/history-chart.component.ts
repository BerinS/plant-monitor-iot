import { Component, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgxChartsModule, Color, ScaleType } from '@swimlane/ngx-charts'; // Import this
import { Plant } from '../../models/dashboard.model';

@Component({
  selector: 'app-history-chart',
  standalone: true,
  imports: [CommonModule, NgxChartsModule],
  template: `
    <div class="chart-container">      
      @if (chartData().length > 0) {
        <ngx-charts-area-chart
          [results]="chartData()"
          [gradient]="true"
          [scheme]="colorScheme"
          [xAxis]="true"
          [yAxis]="true"          
          xAxisLabel="Time"
          [autoScale]="true">
        </ngx-charts-area-chart>
      }
    </div>
  `,
  styleUrls: ['./history-chart-component.scss']
})
export class HistoryChartComponent {
  // raw list from API
  historyData = input.required<any[]>(); 

  chartData = computed(() => {
    const rawData = this.historyData();
    if (!rawData || rawData.length === 0) return [];

    // reverse array so chart goes old -> new and map to ngx format
    const transformedSeries = [...rawData].reverse().map(item => ({
      name: item.formattedTime.split(' ').pop(), // just the time 
      value: item.value
    }));

    return [
      {
        name: 'Moisture',
        series: transformedSeries
      }
    ];
  });

  colorScheme: Color = {
    name: 'myScheme',
    selectable: true,
    group: ScaleType.Ordinal, 
    domain: ['#1a7249', '#A10A28', '#C7B42C', '#AAAAAA'] 
  };
}