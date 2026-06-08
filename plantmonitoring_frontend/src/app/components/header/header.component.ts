import { Component, inject, computed } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import {
  LucideAngularModule,
  Search,
  Bell
} from 'lucide-angular';
import { NotificationService } from '../../services/notification.service';

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
      @if (unreadCount() > 0) {
        <span class="unread-badge">{{ unreadCount() > 9 ? '9+' : unreadCount() }}</span>
      }
    </div>
  </header>
  `,
  styleUrl: './header.scss',
})
export class HeaderComponent {
  readonly SearchIcon = Search;
  readonly BellIcon = Bell;

  private notificationService = inject(NotificationService);

  private unreadNotifications = toSignal(this.notificationService.getNotifications(true), { initialValue: [] });
  unreadCount = computed(() => this.unreadNotifications().length);
}
