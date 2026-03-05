import { Routes } from '@angular/router';
import { DashboardComponent } from './components/dashboard/dashboard';

export const routes: Routes = [
  // When the user visits the root URL (localhost:4200), load the Dashboard
  { path: '', component: DashboardComponent }, 
  
  // If they type a random URL, redirect them back to the home page
  { path: '**', redirectTo: '' } 
];
