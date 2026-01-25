import { Routes } from '@angular/router';
import { MainLayoutComponent } from './main-layout.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { MyPlantsComponent } from './pages/my-plants/my-plants.component';

export const routes: Routes = [
  {
    path: '',
    component: MainLayoutComponent, 
    children: [
      {
        path: '', 
        redirectTo: 'dashboard', 
        pathMatch: 'full' 
      },
      {
        path: 'dashboard',
        component: DashboardComponent 
      },
      {
        path: 'my-plants',
        component: MyPlantsComponent
      }
      
    ]
  }
];