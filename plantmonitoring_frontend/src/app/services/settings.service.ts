import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Setting } from '../models/setting.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class SettingsService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getSettings(): Observable<Setting[]> {
    const url = `${this.apiUrl}/api/settings`;
    console.log('Fetching all settings from API:', url);
    return this.http.get<Setting[]>(url);
  }

  // Bulk update — matches PUT /api/settings { settings: { key: value, ... } }
  updateSettings(settings: Record<string, string | null>): Observable<{ message: string; updated: number }> {
    const url = `${this.apiUrl}/api/settings`;
    console.log('Saving settings to API:', url, settings);
    return this.http.put<{ message: string; updated: number }>(url, { settings });
  }
}
