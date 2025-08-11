import { Routes } from '@angular/router';
import { BusinessListComponent  } from '../features/business-list/business-list';
import { RegisterComponent } from '../features/auth/register/register';
import { LoginComponent } from '../features/auth/login/login';
import { RegisterOwnerComponent } from '../features/auth/register-owner/register-owner';
import { BusinessDetailComponent } from '../features/business-detail/business-detail';
import { DashboardHomeComponent } from '../features/dashboard-home/dashboard-home';
import { OwnerLayoutComponent } from '../layout/owner-layout/owner-layout';
import { MyReservationsComponent } from '../features/my-reservation/my-reservation';
import { authGuard } from '../core/guards/auth-guard';
import { ownerGuard } from '../core/guards/owner-guard';
import { ProfileComponent } from '../features/profile/profile';
import { ManageServicesComponent } from '../features/dashboard/manage-services/manage-services';
import { ManageEmployeesComponent } from '../features/dashboard/manage-employees/manage-employees';
import { ManageReservationsComponent } from '../features/dashboard/manage-reservations/manage-reservations';
import { ReservationDetailComponent } from '../features/dashboard/reservation-detail/reservation-detail';
import { ChatComponent } from '../features/chat/chat';
import { ManageProfileComponent } from '../features/dashboard/manage-profile/manage-profile';
import { HomeComponent } from '../features/home/home';

export const routes: Routes = [
    { path: '', component: HomeComponent },
    { path: 'businesses', component: BusinessListComponent },
    { path: 'business/:id', component: BusinessDetailComponent },
    { path: 'register', component: RegisterComponent },
    { path: 'register-owner', component: RegisterOwnerComponent },
    { path: 'login', component: LoginComponent },
    { path: 'my-reservations', component: MyReservationsComponent, canActivate: [authGuard] },
    { path: 'profile', component: ProfileComponent, canActivate: [authGuard] },
    { path: 'chat', component: ChatComponent, canActivate: [authGuard] },
    { path: 'chat/:id', component: ChatComponent, canActivate: [authGuard] },
    { path: 'dashboard', 
    component: OwnerLayoutComponent,
    canActivate: [ownerGuard],
    children: [
      { path: '', component: DashboardHomeComponent }, 
      { path: 'services', component: ManageServicesComponent },
      { path: 'employees', component: ManageEmployeesComponent },
      { path: 'reservations', component: ManageReservationsComponent },
      { path: 'reservations/:id', component: ReservationDetailComponent },
      { path: 'chat', component: ChatComponent },
      { path: 'profile', component: ManageProfileComponent },
    ]
  },
];