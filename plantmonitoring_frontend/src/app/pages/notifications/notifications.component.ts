import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastrService } from 'ngx-toastr';
import {
  LucideAngularModule,
  Info,
  TriangleAlert,
  OctagonAlert,
  Check,
  CheckCheck,
  Inbox,
} from 'lucide-angular';

import { NotificationService } from '../../services/notification.service';
import { Notification } from '../../models/notification.model';

type Filter = 'all' | 'unread';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './notifications.component.html',
  styleUrl: './notifications.component.scss',
})
export class NotificationsComponent {
  private notificationService = inject(NotificationService);
  private toastr = inject(ToastrService);

  readonly InfoIcon = Info;
  readonly WarningIcon = TriangleAlert;
  readonly CriticalIcon = OctagonAlert;
  readonly CheckIcon = Check;
  readonly CheckAllIcon = CheckCheck;
  readonly InboxIcon = Inbox;

  notifications = signal<Notification[]>([]);
  loading = signal(true);
  error = signal(false);
  markingAll = signal(false);
  filter = signal<Filter>('all');

  unreadCount = computed(() => this.notifications().filter(n => !n.isRead).length);

  filtered = computed(() => {
    const all = this.notifications();
    return this.filter() === 'unread' ? all.filter(n => !n.isRead) : all;
  });

  constructor() {
    this.load();
  }

  private load() {
    this.loading.set(true);
    this.error.set(false);
    this.notificationService.getNotifications().subscribe({
      next: (data) => {
        this.notifications.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load notifications. Error:', err);
        this.error.set(true);
        this.loading.set(false);
      },
    });
  }

  setFilter(filter: Filter) {
    this.filter.set(filter);
  }

  severityIcon(severity: Notification['severity']) {
    if (severity === 'critical') return this.CriticalIcon;
    if (severity === 'warning') return this.WarningIcon;
    return this.InfoIcon;
  }

  markAsRead(notification: Notification) {
    if (notification.isRead) return;

    // Optimistic update so the row responds instantly.
    this.setRead(notification.id, true);

    this.notificationService.markAsRead(notification.id).subscribe({
      error: (err) => {
        console.error('Failed to mark notification as read. Error:', err);
        this.setRead(notification.id, false); // revert
        this.toastr.error('Could not mark notification as read.', 'Error');
      },
    });
  }

  markAllAsRead() {
    if (this.unreadCount() === 0 || this.markingAll()) return;

    this.markingAll.set(true);
    this.notificationService.markAllAsRead().subscribe({
      next: (res) => {
        this.markingAll.set(false);
        this.notifications.update(list => list.map(n => ({ ...n, isRead: true })));
        this.toastr.success(res.message || 'All notifications marked as read.', 'Success');
      },
      error: (err) => {
        console.error('Failed to mark all as read. Error:', err);
        this.markingAll.set(false);
        this.toastr.error('Could not mark all as read.', 'Error');
      },
    });
  }

  private setRead(id: number, isRead: boolean) {
    this.notifications.update(list =>
      list.map(n => (n.id === id ? { ...n, isRead } : n))
    );
  }

  timeSince(dateString: string) {
    const seconds = Math.floor((new Date().getTime() - new Date(dateString).getTime()) / 1000);
    let interval = seconds / 31536000;
    if (interval > 1) return Math.floor(interval) + ' years ago';
    interval = seconds / 2592000;
    if (interval > 1) return Math.floor(interval) + ' months ago';
    interval = seconds / 86400;
    if (interval > 1) return Math.floor(interval) + ' days ago';
    interval = seconds / 3600;
    if (interval > 1) return Math.floor(interval) + ' hours ago';
    interval = seconds / 60;
    if (interval > 1) return Math.floor(interval) + ' mins ago';
    return Math.floor(seconds) + ' seconds ago';
  }
}
