import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Plant } from '../models/dashboard.model';

@Injectable({ providedIn: 'root' })
export class PlantService {
  private http = inject(HttpClient);
  private apiUrl = 'http://192.168.1.13:5000/api/plants'; // ip of the pc which runs the backend

  getPlants() {
    console.log("Fetching plants from API:", this.apiUrl);      
    return this.http.get<Plant[]>(this.apiUrl);    
  }
}