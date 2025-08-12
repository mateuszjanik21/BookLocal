import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { EmployeeService } from '../../../core/services/employee-service';
import { EmployeeDetail } from '../../../types/employee.models';
import { BusinessService } from '../../../core/services/business-service';
import { finalize, switchMap } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-employee-detail',
  standalone: true,
  imports: [CommonModule, CurrencyPipe ],
  templateUrl: './employee-detail.html',
})
export class EmployeeDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private employeeService = inject(EmployeeService);
  private businessService = inject(BusinessService);
  private toastr = inject(ToastrService);
  
  isLoading = true;
  employee: EmployeeDetail | null = null;
  businessId: number | null = null;
  
  dayOfWeekTranslations: { [key: string]: string } = {
  'Sunday': 'Niedziela',
  'Monday': 'Poniedziałek',
  'Tuesday': 'Wtorek',
  'Wednesday': 'Środa',
  'Thursday': 'Czwartek',
  'Friday': 'Piątek',
  'Saturday': 'Sobota'
};

  ngOnInit(): void {
    const employeeId = +this.route.snapshot.paramMap.get('id')!;

    this.businessService.getMyBusiness().pipe(
      switchMap(business => {
        if (!business) {
          throw new Error('Nie znaleziono firmy.');
        }
        this.businessId = business.id;
        return this.employeeService.getEmployeeDetails(this.businessId, employeeId);
      }),
      finalize(() => this.isLoading = false)
    ).subscribe({
  next: (data) => {
    const sortOrder: { [key: string]: number } = {
      'Monday': 1,
      'Tuesday': 2,
      'Wednesday': 3,
      'Thursday': 4,
      'Friday': 5,
      'Saturday': 6,
      'Sunday': 7
    };
    data.workSchedules.sort((a, b) => sortOrder[a.dayOfWeek] - sortOrder[b.dayOfWeek]);

    this.employee = data;
  },
  error: () => this.toastr.error('Nie udało się załadować szczegółów pracownika.')
});
  }
}