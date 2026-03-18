import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../core/services/auth-service';
import { ThemeService } from '../../core/services/theme-service';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-layout.html',
  styleUrls: []
})
export class AdminLayoutComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private router = inject(Router);
  private themeService = inject(ThemeService);
  
  ngOnInit() {
    this.themeService.forceTheme('purple_night');
  }

  ngOnDestroy() {
    this.themeService.forceTheme(null);
  }
  
  isMobileMenuOpen = false;
  isProfileMenuOpen = false;

  toggleMobileMenu() {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
  }

  closeMobileMenu() {
    this.isMobileMenuOpen = false;
  }

  toggleProfileMenu() {
    this.isProfileMenuOpen = !this.isProfileMenuOpen;
  }

  closeProfileMenu() {
    this.isProfileMenuOpen = false;
  }

  logout() {
    this.authService.logout();
    this.closeProfileMenu();
    this.closeMobileMenu();
  }
}