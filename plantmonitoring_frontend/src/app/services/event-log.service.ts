import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { EventLogEntry, EventLogRange } from '../models/event-log.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class EventLogService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getEvents(range: EventLogRange = 'recent'): Observable<EventLogEntry[]> {
    const url = `${this.apiUrl}/api/eventlog`;
    const params = new HttpParams().set('range', range);
    console.log('Fetching event log from API:', url, range);
    return this.http.get<EventLogEntry[]>(url, { params });
  }
}
