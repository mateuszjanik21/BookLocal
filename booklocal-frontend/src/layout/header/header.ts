import { Component, inject } from '@angular/core';
import { AuthService } from '../../core/services/auth-service';
import { PresenceService } from '../../core/services/presence-service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './header.html',
  styleUrl: './header.css'
})
export class HeaderComponent {
  authService = inject(AuthService);
  presenceService = inject(PresenceService);

  closeMobileMenu(): void {
    const activeElement = document.activeElement as HTMLElement;
    if (activeElement) {
      activeElement.blur();
    }
  }
}