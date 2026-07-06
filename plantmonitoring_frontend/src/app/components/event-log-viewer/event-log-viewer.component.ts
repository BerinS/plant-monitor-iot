import { Component, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { LucideAngularModule, ScrollText } from 'lucide-angular';

import { EventLogService } from '../../services/event-log.service';
import { EventLogEntry, EventLogRange } from '../../models/event-log.model';

interface RangeOption {
  value: EventLogRange;
  label: string;
}

@Component({
  selector: 'app-event-log-viewer',
  imports: [DatePipe, LucideAngularModule],
  templateUrl: './event-log-viewer.component.html',
  styleUrl: './event-log-viewer.component.scss',
})
export class EventLogViewerComponent {
  readonly LogIcon = ScrollText;

  private eventLogService = inject(EventLogService);

  // Column headers mirror the event_log table field names (snake_case), as
  // requested; each maps to the camelCase model property for value lookup.
  readonly columnMap: Record<string, keyof EventLogEntry> = {
    id: 'id',
    event_type: 'eventType',
    plant_id: 'plantId',
    plant_name: 'plantName',
    device_id: 'deviceId',
    triggered_by: 'triggeredBy',
    moisture_at_time: 'moistureAtTime',
    duration_seconds: 'durationSeconds',
    notes: 'notes',
    created_at: 'createdAt',
  };

  readonly headers = Object.keys(this.columnMap);

  readonly ranges: RangeOption[] = [
    { value: 'recent', label: 'Last 10' },
    { value: 'day', label: 'Last day' },
    { value: 'week', label: 'Last week' },
    { value: 'all', label: 'All' },
  ];

  range = signal<EventLogRange>('recent');
  entries = signal<EventLogEntry[]>([]);
  loading = signal(true);
  error = signal(false);

  isDate = (header: string) => header === 'created_at';

  constructor() {
    this.load();
  }

  setRange(range: EventLogRange) {
    if (this.range() === range) return;
    this.range.set(range);
    this.load();
  }

  private load() {
    this.loading.set(true);
    this.error.set(false);
    this.eventLogService.getEvents(this.range()).subscribe({
      next: (data) => {
        this.entries.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load event log. Error:', err);
        this.error.set(true);
        this.loading.set(false);
      },
    });
  }

  // Reads a table cell value via the snake_case header, returning '' for nulls.
  cell(entry: EventLogEntry, header: string): string | number {
    const prop = this.columnMap[header];
    const value = entry[prop];
    return value === null || value === undefined ? '' : value;
  }
}
