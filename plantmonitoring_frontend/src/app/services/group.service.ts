import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Group } from '../models/group.model';
import { environment } from '../../environments/environment';


@Injectable({ providedIn: 'root' })
export class GroupService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getGroups() {
    const url = `${this.apiUrl}/api/group`;
    return this.http.get<Group[]>(url);   
  }
}