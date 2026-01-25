import { Component, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { 
  LucideAngularModule, 
  LayoutDashboard, 
  Leaf, 
  Settings, 
  X,
  SatelliteDish
} from 'lucide-angular';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [
    RouterLink, 
    RouterLinkActive,
    LucideAngularModule
  ],
  styleUrl: './sidebar.component.scss', 
  template: `
    <nav>
      <div class="brand">Plant Monitor</div>
      
      <a routerLink="/dashboard" routerLinkActive="active">
        <lucide-icon [img]="DashboardIcon" size="21"></lucide-icon>
        <span>Dashboard</span>
      </a>

      <a routerLink="/my-plants" routerLinkActive="active">
        <lucide-icon [img]="PlantIcon" size="21"></lucide-icon>
        <span>My Plants</span>
      </a>

      <a routerLink="/sensors" routerLinkActive="active">
        <lucide-icon [img]="SatelliteDish" size="21"></lucide-icon>
        <span>Sensors</span>
      </a>

      <a routerLink="/settings" routerLinkActive="active">
        <lucide-icon [img]="SettingsIcon" size="21"></lucide-icon>
        <span>Settings</span>
      </a>

      <button class="close-btn" (click)="close.emit()">
        <lucide-icon [img]="CloseIcon" size="20"></lucide-icon>
      </button>
    </nav>
  `
})
export class SidebarComponent {
  close = output(); 

  readonly DashboardIcon = LayoutDashboard;
  readonly PlantIcon = Leaf;
  readonly SettingsIcon = Settings;
  readonly CloseIcon = X;
  readonly SatelliteDish = SatelliteDish;
}