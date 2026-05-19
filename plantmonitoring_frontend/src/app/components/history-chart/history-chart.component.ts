import {
  Component, input, output, computed, effect,
  ElementRef, viewChild, OnDestroy, inject
} from '@angular/core';
import { CommonModule } from '@angular/common';
import * as echarts from 'echarts';
import type { ECharts, EChartsOption } from 'echarts';

@Component({
  selector: 'app-history-chart',
  standalone: true,
  imports: [CommonModule],
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
          <div #chartEl class="echart"></div>
        } @else {
          <p class="no-data">No data for this time range</p>
        }
      </div>

    </div>
  `,
  styleUrls: ['./history-chart-component.scss']
})
export class HistoryChartComponent implements OnDestroy {
  historyData = input.required<any[]>();
  selectedRange = input<number>(24);
  rangeChange = output<number>();

  // reference to the chart DOM element
  chartEl = viewChild<ElementRef>('chartEl');

  private chartInstance: ECharts | null = null;
  private resizeObserver: ResizeObserver | null = null;

  timeOptions = [
    { label: '24H', value: 24 },
    { label: '7D', value: 168 },
    { label: '30D', value: 720 }
  ];

  onRangeSelect(hours: number) {
    this.rangeChange.emit(hours);
  }

  constructor() {
    // effect runs whenever historyData or selectedRange signals change
    effect(() => {
      const raw = this.historyData();
      const range = this.selectedRange();
      const el = this.chartEl();

      // el may not exist yet if historyData is empty (ngIf hides the div)
      if (!el || !raw || raw.length === 0) {
        this.chartInstance?.dispose();
        this.chartInstance = null;
        return;
      }

      // init or reuse existing instance
      if (!this.chartInstance) {
        this.chartInstance = echarts.init(el.nativeElement);

        // resize chart when container size changes
        this.resizeObserver = new ResizeObserver(() => {
          this.chartInstance?.resize();
        });
        this.resizeObserver.observe(el.nativeElement);
      }

      this.chartInstance.setOption(this.buildOptions(raw, range), true);
    });
  }

  private buildOptions(raw: any[], range: number): EChartsOption {
    const sorted = [...raw].reverse();

    const xData = sorted.map(item => item.time ?? item.formattedTime);
    const yData = sorted.map(item => item.value);

    const xAxisFormatter = (val: string) => {
      const date = new Date(val);
      if (isNaN(date.getTime())) return val;
      return range <= 24
        ? date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
        : date.toLocaleDateString([], { day: '2-digit', month: 'short' });
    };

    const tooltipFormatter = (val: string) => {
      const date = new Date(val);
      if (isNaN(date.getTime())) return val;
      return date.toLocaleString([], {
        day: '2-digit', month: 'short',
        hour: '2-digit', minute: '2-digit'
      });
    };

    const maxLabels = range <= 24 ? 8 : range <= 168 ? 7 : 10;

    return {
      animation: true,
      animationDuration: 400,
      grid: {
        top: 20,
        right: 20,
        bottom: 80,
        left: 50
      },
      tooltip: {
        trigger: 'axis',
        backgroundColor: '#fff',
        borderColor: '#e0e0e0',
        borderWidth: 1,
        textStyle: { color: '#333', fontSize: 13 },
        formatter: (params: any) => {
          const p = params[0];
          const label = tooltipFormatter(xData[p.dataIndex]);
          return `
            <div style="font-weight:600;margin-bottom:4px">${label}</div>
            <div style="color:#1a7249;font-size:15px;font-weight:700">${p.value}%</div>
            <div style="color:#999;font-size:11px">Moisture</div>
          `;
        }
      },
      xAxis: {
        type: 'category',
        data: xData,
        boundaryGap: false,
        axisLabel: {
          formatter: xAxisFormatter,
          interval: Math.floor(xData.length / maxLabels),
          rotate: 0,
          fontSize: 11,
          color: '#888'
        },
        axisLine: { lineStyle: { color: '#e0e0e0' } },
        splitLine: { show: false }
      },
      yAxis: {
        type: 'value',
        min: 0,
        max: 100,
        interval: 20,
        axisLabel: {
          formatter: '{value}%',
          fontSize: 11,
          color: '#888'
        },
        splitLine: {
          lineStyle: { color: '#f0f0f0', type: 'dashed' }
        },
        axisLine: { show: false },
        axisTick: { show: false }
      },
      dataZoom: [
        {
          type: 'slider',
          bottom: 10,
          height: 20,
          borderColor: '#e0e0e0',
          fillerColor: 'rgba(26, 114, 73, 0.08)',
          handleStyle: { color: '#1a7249' },
          moveHandleStyle: { color: '#1a7249' },
          textStyle: { color: '#888', fontSize: 10 },
          start: range > 24 ? 70 : 0,
          end: 100
        },
        { type: 'inside' }
      ],
      series: [
        {
          name: 'Moisture',
          type: 'line',
          data: yData,
          smooth: true,
          symbol: 'none',
          lineStyle: { color: '#1a7249', width: 2 },
          areaStyle: {
            color: {
              type: 'linear',
              x: 0, y: 0, x2: 0, y2: 1,
              colorStops: [
                { offset: 0,   color: 'rgba(26, 114, 73, 0.35)' },
                { offset: 0.6, color: 'rgba(26, 114, 73, 0.12)' },
                { offset: 1,   color: 'rgba(26, 114, 73, 0)' }
              ]
            }
          },
          emphasis: { disabled: true }
        }
      ]
    };
  }

  ngOnDestroy() {
    this.resizeObserver?.disconnect();
    this.chartInstance?.dispose();
  }
}