export interface EventLogEntry {
  id: number;
  eventType: string;
  plantId: number | null;
  plantName: string | null;
  deviceId: number | null;
  triggeredBy: string;
  moistureAtTime: number | null;
  durationSeconds: number | null;
  notes: string | null;
  createdAt: string;
}

export type EventLogRange = 'recent' | 'day' | 'week' | 'all';
