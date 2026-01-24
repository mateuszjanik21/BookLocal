import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavigationEnd, Router, RouterModule } from '@angular/router';
import { filter } from 'rxjs';
import { HeaderComponent } from '../layout/header/header';
import { AuthService } from '../core/services/auth-service';
import { PresenceService } from '../core/services/presence-service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterModule, HeaderComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  router = inject(Router);
  authService = inject(AuthService);
  presenceService = inject(PresenceService);
  toastr = inject(ToastrService);
  showMainHeader = true;

  constructor() {
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      this.showMainHeader = !event.url.startsWith('/dashboard') && !event.url.startsWith('/admin');
    });

    this.authService.currentUser$.subscribe(user => {
      if (user) {
        this.presenceService.createHubConnection();
      } else {
        this.presenceService.stopHubConnection();
      }
    });
  }
}