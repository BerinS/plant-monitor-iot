import { Component, input, computed, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgxChartsModule, Color, ScaleType } from '@swimlane/ngx-charts'; 
import { Plant } from '../../models/dashboard.model';
import { curveNatural, curveMonotoneX } from 'd3-shape'; // used for line chart curve interpolation

@Component({
  selector: 'app-history-chart',
  standalone: true,
  imports: [CommonModule, NgxChartsModule],
  template: `
    <div class="chart-wrapper">
      <div class="controls">
        <div class="time-toggles">
          @for (opt of timeOptions; track opt.value) {
            <button 
              [class.active]="selectedRange() === opt.value"
              (click)="onRangeSelect(opt.value)">
              {{ opt.label }}
            </button>
          }
        </div>
      </div>

      <div class="chart-container">      
        @if (historyData().length > 0) {
           <ngx-charts-area-chart
             [results]="chartData()"
             [curve]="curveMonotoneX"
             [gradient]="true"
             [scheme]="colorScheme"
             [xAxis]="true"
             [yAxis]="true"          
             [autoScale]="true">
           </ngx-charts-area-chart>
        } @else {
           <p class="no-data">No data for this time range</p>
        }
      </div>
    </div>
  `,
  styleUrls: ['./history-chart-component.scss']
})
export class HistoryChartComponent {
  readonly curveNatural = curveNatural;
  readonly curveMonotoneX = curveMonotoneX;

  // raw list from API
  historyData = input.required<any[]>(); 
  selectedRange = input<number>(24);
  rangeChange = output<number>();

  timeOptions = [
    { label: '24H', value: 24 },
    { label: '7D', value: 168 },
    { label: '30D', value: 720 }
  ];

  onRangeSelect(hours: number) {
    this.rangeChange.emit(hours);
  }


  chartData = computed(() => {
    const rawData = this.historyData();
    if (!rawData || rawData.length === 0) return [];

    // max points on chart
    const targetPoints = 50;
    
    // step calculation
    const step = Math.ceil(rawData.length / targetPoints);

    const transformedSeries = [...rawData]
      .filter((_, index) => index % step === 0) // dynamic step to avoid clutter
      .reverse()
      .map(item => ({
        name: item.formattedTime.split(' ').pop() + ' ' + item.formattedTime.split(' ')[0]  + item.formattedTime.split(' ')[1], // time, day, month
        value: item.value
      }));

    return [{ name: 'Moisture', series: transformedSeries }];
  });

  colorScheme: Color = {
    name: 'myScheme',
    selectable: true,
    group: ScaleType.Ordinal, 
    domain: ['#1a7249', '#A10A28', '#C7B42C', '#AAAAAA'] 
  };
}