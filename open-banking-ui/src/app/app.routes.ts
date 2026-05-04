import { Routes } from '@angular/router';
import { HomeComponent } from './components/home/home';
import { DashboardComponent } from './components/dashboard/dashboard';
import { RegisterComponent } from './components/register/register';
import { LoginComponent } from './components/login/login';
import { AccountDetailComponent } from './components/account-detail/account-detail';
import { AccountCreateComponent } from './components/account-create/account-create';

export const routes: Routes = [
  //default route
  { path: '', component: HomeComponent },

  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'dashboard', component: DashboardComponent },

  { path: 'accounts/new', component: AccountCreateComponent },

  { path: 'account/:accountNumber', component: AccountDetailComponent },
  
  // If they type a random URL, redirect them back to the home page
  { path: '**', redirectTo: '' } 
];
