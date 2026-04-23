import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { SensorHealth } from '../models/dashboard.model';
import { environment } from '../../environments/environment';

import { Sensor } from '../models/sensor.model';
import { Observable } from 'rxjs';


@Injectable({ providedIn: 'root' })
export class SensorService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getSensorHealth() {
    const url = `${this.apiUrl}/api/sensor/health`;
    console.log("Fetching all sensor health data from API:", url);      
    return this.http.get<SensorHealth[]>(url);   
  }

  getAllSensors() {
    const url = `${this.apiUrl}/api/sensor`;
    console.log("Fetching all sensors from API:", url);      
    return this.http.get<Sensor[]>(url);
  }

  updateSensor(id: number, payload: any): Observable<Sensor> {
    const url = `${this.apiUrl}/api/sensor/${id}`;
    console.log(`Updating sensor ${id} at API:`, url);
    return this.http.put<Sensor>(url, payload);
  }

  deleteSensor(id: number) {
    const url = `${this.apiUrl}/api/sensor/${id}`;
    console.log(`Deleting sensor ${id} at API:`, url);
    return this.http.delete(url);
  }
  
  addSensor(payload: { name: string; macAddress: string | null; description: string }): Observable<Sensor> {
    const url = `${this.apiUrl}/api/sensor`;
    console.log("Adding new sensor to API:", url);
    return this.http.post<Sensor>(url, payload);
  }
}