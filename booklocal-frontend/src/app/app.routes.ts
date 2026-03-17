import { importProvidersFrom } from '@angular/core';
import { Routes } from '@angular/router';
import { Component } from '@angular/core';
import { RegisterComponent } from '../features/auth/register/register';
import { LoginComponent } from '../features/auth/login/login';
import { RegisterOwnerComponent } from '../features/auth/register-owner/register-owner';
import { BusinessDetailComponent } from '../features/business-detail/business-detail';
import { DashboardHomeComponent } from '../features/dashboard-home/dashboard-home';
import { OwnerLayoutComponent } from '../layout/owner-layout/owner-layout';
import { MyReservationsComponent } from '../features/my-reservation/my-reservation';
import { authGuard } from '../core/guards/auth-guard';
import { ownerGuard } from '../core/guards/owner-guard';
import { featureGuard } from '../core/guards/feature-guard';
import { ProfileComponent } from '../features/profile/profile';
import { ReservationDetailComponent } from '../features/dashboard/reservation-detail/reservation-detail';
import { ChatComponent } from '../features/chat/chat';
import { HomeComponent } from '../features/home/home';
import { AdminLayoutComponent } from '../layout/admin-layout/admin-layout';
import { superAdminGuard } from '../core/guards/super-admin-guard';
import { UpgradeRequiredComponent } from '../features/dashboard/upgrade-required/upgrade-required';

console.log('App Routes Loaded');

export const routes: Routes = [
    { path: '', component: HomeComponent },
    { path: 'about', loadComponent: () => import('../features/about/about').then(m => m.AboutComponent) },
    { path: 'businesses', loadComponent: () => import('../features/business-list/business-list').then(m => m.BusinessListComponent) },
    { path: 'services', loadComponent: () => import('../features/service-list/service-list').then(m => m.ServiceListComponent) },
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
      { path: 'services', loadComponent: () => import('../features/dashboard/manage-services/manage-services').then(m => m.ManageServicesComponent) },
      { path: 'employees', loadComponent: () => import('../features/dashboard/manage-employees/manage-employees').then(m => m.ManageEmployeesComponent) },
      { path: 'customers', loadComponent: () => import('../features/dashboard/manage-customers/manage-customers').then(m => m.ManageCustomersComponent) },
      { 
        path: 'reservations', 
        loadComponent: () => import('../features/dashboard/manage-reservations/manage-reservations').then(m => m.ManageReservationsComponent)
      },
      { path: 'reservations/:id', component: ReservationDetailComponent },
      { path: 'chat', component: ChatComponent },
      { path: 'profile', loadComponent: () => import('../features/dashboard/manage-profile/manage-profile').then(m => m.ManageProfileComponent) },
      { path: 'employees/:id', loadComponent: () => import('../features/dashboard/employee-detail/employee-detail').then(m => m.EmployeeDetailComponent) },
      { path: 'hr', loadComponent: () => import('../features/dashboard/manage-hr/manage-hr').then(m => m.ManageHrComponent) },
      { path: 'templates', loadComponent: () => import('../features/dashboard/templates/templates').then(m => m.TemplatesComponent) },
      { path: 'reviews', loadComponent: () => import('../features/dashboard/reviews/reviews').then(m => m.ReviewsComponent) },
      
      { path: 'loyalty', loadComponent: () => import('../features/dashboard/loyalty-settings/loyalty-settings').then(m => m.LoyaltySettingsComponent), canActivate: [featureGuard], data: { feature: 'marketing' } },
      { path: 'discounts', loadComponent: () => import('../features/dashboard/discount-manager/discount-manager/discount-manager').then(m => m.DiscountManagerComponent), canActivate: [featureGuard], data: { feature: 'marketing' } },
      
      { path: 'finance', loadComponent: () => import('../features/dashboard/finance/finance-dashboard/finance-dashboard').then(m => m.FinanceDashboardComponent), canActivate: [featureGuard], data: { feature: 'reports' } },
      { path: 'invoices', loadComponent: () => import('../features/dashboard/invoices/invoices').then(m => m.InvoicesListComponent), canActivate: [featureGuard], data: { feature: 'reports' } },
      { path: 'payments', loadComponent: () => import('../features/dashboard/finance/payments-list/payments-list').then(m => m.PaymentsListComponent) },
      
      { path: 'bundles', loadComponent: () => import('../features/dashboard/service-bundles/service-bundles-list/service-bundles-list').then(m => m.ServiceBundlesListComponent) },
      { path: 'bundles/create', loadComponent: () => import('../features/dashboard/service-bundles/service-bundle-wizard/service-bundle-wizard').then(m => m.ServiceBundleWizardComponent) },
      { path: 'bundles/edit/:id', loadComponent: () => import('../features/dashboard/service-bundles/service-bundle-wizard/service-bundle-wizard').then(m => m.ServiceBundleWizardComponent) },
      { path: 'subscription', loadComponent: () => import('../features/dashboard/subscription/subscription').then(m => m.SubscriptionManagerComponent) },
      { path: 'upgrade-required', component: UpgradeRequiredComponent }
    ]
  },
  {
    path: 'admin',
    component: AdminLayoutComponent,
    canActivate: [superAdminGuard],
    children: [
        { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
        { path: 'dashboard', loadComponent: () => import('../features/admin/admin-dashboard/admin-dashboard').then(m => m.AdminDashboardComponent) },
        { path: 'plans', loadComponent: () => import('../features/admin/plans/plans').then(m => m.AdminPlansComponent) },
        { path: 'businesses', loadComponent: () => import('../features/admin/business-approval/business-approval').then(m => m.AdminBusinessApprovalComponent) },
    ]
  }
];