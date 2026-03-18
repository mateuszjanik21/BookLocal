<<<<<<< HEAD
import { Component, inject, OnInit, OnDestroy } from '@angular/core';
=======
import { Component, inject, OnInit } from '@angular/core';
>>>>>>> 4c72ec2fbd0529d607afe301fd1930bdf361b105
import { AuthService } from '../../core/services/auth-service';
import { PresenceService } from '../../core/services/presence-service';
import { ThemeService } from '../../core/services/theme-service';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './header.html',
  styleUrl: './header.css'
})
<<<<<<< HEAD
export class HeaderComponent implements OnInit, OnDestroy {
=======
export class HeaderComponent implements OnInit {
>>>>>>> 4c72ec2fbd0529d607afe301fd1930bdf361b105
  authService = inject(AuthService);
  presenceService = inject(PresenceService);
  themeService = inject(ThemeService);
  router = inject(Router);

  isAuthPage = false;

  ngOnInit() {
    this.checkAuthPage(this.router.url);
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      this.checkAuthPage(event.url);
    });
  }

  private checkAuthPage(url: string) {
    this.isAuthPage = url.includes('/login') || url.includes('/register') || url.includes('/register-owner');
    
    if (this.isAuthPage) {
      this.themeService.forceTheme('booklocal_theme');
    } else {
      this.themeService.forceTheme(null);
    }
  }
<<<<<<< HEAD

  ngOnDestroy(): void {
    this.themeService.forceTheme(null);
  }
=======
>>>>>>> 4c72ec2fbd0529d607afe301fd1930bdf361b105

  closeMobileMenu(): void {
    const activeElement = document.activeElement as HTMLElement;
    if (activeElement) {
      activeElement.blur();
    }
  }
}