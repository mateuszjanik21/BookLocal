import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { SubscriptionService } from '../services/subscription-service';
import { map, take, tap } from 'rxjs/operators';

export const featureGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const subService = inject(SubscriptionService);
  const requiredFeature = route.data['feature'] as string;

  return subService.getCurrentSubscription().pipe(
    take(1),
    map(sub => {
        if (!sub || !sub.isActive) {
             return router.createUrlTree(['/dashboard/subscription']);
        }

        let hasAccess = false;
        
        if (requiredFeature === 'reports') {
            hasAccess = !!sub.hasAdvancedReports;
        }
        
        if (requiredFeature === 'marketing') {
            hasAccess = !!sub.hasMarketingTools;
        }

        if (hasAccess) {
            return true;
        } else {
            return router.createUrlTree(['/dashboard/upgrade-required']);
        }
    })
  );
};
