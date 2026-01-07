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
  type: 'plant' | 'chart' | 'list'; 
  cols: number;
  rows: number;
  title?: string;
  data?: any;
}