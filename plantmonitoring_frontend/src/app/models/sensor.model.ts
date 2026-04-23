export interface Sensor {
  id: number;
  name: string | null;
  macAddress: string | null;
  apiToken: string; 
  currentPlantId: number | null;
  groupId: number | null;
  description: string | null;
}