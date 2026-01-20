import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { SensorHealth } from '../models/dashboard.model';
import { environment } from '../../environments/environment';


@Injectable({ providedIn: 'root' })
export class SensorService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getSensorHealth() {
    const url = `${this.apiUrl}/api/sensor/health`;
    console.log("Fetching all sensor health data from API:", url);      
    return this.http.get<SensorHealth[]>(url);   
  }
}