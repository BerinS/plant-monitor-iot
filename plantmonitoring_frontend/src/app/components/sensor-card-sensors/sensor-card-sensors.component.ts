import { Component, input, output } from '@angular/core';
import { LucideAngularModule, Pen, Trash2, CircleDot } from 'lucide-angular';
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
  readonly CircleDot = CircleDot;

  sensor = input.required<Sensor>();
  editRequest = output<Sensor>();
  deleteRequest = output<Sensor>();

  onEdit() {
    this.editRequest.emit(this.sensor());
  }

  onDelete() {
    this.deleteRequest.emit(this.sensor());
  }
}
