import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private themeSignal = signal<'booklocal_theme' | 'booklocal-dark' | 'purple_night'>('booklocal_theme');
  private userPreference: 'booklocal_theme' | 'booklocal-dark' = 'booklocal_theme';
  private forcedTheme: 'booklocal_theme' | 'booklocal-dark' | 'purple_night' | null = null;

  constructor() {
    this.initTheme();
  }

  get currentTheme() {
    return this.themeSignal();
  }

  get currentThemeSignal() {
    return this.themeSignal;
  }

  toggleTheme() {
    if (this.forcedTheme) return;
    const newTheme = this.themeSignal() === 'booklocal_theme' ? 'booklocal-dark' : 'booklocal_theme';
    this.setTheme(newTheme, true);
  }

  setTheme(theme: 'booklocal_theme' | 'booklocal-dark' | 'purple_night', saveToStorage: boolean = true) {
    this.themeSignal.set(theme);
    if (saveToStorage && theme !== 'purple_night') {
      this.userPreference = theme;
      localStorage.setItem('theme', theme);
    }
    document.documentElement.setAttribute('data-theme', theme);
  }

  forceTheme(theme: 'booklocal_theme' | 'booklocal-dark' | 'purple_night' | null) {
    this.forcedTheme = theme;
    if (theme) {
      this.setTheme(theme, false);
    } else {
      this.setTheme(this.userPreference, false);
    }
  }

  private initTheme() {
    const savedTheme = localStorage.getItem('theme') as 'booklocal_theme' | 'booklocal-dark' | null;
    if (savedTheme) {
      this.userPreference = savedTheme;
      this.setTheme(savedTheme, true);
    } else {
      this.setTheme('booklocal_theme', true);
    }
  }
}

