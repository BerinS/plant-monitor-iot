import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { LucideAngularModule, Sprout } from 'lucide-angular';
import { ToastrService } from 'ngx-toastr';

import { ModalComponent } from '../../components/modal/modal.component';
import { PlantCardMyPlants } from '../../components/plantCard-my-plants/plant-card-my-plants.component'
import { PlantService } from '../../services/plant.service';
import { Plant } from '../../models/my-plants.model';

@Component({
  selector: 'app-my-plants',
  imports: [PlantCardMyPlants, LucideAngularModule, ModalComponent, FormsModule],
  templateUrl: './my-plants.component.html',
  styleUrl: './my-plants.component.scss',
})
export class MyPlantsComponent {
  readonly Sprout = Sprout;
  private plantService = inject(PlantService);

  isModalOpen = signal(false);
  activePlant = signal<Plant | null>(null);
  modalType = signal<'default' | 'danger' | 'info'>('default');
  modalMode = signal<'edit' | 'delete' | 'default'>('default');
  plants = signal<Plant[]>([]);

  private toastr = inject(ToastrService);

  constructor() {
    this.plantService.getPlants().subscribe(data => {
      this.plants.set(data);
    });
  }

  handleEdit(plant: Plant) {
    this.activePlant.set({ ...plant });
    this.modalType.set('info'); // visual style
    this.modalMode.set('edit'); // logic mode
    this.isModalOpen.set(true);
  }

  handleDelete(plant: Plant) {
    this.activePlant.set(plant);
    this.modalType.set('danger'); 
    this.modalMode.set('delete'); 
    this.isModalOpen.set(true);
  }

  closeModal() {
    this.isModalOpen.set(false);
    this.activePlant.set(null);
    this.modalMode.set('default'); // Reset
  }

  saveChanges() {
    const updatedPlant = this.activePlant();
    if (updatedPlant) {
      console.log('Saving changes:', updatedPlant);
      // service call here
      this.closeModal();
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
          console.error('Failed to delete plant. Server said:', err);

          this.toastr.error('Could not delete the plant. Please try again.', 'Error!');
        }
      });
    }
  }

  
}