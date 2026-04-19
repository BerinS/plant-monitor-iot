import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Plant } from '../models/plant.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class PlantService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getPlants() {
    const url = `${this.apiUrl}/api/plants`;
    console.log("Fetching all plants from API:", url);      
    return this.http.get<Plant[]>(url);    
  }

  getSensorHistory(plantId: number, hours: number = 24) {   
    const url = `${this.apiUrl}/api/sensor/${plantId}/history?hours=${hours}`;
     console.log("Fetching plant history:", url);   
    return this.http.get<any[]>(url);
  }

  deletePlant(plantId: number) {
    const url = `${this.apiUrl}/api/plants/${plantId}`;
    console.log("Deleting plant with ID:", plantId, "URL:", url);
    return this.http.delete(url);
  }

  updatePlant(plantId: number, updatedData: any) {
    const url = `${this.apiUrl}/api/plants/${plantId}`;
    console.log("Updating plant with ID:", plantId, "URL:", url);
    return this.http.put(url, updatedData); 
  }

  addPlant(payload: any) {
    const url = `${this.apiUrl}/api/plants`;
    return this.http.post<Plant>(url, payload);
  }
}