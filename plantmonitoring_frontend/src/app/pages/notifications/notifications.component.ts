import { Component } from '@angular/core';
import { LucideAngularModule, Bell } from 'lucide-angular';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [LucideAngularModule],
  template: `
    <div class="placeholder">
      <lucide-icon [img]="BellIcon" size="32"></lucide-icon>
      <h2>Notifications</h2>
      <p>This page is coming soon.</p>
    </div>
  `,
  styles: [`
    .placeholder {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 0.5rem;
      height: 100%;
      min-height: 300px;
      color: #888;
      text-align: center;
    }

    h2 {
      margin: 0;
      color: #444;
    }

    p {
      margin: 0;
      font-size: 0.9rem;
    }
  `]
})
export class NotificationsComponent {
  readonly BellIcon = Bell;
}
