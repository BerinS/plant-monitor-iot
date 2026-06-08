import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { ToastrService } from 'ngx-toastr';
import { LucideAngularModule, Info, TriangleAlert, OctagonAlert, Check, ArrowRight, BellOff } from 'lucide-angular';

import { NotificationService } from '../../services/notification.service';
import { Notification } from '../../models/notification.model';

const MAX_VISIBLE = 8;

@Component({
  selector: 'app-recent-alerts',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  template: `
    <div class="alerts-container">

      <div class="alerts-header">
        <p>{{ unreadCount() }} unread</p>
        <button class="view-all" (click)="goToNotifications()">
          View all <lucide-icon [img]="ArrowRightIcon" size="14"></lucide-icon>
        </button>
      </div>

      @if (displayNotifications().length > 0) {
        <div class="alerts-list">
          @for (notification of displayNotifications(); track notification.id) {
            <div class="alert-row" [class.read]="notification.isRead" [class]="'severity-' + notification.severity">
              <div class="alert-top">
                <div class="alert-heading">
                  <lucide-icon [img]="severityIcon(notification.severity)" size="16" class="severity-icon"></lucide-icon>
                  <span class="alert-title">{{ notification.title }}</span>
                </div>
                <span class="alert-time">{{ timeSince(notification.createdAt) }}</span>
              </div>

              <p class="alert-message">{{ notification.message }}</p>

              <div class="alert-bottom">
                <span class="alert-plant">{{ notification.plantName ?? 'System' }}</span>

                @if (!notification.isRead) {
                  <button class="mark-read" (click)="markAsRead(notification)">
                    <lucide-icon [img]="CheckIcon" size="13"></lucide-icon>
                    Mark as read
                  </button>
                } @else {
                  <span class="read-label">
                    <lucide-icon [img]="CheckIcon" size="13"></lucide-icon>
                    Read
                  </span>
                }
              </div>
            </div>
          }
        </div>
      } @else {
        <div class="empty-alerts">
          <lucide-icon [img]="BellOffIcon" size="26"></lucide-icon>
          <small>No alerts to show</small>
        </div>
      }
    </div>
  `,
  styleUrls: ['./recent-alerts.component.scss']
})
export class RecentAlertsComponent {
  private notificationService = inject(NotificationService);
  private toastr = inject(ToastrService);
  private router = inject(Router);

  readonly InfoIcon = Info;
  readonly WarningIcon = TriangleAlert;
  readonly CriticalIcon = OctagonAlert;
  readonly CheckIcon = Check;
  readonly ArrowRightIcon = ArrowRight;
  readonly BellOffIcon = BellOff;

  notifications = toSignal(this.notificationService.getNotifications(), { initialValue: [] as Notification[] });

  // ids marked as read locally (optimistic), so the UI updates instantly without a refetch
  private locallyRead = signal<Set<number>>(new Set());

  displayNotifications = computed(() => {
    const read = this.locallyRead();
    return this.notifications()
      .slice(0, MAX_VISIBLE)
      .map(n => (read.has(n.id) ? { ...n, isRead: true } : n));
  });

  unreadCount = computed(() => this.displayNotifications().filter(n => !n.isRead).length);

  severityIcon(severity: Notification['severity']) {
    if (severity === 'critical') return this.CriticalIcon;
    if (severity === 'warning') return this.WarningIcon;
    return this.InfoIcon;
  }

  markAsRead(notification: Notification) {
    if (notification.isRead || this.locallyRead().has(notification.id)) return;

    this.locallyRead.update(ids => new Set(ids).add(notification.id));

    this.notificationService.markAsRead(notification.id).subscribe({
      error: (err) => {
        console.error('Failed to mark notification as read. Error:', err);

        this.locallyRead.update(ids => {
          const updated = new Set(ids);
          updated.delete(notification.id);
          return updated;
        });

        this.toastr.error('Could not mark notification as read.', 'Error');
      }
    });
  }

  goToNotifications() {
    this.router.navigate(['/notifications']);
  }

  timeSince(dateString: string) {
    const seconds = Math.floor((new Date().getTime() - new Date(dateString).getTime()) / 1000);
    let interval = seconds / 31536000;
    if (interval > 1) return Math.floor(interval) + " years ago";
    interval = seconds / 2592000;
    if (interval > 1) return Math.floor(interval) + " months ago";
    interval = seconds / 86400;
    if (interval > 1) return Math.floor(interval) + " days ago";
    interval = seconds / 3600;
    if (interval > 1) return Math.floor(interval) + " hours ago";
    interval = seconds / 60;
    if (interval > 1) return Math.floor(interval) + " mins ago";
    return Math.floor(seconds) + " seconds ago";
  }
}
