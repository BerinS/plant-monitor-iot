export interface Notification {
  id: number;
  plantId: number | null;
  plantName: string | null;
  severity: 'info' | 'warning' | 'critical';
  title: string;
  message: string;
  isRead: boolean;
  sentEmail: boolean;
  createdAt: string;
}
