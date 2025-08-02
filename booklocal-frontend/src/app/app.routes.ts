import { Routes } from '@angular/router';
import { BusinessListComponent  } from '../features/business-list/business-list';
import { RegisterComponent } from '../features/auth/register/register';
import { LoginComponent } from '../features/auth/login/login';
import { RegisterOwnerComponent } from '../features/auth/register-owner/register-owner';

export const routes: Routes = [
    { path: 'businesses', component: BusinessListComponent },
    { path: 'register', component: RegisterComponent },
    { path: 'register-owner', component: RegisterOwnerComponent },
    { path: 'login', component: LoginComponent },
    { path: '', redirectTo: '/businesses', pathMatch: 'full' }
];