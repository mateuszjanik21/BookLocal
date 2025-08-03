import { Routes } from '@angular/router';
import { BusinessListComponent  } from '../features/business-list/business-list';
import { RegisterComponent } from '../features/auth/register/register';
import { LoginComponent } from '../features/auth/login/login';
import { RegisterOwnerComponent } from '../features/auth/register-owner/register-owner';
import { BusinessDetailComponent } from '../features/business-detail/business-detail';
import { DashboardComponent } from '../features/dashboard/dashboard';
import { MyReservationsComponent } from '../features/my-reservation/my-reservation';
import { authGuard } from '../core/guards/auth-guard';
import { ownerGuard } from '../core/guards/owner-guard';
import { ProfileComponent } from '../features/profile/profile';

export const routes: Routes = [
    { path: 'businesses', component: BusinessListComponent },
    { path: 'business/:id', component: BusinessDetailComponent },
    { path: 'register', component: RegisterComponent },
    { path: 'register-owner', component: RegisterOwnerComponent },
    { path: 'login', component: LoginComponent },
    { path: 'dashboard', component: DashboardComponent, canActivate: [ownerGuard] },
    { path: 'my-reservations', component: MyReservationsComponent, canActivate: [authGuard] },
    { path: 'profile', component: ProfileComponent, canActivate: [authGuard] },
    { path: '', redirectTo: '/businesses', pathMatch: 'full' }
];