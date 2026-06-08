export interface DashboardWidget {
  id: string | number;
  type: 'plant' | 'chart' | 'list' | 'spacer' | 'noplants'; 
  cols: number;
  rows: number;
  title?: string;
  data?: any;
}

export interface SensorHealth {
  id: number;
  name: string;
  macAddress: string;
  assignedPlant: string;
  lastContact: string | null;
}