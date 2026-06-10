import { Component, input, output, computed } from '@angular/core';
import { LucideAngularModule, Pen, Trash2, Wifi, WifiOff, CircleDot } from 'lucide-angular';
import { Sensor } from '../../models/sensor.model';

@Component({
  selector: 'app-sensor-card-sensors',
  imports: [LucideAngularModule],
  templateUrl: './sensor-card-sensors.component.html',
  styleUrl: './sensor-card-sensors.component.scss',
})
export class SensorCardSensors {
  readonly PenIcon = Pen;
  readonly TrashIcon = Trash2;
  readonly WifiIcon = Wifi;
  readonly WifiOffIcon = WifiOff;
  readonly CircleDot = CircleDot;

  sensor = input.required<Sensor>();
  status = input<'online' | 'offline' | 'idle'>('idle');
  editRequest = output<Sensor>();
  deleteRequest = output<Sensor>();

  statusIcon = computed(() => {
    switch (this.status()) {
      case 'online': return this.WifiIcon;
      case 'offline': return this.WifiOffIcon;
      default: return this.CircleDot;
    }
  });

  onEdit() {
    this.editRequest.emit(this.sensor());
  }

  onDelete() {
    this.deleteRequest.emit(this.sensor());
  }
}
