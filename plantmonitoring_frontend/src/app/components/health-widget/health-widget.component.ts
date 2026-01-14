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
          <div class="node-card" [class.offline]="!sensor.isOnline" [class.online]="sensor.isOnline">
            <div class="card-header">
              <span class="mac-address">Mac: {{ sensor.mac }}</span>
              <span class="badge" [class.red]="!sensor.isOnline" [class.green]="sensor.isOnline">
                <div class="circle"> <lucide-icon [img]="CircleDot" size="14" ></lucide-icon></div> 
                <div class="onlineORoffline">{{ sensor.isOnline ? 'online' : 'offline' }}</div>
              </span>
            </div>
            
            <div class="metrics">
              <div class="label-value">
                <span class="label">Last update: </span>
                <span class="metrics-value">{{ sensor.lastSeenRelative }}</span>
              </div>
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
      const lastContactTime = new Date(s.lastContact).getTime();
      const diff = now - lastContactTime;
      const isOnline = diff < TIMEOUT_MS;

      return {
        id: s.id,
        plant: s.assignedPlant || 'Unassigned Sensor',
        mac: s.macAddress,
        isOnline: isOnline,
        lastSeenRelative: this.timeSince(lastContactTime)
      };
    });

    const onlineCount = processedNodes.filter(n => n.isOnline).length;

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