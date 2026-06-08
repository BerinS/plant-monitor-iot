import { Component, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, Plus } from 'lucide-angular';
import { ToastrService } from 'ngx-toastr';

import { ModalComponent } from '../../components/modal/modal.component';
import { SensorCardSensors } from '../../components/sensor-card-sensors/sensor-card-sensors.component';
import { SensorService } from '../../services/sensor.service';
import { Sensor } from '../../models/sensor.model';
import { SensorHealth } from '../../models/dashboard.model';

@Component({
  selector: 'app-sensors',
  standalone: true,
  imports: [SensorCardSensors, LucideAngularModule, ModalComponent, FormsModule],
  templateUrl: './sensors.component.html',
  styleUrl: './sensors.component.scss',
})
export class SensorsComponent {
  readonly Plus = Plus;
  private sensorService = inject(SensorService);
  private toastr = inject(ToastrService);

  isModalOpen = signal(false);
  activeSensor = signal<Sensor | null>(null);
  modalType = signal<'default' | 'danger' | 'info'>('default');
  modalMode = signal<'edit' | 'delete' | 'add' | 'default'>('default');
  sensors = signal<Sensor[]>([]);
  sensorHealth = signal<SensorHealth[]>([]);
  sortMode = signal<'newest' | 'oldest' | 'name'>('newest');

  constructor() {
    this.sensorService.getAllSensors().subscribe(data => {
      this.sensors.set(data);
    });

    // used only to derive each card's online/offline/idle status (same rule as the health widget)
    this.sensorService.getSensorHealth().subscribe(data => {
      this.sensorHealth.set(data);
    });
  }

  // mirrors the status logic in health-widget.component.ts: no contact ever -> idle,
  // contact within the last hour -> online, otherwise offline
  statusById = computed(() => {
    const now = new Date().getTime();
    const TIMEOUT_MS = 1000 * 60 * 60; // 1 hour timeout
    const map = new Map<number, 'online' | 'offline' | 'idle'>();

    for (const h of this.sensorHealth()) {
      if (h.lastContact == null) {
        map.set(h.id, 'idle');
      } else {
        const lastContactTime = new Date(h.lastContact).getTime();
        map.set(h.id, (now - lastContactTime) < TIMEOUT_MS ? 'online' : 'offline');
      }
    }

    return map;
  });

  sortedSensors = computed(() => {
    // [...this.sensors()] creates a copy
    const currentSensors = [...this.sensors()]; 

    switch (this.sortMode()) {
      case 'newest':
        return currentSensors.sort((a, b) => b.id - a.id);
      case 'oldest':
        return currentSensors.sort((a, b) => a.id - b.id);
      case 'name':
        return currentSensors.sort((a, b) => (a.name || '').localeCompare(b.name || ''));
      default:
        return currentSensors;
    }
  }); 

  handleEdit(sensor: Sensor) {
    this.activeSensor.set({ ...sensor });
    this.modalType.set('info'); // visual style
    this.modalMode.set('edit'); // logic mode
    this.isModalOpen.set(true);
  }

  handleAdd() {
    // dummy sensor to populate form
    this.activeSensor.set({
      id: 0, 
      name: '',
      macAddress: '',
      description: '',
      apiToken: '',
      currentPlantId: null,
      plantName: null,
      groupId: null
    } as Sensor);
    
    this.modalType.set('default'); 
    this.modalMode.set('add'); 
    this.isModalOpen.set(true);
  }

  handleDelete(sensor: Sensor) {
    this.activeSensor.set(sensor);
    this.modalType.set('danger'); 
    this.modalMode.set('delete'); 
    this.isModalOpen.set(true);
  }  

  closeModal() {
    this.isModalOpen.set(false);
    this.activeSensor.set(null);
    this.modalMode.set('default'); // Reset
  }

  saveChanges() {
    const updatedSensor = this.activeSensor();
    
    if (updatedSensor && updatedSensor.id) {
      const payload = {
        id: updatedSensor.id,
        name: updatedSensor.name,
        macAddress: updatedSensor.macAddress,
        currentPlantId: updatedSensor.currentPlantId,
        groupId: updatedSensor.groupId,
        description: updatedSensor.description ?? ''
      };

      console.log('Sending payload to backend:', payload);

      this.sensorService.updateSensor(updatedSensor.id, payload).subscribe({
        next: () => {
          this.closeModal();
          
          this.toastr.success(
            `${updatedSensor.name} has been updated.`, 
            'Success!'
          );

          this.sensors.update(currentSensors => 
            currentSensors.map(s => 
              s.id === updatedSensor.id ? { ...updatedSensor } : s
            )
          );
        },
        error: (err) => {
          console.error('Failed to update sensor. Error:', err);
          this.closeModal();          
          this.toastr.error('Could not save changes.', 'Error');
        }
      });
    }
  }

  confirmDelete() {
    const sensorToDelete = this.activeSensor();
    
    if (sensorToDelete && sensorToDelete.id) {      
      this.sensorService.deleteSensor(sensorToDelete.id).subscribe({
        next: (response) => {
          console.log('Sensor successfully deleted');          
          this.closeModal(); 

          this.toastr.success(
            `Sensor ${sensorToDelete.name} deleted successfully.`, 
            'Success'
          );
          
          this.sensors.update(currentSensors => 
            currentSensors.filter(s => s.id !== sensorToDelete.id)
          );
        },
        error: (err) => {
          console.error('Failed to delete sensor. Error:', err);
          this.toastr.error('Could not delete the sensor.', 'Error');
        }
      });
    }
  }

  createNewSensor() {
    const newSensor = this.activeSensor();
    
    if (newSensor) {

      const payload = {
        name: newSensor.name ?? '',
        macAddress: newSensor.macAddress,
        currentPlantId: newSensor.currentPlantId, 
        groupId: newSensor.groupId,               
        description: newSensor.description ?? ''
      };

      this.sensorService.addSensor(payload).subscribe({
        next: (createdSensorFromDb: Sensor) => {
          this.closeModal();
          this.toastr.success(`${payload.name} has been added.`, 'Success');

          this.sensors.update(currentSensors => [...currentSensors, createdSensorFromDb]);
        },
        error: (err) => {
          console.error('Failed to add sensor. Error:', err);
          this.closeModal(); 
          this.toastr.error('Could not add sensor.', 'Error');
        }
      });
    }
  }
}