import { Routes } from '@angular/router';
import { DashboardComponent } from './components/dashboard/dashboard';
import { LoginComponent } from './components/login/login';

export const routes: Routes = [
  //default route
  { path: '', component: LoginComponent },

  // When the user visits the root URL (localhost:4200), load the Dashboard
  { path: 'dashboard', component: DashboardComponent },
  
  // If they type a random URL, redirect them back to the home page
  { path: '**', redirectTo: '' } 
];
