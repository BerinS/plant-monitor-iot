import { Component, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SensorHealth } from '../../models/dashboard.model';
import { LucideAngularModule, CircleDot} from 'lucide-angular';


@Component({
  selector: 'app-health-widget',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  template: `
    <div class="health-container">
      
      <div class="summary-panel" [class.ok]="stats().isSystemHealthy">        
        <div class="summary-text">
          <p>{{ stats().onlineCount }} / {{ stats().total }} Sensors online</p>
        </div>
      </div>

      <div class="node-grid">
        @for (sensor of stats().nodes; track sensor.id) {
          <div class="node-card" [class.offline]="sensor.status === 'offline'" [class.online]="sensor.status === 'online'" [class.idle]="sensor.status === 'idle'">
            <div class="card-header">
              <span class="device-name">{{ sensor.name }}</span>
              <span class="badge" [ngClass]="'status-' + sensor.status">
                <div class="circle"> <lucide-icon [img]="CircleDot" size="14" ></lucide-icon></div>
                <div class="status-label">{{ sensor.status }}</div>
              </span>
            </div>

            <div class="metrics">
              @if (sensor.status !== 'idle') {
                <div class="label-value">
                  <span class="label">Last update: </span>
                  <span class="metrics-value">{{ sensor.lastSeenRelative }}</span>
                </div>
              }
              <div class="label-value">
                <span class="label">Assigned plant: </span>
                <span class="metrics-value">{{ sensor.plant }}</span>
              </div>
            </div>
          </div>
        }
      </div>
    </div>
  `,
  styleUrls: ['./health-widget.component.scss']
})
export class HealthWidgetComponent {
  //  receives the raw list
  sensors = input.required<SensorHealth[]>();

  readonly CircleDot = CircleDot;

  // logic to process timestamps
  stats = computed(() => {
    const rawList = this.sensors() || [];
    const now = new Date().getTime();
    const TIMEOUT_MS = 1000 * 60 * 60; // 1 hour timeout

    const processedNodes = rawList.map(s => {
      // a sensor with no plant assigned has never reported readings —
      // it's idle, not offline, so it shouldn't be judged against the timeout
      if (s.lastContact == null) {
        return {
          id: s.id,
          name: s.name,
          plant: s.assignedPlant,
          status: 'idle' as const,
          lastSeenRelative: 'No data yet'
        };
      }

      const lastContactTime = new Date(s.lastContact).getTime();
      const status: 'online' | 'offline' = (now - lastContactTime) < TIMEOUT_MS ? 'online' : 'offline';

      return {
        id: s.id,
        name: s.name,
        plant: s.assignedPlant,
        status,
        lastSeenRelative: this.timeSince(lastContactTime)
      };
    });

    const onlineCount = processedNodes.filter(n => n.status === 'online').length;

    return {
      total: rawList.length,
      onlineCount: onlineCount,
      isSystemHealthy: onlineCount === rawList.length && rawList.length > 0,
      nodes: processedNodes
    };
  });

  timeSince(date: number) {
    const seconds = Math.floor((new Date().getTime() - date) / 1000);
    let interval = seconds / 31536000;
    if (interval > 1) return Math.floor(interval) + " years ago";
    interval = seconds / 2592000;
    if (interval > 1) return Math.floor(interval) + " months ago";
    interval = seconds / 86400;
    if (interval > 1) return Math.floor(interval) + " days ago";
    interval = seconds / 3600;
    if (interval > 1) return Math.floor(interval) + " hours ago";
    interval = seconds / 60;
    if (interval > 1) return Math.floor(interval) + " mins ago";
    return Math.floor(seconds) + " seconds ago";
  }
}