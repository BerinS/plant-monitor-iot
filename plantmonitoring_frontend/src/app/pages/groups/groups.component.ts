import { Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { LucideAngularModule, Group as GroupIcon, Leaf, Droplets, FolderOpen } from 'lucide-angular';

import { GroupService } from '../../services/group.service';
import { PlantService } from '../../services/plant.service';
import { Group } from '../../models/group.model';
import { Plant } from '../../models/plant.model';

interface GroupWithPlants extends Group {
  plants: Plant[];
}

@Component({
  selector: 'app-groups',
  standalone: true,
  imports: [LucideAngularModule],
  templateUrl: './groups.component.html',
  styleUrl: './groups.component.scss',
})
export class GroupsComponent {
  readonly GroupIcon = GroupIcon;
  readonly LeafIcon = Leaf;
  readonly DropIcon = Droplets;
  readonly EmptyIcon = FolderOpen;

  private groupService = inject(GroupService);
  private plantService = inject(PlantService);

  private groups = toSignal(this.groupService.getGroups(), { initialValue: null });
  private plants = toSignal(this.plantService.getPlants(), { initialValue: null });

  loading = computed(() => this.groups() === null || this.plants() === null);

  // Join plants onto their group by name — the same key the rest of the app
  // already uses to relate plants and groups (PlantDto exposes groupName).
  groupsWithPlants = computed<GroupWithPlants[]>(() => {
    const groups = this.groups() ?? [];
    const plants = this.plants() ?? [];
    return groups.map(g => ({
      ...g,
      plants: plants.filter(p => p.groupName === g.name),
    }));
  });

  // Plants whose group name matches no known group (or none assigned).
  ungrouped = computed<Plant[]>(() => {
    const groups = this.groups() ?? [];
    const plants = this.plants() ?? [];
    const names = new Set(groups.map(g => g.name));
    return plants.filter(p => !p.groupName || !names.has(p.groupName));
  });

  isLow(plant: Plant): boolean {
    const { currentMoisture, moistureThreshold } = plant;
    return currentMoisture != null && moistureThreshold != null && currentMoisture < moistureThreshold;
  }
}
