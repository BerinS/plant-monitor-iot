export interface Plant {
  id: number;
  name: string;
  description: string;
  createdAt: string;
  groupName: string;
  sensorName: string;
  moistureThreshold: number | null;
  currentMoisture: number;
  lastUpdate: string;
}