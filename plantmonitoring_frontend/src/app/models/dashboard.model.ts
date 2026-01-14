export interface Plant {
  id: number;
  name: string;
  description: string;
  createdAt: string;
  groupName: string;
  currentMoisture: number;
  lastUpdate: string;
}

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
  macAddress: string;
  assignedPlant: string; 
  lastContact: string;  
}