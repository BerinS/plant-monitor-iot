import { Component, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { LucideAngularModule, Sprout, Plus } from 'lucide-angular';
import { ToastrService } from 'ngx-toastr';

import { ModalComponent } from '../../components/modal/modal.component';
import { PlantCardMyPlants } from '../../components/plantCard-my-plants/plant-card-my-plants.component'
import { PlantService } from '../../services/plant.service';
import { GroupService } from '../../services/group.service';
import { Plant } from '../../models/plant.model';

@Component({
  selector: 'app-my-plants',
  imports: [PlantCardMyPlants, LucideAngularModule, ModalComponent, FormsModule],
  templateUrl: './my-plants.component.html',
  styleUrl: './my-plants.component.scss',
})
export class MyPlantsComponent {
  readonly Sprout = Sprout;
  readonly Plus = Plus;
  private plantService = inject(PlantService);
  private groupService = inject(GroupService);
  private toastr = inject(ToastrService);

  isModalOpen = signal(false);
  activePlant = signal<Plant | null>(null);
  modalType = signal<'default' | 'danger' | 'info'>('default');
  modalMode = signal<'edit' | 'delete' | 'add' | 'water' | 'default'>('default');
  wateringDuration = signal(1);
  plants = signal<Plant[]>([]); 
  sortMode = signal<'newest' | 'oldest' | 'name'>('newest');
  groups = toSignal(this.groupService.getGroups(), { initialValue: [] });

  constructor() {
    this.plantService.getPlants().subscribe(data => {
      this.plants.set(data);
    });
  }

  sortedPlants = computed(() => {
    // [...this.plants()] creates a copy
    const currentPlants = [...this.plants()]; 

    switch (this.sortMode()) {
      case 'newest':
        return currentPlants.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
      case 'oldest':
        return currentPlants.sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime());
      case 'name':
        return currentPlants.sort((a, b) => a.name.localeCompare(b.name));
      default:
        return currentPlants;
    }
  }); 

  handleEdit(plant: Plant) {
    this.activePlant.set({ ...plant });
    this.modalType.set('info'); // visual style
    this.modalMode.set('edit'); // logic mode
    this.isModalOpen.set(true);
  }

  handleAdd() {
    // dummy plant to populate form
    this.activePlant.set({
      id: 0,
      name: '',
      description: '',
      createdAt: new Date().toISOString(),
      groupName: 'General',
      moistureThreshold: null,
      currentMoisture: 0,
      sensorName: 'No Sensor Assigned',
      lastUpdate: ''
    } as Plant);
    
    this.modalType.set('default'); 
    this.modalMode.set('add'); 
    this.isModalOpen.set(true);
  }

  handleDelete(plant: Plant) {
    this.activePlant.set(plant);
    this.modalType.set('danger');
    this.modalMode.set('delete');
    this.isModalOpen.set(true);
  }

  isWateringDurationValid = computed(() => {
    const value = this.wateringDuration();
    return Number.isInteger(value) && value >= 1 && value <= 8;
  });

  adjustWateringDuration(delta: number) {
    const next = Math.round(this.wateringDuration()) + delta;
    if (next >= 1 && next <= 8) {
      this.wateringDuration.set(next);
    }
  }

  handleWater(plant: Plant) {
    this.activePlant.set(plant);
    this.wateringDuration.set(1);
    this.modalType.set('info');
    this.modalMode.set('water');
    this.isModalOpen.set(true);
  }

  closeModal() {
    this.isModalOpen.set(false);
    this.activePlant.set(null);
    this.modalMode.set('default'); // Reset
  }

  activatePump() {
    const plant = this.activePlant();
    if (!plant) return;

    const duration = Math.min(8, Math.max(1, Math.round(this.wateringDuration())));

    this.plantService.triggerWatering(plant.id, duration).subscribe({
      next: (response) => {
        this.closeModal();
        this.toastr.success(
          `${response.message} (${response.durationSeconds}s, device #${response.deviceId})`,
          'Watering started'
        );
      },
      error: (err) => {
        console.error('Failed to trigger watering. Error:', err);
        this.closeModal();
        this.toastr.error(err?.error?.message || 'Could not start watering.', 'Error');
      }
    });
  }


  saveChanges() {
    const updatedPlant = this.activePlant();
    
    if (updatedPlant && updatedPlant.id) {
      
      const selectedGroup = this.groups().find(g => g.name === updatedPlant.groupName);
      
      const groupIdToSave = selectedGroup ? selectedGroup.id : null; 

      const payload = {
        id: updatedPlant.id,
        name: updatedPlant.name,
        description: updatedPlant.description,
        groupId: groupIdToSave,
        moistureThreshold: updatedPlant.moistureThreshold
      };

      console.log('Sending payload to backend:', payload);

      this.plantService.updatePlant(updatedPlant.id, payload).subscribe({
        next: () => {
          this.closeModal();
          
          this.toastr.success(
            `${updatedPlant.name} has been updated.`, 
            'Success!'
          );

          this.plants.update(currentPlants => 
            currentPlants.map(p => 
              p.id === updatedPlant.id ? { ...updatedPlant } : p
            )
          );
        },
        error: (err) => {
          console.error('Failed to update plant. Error:', err);
          this.closeModal();          
          this.toastr.error('Could not save changes.', 'Error');
        }
      });
    }
  }

  confirmDelete() {
    const plantToDelete = this.activePlant();
    
    if (plantToDelete && plantToDelete.id) {      
      this.plantService.deletePlant(plantToDelete.id).subscribe({
        next: (response) => {
          console.log('Plant successfully deleted');          
          this.closeModal(); 

          this.toastr.success(
            `Plant ${plantToDelete.name} deleted successfully.`, 
            'Success'
          );
          
          this.plants.update(currentPlants => 
            currentPlants.filter(p => p.id !== plantToDelete.id)
          );
        },
        error: (err) => {
          console.error('Failed to delete plant. Error:', err);

          this.toastr.error('Could not delete the plant.', 'Error');
        }
      });
    }
  }

  createNewPlant() {
    const newPlant = this.activePlant();
    
    if (newPlant) {
      // Find the group ID just like we did for edit
      const selectedGroup = this.groups().find(g => g.name === newPlant.groupName);
      const groupIdToSave = selectedGroup ? selectedGroup.id : null; 

      // Build the POST payload (Notice we don't send an ID, the DB creates it)
      const payload = {
        name: newPlant.name,
        description: newPlant.description,
        groupId: groupIdToSave,
        moistureThreshold: newPlant.moistureThreshold
      };

      this.plantService.addPlant(payload).subscribe({
        next: (createdPlantFromDb) => {
          this.closeModal();
          this.toastr.success(`${newPlant.name} has been added!`, 'Success');

          // Add the newly created plant (which now has a real DB ID) to the grid
          this.plants.update(currentPlants => [...currentPlants, createdPlantFromDb]);
        },
        error: (err) => {
          console.error('Failed to add plant. Error:', err);
          this.closeModal(); // Closing modal first to avoid z-index bugs with toastr!
          this.toastr.error('Could not add the plant. Please try again.', 'Error');
        }
      });
    }
  }

  
}