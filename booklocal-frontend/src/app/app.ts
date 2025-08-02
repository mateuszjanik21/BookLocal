import { Component, signal } from '@angular/core';
import { RouterModule } from '@angular/router';
import { HeaderComponent } from '../layout/header/header';

@Component({
  selector: 'app-root',
  imports: [RouterModule, HeaderComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('booklocal-frontend');
}
