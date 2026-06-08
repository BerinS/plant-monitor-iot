import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Notification } from '../models/notification.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getNotifications(unreadOnly: boolean = false) {
    const url = `${this.apiUrl}/api/notifications${unreadOnly ? '?unreadOnly=true' : ''}`;
    console.log("Fetching notifications from API:", url);
    return this.http.get<Notification[]>(url);
  }

  markAsRead(id: number) {
    const url = `${this.apiUrl}/api/notifications/${id}/read`;
    console.log(`Marking notification ${id} as read at API:`, url);
    return this.http.patch<{ message: string }>(url, {});
  }
}
