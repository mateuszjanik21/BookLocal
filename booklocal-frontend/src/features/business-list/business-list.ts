import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BusinessService } from '../../core/services/business-service';
import { Business } from '../../types/business.model';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-business-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './business-list.html',
  styleUrl: './business-list.css'
})
export class BusinessListComponent implements OnInit {
  private businessService = inject(BusinessService);
  businesses: Business[] = [];

  ngOnInit(): void {
    this.businessService.getBusinesses().subscribe({
      next: (data) => {
        this.businesses = data;
      },
      error: (err: any) => {
        console.error('Błąd podczas pobierania firm:', err);
      }
    });
  }
}