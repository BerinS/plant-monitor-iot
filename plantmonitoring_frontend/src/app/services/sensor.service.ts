import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { SensorHealth } from '../models/dashboard.model';

@Injectable({ providedIn: 'root' })
export class SensorService {
  private http = inject(HttpClient);
  private apiUrl = 'http://192.168.1.13:5000'; // ip of the pc which runs the backend

  getSensorHealth() {
    const url = `${this.apiUrl}/api/sensor/health`;
    console.log("Fetching all sensor health data from API:", url);      
    return this.http.get<SensorHealth[]>(url);   
  }
}