import { Component } from '@angular/core';
import { 
  LucideAngularModule, 
  Search,
  Bell
} from 'lucide-angular';

@Component({
  selector: 'app-header',
  imports: [LucideAngularModule],
  template:`
  <header>
    <div class="search-wrapper">
      <lucide-icon [img]="SearchIcon" size="20" class="icon"></lucide-icon>
      <input type="search" placeholder="Search..." class="search-input">
    </div>
    <div class="notification-wrapper">
      <lucide-icon [img]="BellIcon" size="22" class="icon"></lucide-icon>
    </div>
  </header>
  `,
  styleUrl: './header.scss',
})
export class HeaderComponent {
  readonly SearchIcon = Search;
  readonly BellIcon = Bell;
}
