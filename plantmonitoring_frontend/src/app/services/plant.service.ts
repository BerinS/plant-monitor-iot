import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Plant } from '../models/dashboard.model';
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
}