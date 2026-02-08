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
import { featureGuard } from '../core/guards/feature-guard';
import { ProfileComponent } from '../features/profile/profile';
import { ManageServicesComponent } from '../features/dashboard/manage-services/manage-services';
import { ManageEmployeesComponent } from '../features/dashboard/manage-employees/manage-employees';
import { ManageReservationsComponent } from '../features/dashboard/manage-reservations/manage-reservations';
import { ReservationDetailComponent } from '../features/dashboard/reservation-detail/reservation-detail';
import { ChatComponent } from '../features/chat/chat';
import { ManageProfileComponent } from '../features/dashboard/manage-profile/manage-profile';
import { HomeComponent } from '../features/home/home';
import { EmployeeDetailComponent } from '../features/dashboard/employee-detail/employee-detail';
import { ServiceListComponent } from '../features/service-list/service-list';
import { ManageHrComponent } from '../features/dashboard/manage-hr/manage-hr';
import { ManageCustomersComponent } from '../features/dashboard/manage-customers/manage-customers';
import { LoyaltySettingsComponent } from '../features/dashboard/loyalty-settings/loyalty-settings';
import { DiscountManagerComponent } from '../features/dashboard/discount-manager/discount-manager/discount-manager';
import { FinanceDashboardComponent } from '../features/dashboard/finance/finance-dashboard/finance-dashboard';
import { InvoicesListComponent } from '../features/dashboard/invoices/invoices';
import { PaymentsListComponent } from '../features/dashboard/finance/payments-list/payments-list';
import { ServiceBundlesListComponent } from '../features/dashboard/service-bundles/service-bundles-list/service-bundles-list';
import { ServiceBundleWizardComponent } from '../features/dashboard/service-bundles/service-bundle-wizard/service-bundle-wizard';
import { AdminLayoutComponent } from '../layout/admin-layout/admin-layout';
import { superAdminGuard } from '../core/guards/super-admin-guard';
import { AdminDashboardComponent } from '../features/admin/admin-dashboard/admin-dashboard';
import { AdminPlansComponent } from '../features/admin/plans/plans';
import { AdminBusinessApprovalComponent } from '../features/admin/business-approval/business-approval';
import { SubscriptionManagerComponent } from '../features/dashboard/subscription/subscription';
import { UpgradeRequiredComponent } from '../features/dashboard/upgrade-required/upgrade-required';
import { TemplatesComponent } from '../features/dashboard/templates/templates';
import { ReviewsComponent } from '../features/dashboard/reviews/reviews';

export const routes: Routes = [
    { path: '', component: HomeComponent },
    { path: 'businesses', component: BusinessListComponent },
    { path: 'services', component: ServiceListComponent },
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
      { path: 'customers', component: ManageCustomersComponent },
      { path: 'reservations', component: ManageReservationsComponent },
      { path: 'reservations/:id', component: ReservationDetailComponent },
      { path: 'chat', component: ChatComponent },
      { path: 'profile', component: ManageProfileComponent },
      { path: 'employees/:id', component: EmployeeDetailComponent },
      { path: 'hr', component: ManageHrComponent },
      { path: 'reviews', component: ReviewsComponent },
      { path: 'loyalty', component: LoyaltySettingsComponent, canActivate: [featureGuard], data: { feature: 'marketing' } },
      { path: 'discounts', component: DiscountManagerComponent, canActivate: [featureGuard], data: { feature: 'marketing' } },
      { path: 'finance', component: FinanceDashboardComponent, canActivate: [featureGuard], data: { feature: 'reports' } },
      { path: 'invoices', component: InvoicesListComponent, canActivate: [featureGuard], data: { feature: 'reports' } },
      { path: 'payments', component: PaymentsListComponent },
      { path: 'bundles', component: ServiceBundlesListComponent },
      { path: 'bundles/create', component: ServiceBundleWizardComponent },
      { path: 'bundles/edit/:id', component: ServiceBundleWizardComponent },
      { path: 'subscription', component: SubscriptionManagerComponent },
      { path: 'upgrade-required', component: UpgradeRequiredComponent },
      { path: 'templates', component: TemplatesComponent },
      
    ]
  },
  {
    path: 'admin',
    component: AdminLayoutComponent,
    canActivate: [superAdminGuard],
    children: [
        { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
        { path: 'dashboard', component: AdminDashboardComponent },
        { path: 'plans', component: AdminPlansComponent },
        { path: 'businesses', component: AdminBusinessApprovalComponent },
    ]
  }
];