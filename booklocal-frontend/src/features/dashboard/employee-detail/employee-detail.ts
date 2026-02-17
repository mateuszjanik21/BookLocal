import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { EmployeeService } from '../../../core/services/employee-service';
import { EmployeeDetail, EmployeeReservationDto } from '../../../types/employee.models';
import { BusinessService } from '../../../core/services/business-service';
import { ServiceCategory, Employee } from '../../../types/business.model';
import { finalize, switchMap } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { EditEmployeeModalComponent } from '../../../shared/components/edit-employee-modal/edit-employee-modal';
import { ScheduleModalComponent } from '../../../shared/components/schedule-modal/schedule-modal';
import { AssignServicesModalComponent } from '../../../shared/components/assign-services-modal/assign-services-modal';
import { EmployeePhotoModalComponent } from '../../../shared/components/employee-photo-modal/employee-photo-modal';

@Component({
  selector: 'app-employee-detail',
  standalone: true,
  imports: [
    CommonModule, CurrencyPipe, RouterModule, FormsModule, ReactiveFormsModule,
    EditEmployeeModalComponent, ScheduleModalComponent,
    AssignServicesModalComponent, EmployeePhotoModalComponent
  ],
  templateUrl: './employee-detail.html',
  styleUrls: ['./employee-detail.css']
})
export class EmployeeDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private employeeService = inject(EmployeeService);
  private businessService = inject(BusinessService);
  private toastr = inject(ToastrService);
  private fb = inject(FormBuilder);

  isLoading = true;
  employee: EmployeeDetail | null = null;
  businessId: number | null = null;
  categories: ServiceCategory[] = [];
  activeTab: string = 'schedule';

  isEditModalVisible = false;
  isScheduleModalVisible = false;
  isAssignServicesModalVisible = false;
  
  employeeForPhoto: Employee | null = null;

  showCertForm = false;
  newCert = { name: '', institution: '', dateObtained: '', isVisibleToClient: true };
  today = new Date().toISOString().split('T')[0];

  showAbsenceForm = false;
  newAbsence = { dateFrom: '', dateTo: '', type: 'Vacation', reason: '', blocksCalendar: true };

  tabs = [
    { id: 'schedule', label: 'Grafik', shortLabel: 'Grafik', icon: 'M6.75 3v2.25M17.25 3v2.25M3 18.75V7.5a2.25 2.25 0 012.25-2.25h13.5A2.25 2.25 0 0121 7.5v11.25m-18 0A2.25 2.25 0 005.25 21h13.5A2.25 2.25 0 0021 18.75m-18 0v-7.5A2.25 2.25 0 015.25 9h13.5A2.25 2.25 0 0121 11.25v7.5' },
    { id: 'services', label: 'Usługi', shortLabel: 'Usługi', icon: 'M9.53 16.122a3 3 0 00-5.78 1.128 2.25 2.25 0 01-2.4 2.245 4.5 4.5 0 008.4-2.245c0-.399-.078-.78-.22-1.128zm0 0a15.998 15.998 0 003.388-1.62m-5.043-.025a15.994 15.994 0 011.622-3.395m3.42 3.42a15.995 15.995 0 004.764-4.648l3.876-5.814a1.151 1.151 0 00-1.597-1.597L14.146 6.32a15.996 15.996 0 00-4.649 4.763m3.42 3.42a6.776 6.776 0 00-3.42-3.42' },
    { id: 'finance', label: 'Finanse', shortLabel: 'Finanse', icon: 'M2.25 18.75a60.07 60.07 0 0115.797 2.101c.727.198 1.453-.342 1.453-1.096V18.75M3.75 4.5v.75A.75.75 0 013 6h-.75m0 0v-.375c0-.621.504-1.125 1.125-1.125H20.25M2.25 6v9m18-10.5v.75c0 .414.336.75.75.75h.75m-1.5-1.5h.375c.621 0 1.125.504 1.125 1.125v9.75c0 .621-.504 1.125-1.125 1.125h-.375m1.5-1.5H21a.75.75 0 00-.75.75v.75m0 0H3.75m0 0h-.375a1.125 1.125 0 01-1.125-1.125V15m1.5 1.5v-.75A.75.75 0 003 15h-.75M15 10.5a3 3 0 11-6 0 3 3 0 016 0zm3 0h.008v.008H18V10.5zm-12 0h.008v.008H6V10.5z' },
    { id: 'contracts', label: 'Umowy', shortLabel: 'Umowy', icon: 'M19.5 14.25v-2.625a3.375 3.375 0 00-3.375-3.375h-1.5A1.125 1.125 0 0113.5 7.125v-1.5a3.375 3.375 0 00-3.375-3.375H8.25m2.25 0H5.625c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125V11.25a9 9 0 00-9-9z' },
    { id: 'certificates', label: 'Certyfikaty', shortLabel: 'Cert.', icon: 'M4.26 10.147a60.436 60.436 0 00-.491 6.347A48.627 48.627 0 0112 20.904a48.627 48.627 0 018.232-4.41 60.46 60.46 0 00-.491-6.347m-15.482 0a50.57 50.57 0 00-2.658-.813A59.905 59.905 0 0112 3.493a59.902 59.902 0 0110.399 5.84c-.896.248-1.783.52-2.658.814m-15.482 0A50.697 50.697 0 0112 13.489a50.702 50.702 0 017.74-3.342M6.75 15a.75.75 0 100-1.5.75.75 0 000 1.5zm0 0v-3.675A55.378 55.378 0 0112 8.443m-7.007 11.55A5.981 5.981 0 006.75 15.75v-1.5' },
    { id: 'absences', label: 'Nieobecności', shortLabel: 'Urlopy', icon: 'M22 10.5h-6m-2.25-4.125a3.375 3.375 0 11-6.75 0 3.375 3.375 0 016.75 0zM4 19.235v-.11a6.375 6.375 0 0112.75 0v.109A12.318 12.318 0 0110.374 21c-2.331 0-4.512-.645-6.374-1.766z' },
  ];

  dayOfWeekTranslations: { [key: string]: string } = {
    'Sunday': 'Niedziela', 'Monday': 'Poniedziałek', 'Tuesday': 'Wtorek',
    'Wednesday': 'Środa', 'Thursday': 'Czwartek', 'Friday': 'Piątek', 'Saturday': 'Sobota'
  };

  dayOfWeekShort: { [key: string]: string } = {
    'Sunday': 'Ndz', 'Monday': 'Pon', 'Tuesday': 'Wt',
    'Wednesday': 'Śr', 'Thursday': 'Czw', 'Friday': 'Pt', 'Saturday': 'Sob'
  };

  contractTypeTranslations: { [key: string]: string } = {
    'EmploymentContract': 'Umowa o pracę', 'B2B': 'B2B',
    'MandateContract': 'Umowa zlecenie', 'Apprenticeship': 'Staż / Praktyka'
  };

  absenceTypeTranslations: { [key: string]: string } = {
    'Vacation': 'Urlop wypoczynkowy', 'SickLeave': 'Zwolnienie lekarskie',
    'UnpaidLeave': 'Urlop bezpłatny', 'Training': 'Szkolenie', 'Other': 'Inne'
  };

  payrollStatusTranslations: { [key: string]: string } = {
    'Draft': 'Szkic', 'Calculated': 'Obliczony', 'Approved': 'Zatwierdzony', 'Paid': 'Wypłacony'
  };

  monthNames: { [key: number]: string } = {
    1: 'Styczeń', 2: 'Luty', 3: 'Marzec', 4: 'Kwiecień',
    5: 'Maj', 6: 'Czerwiec', 7: 'Lipiec', 8: 'Sierpień',
    9: 'Wrzesień', 10: 'Październik', 11: 'Listopad', 12: 'Grudzień'
  };

  showFinanceSettingsModal = false;
  financeSubTab: 'config' | 'history' = 'config';
  financeForm = this.fb.group({
    commissionPercentage: [0],
    hourlyRate: [0],
    hasPit2Filed: [true],
    isPensionRetired: [false],
    voluntarySicknessInsurance: [true],
    participatesInPPK: [false],
    ppkEmployeeRate: [2.0],
    ppkEmployerRate: [1.5],
    commuteType: [0]
  });

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    const employeeId = +this.route.snapshot.paramMap.get('id')!;
    this.isLoading = true;

    this.businessService.getMyBusiness().pipe(
      switchMap(business => {
        if (!business) throw new Error('Nie znaleziono firmy.');
        this.businessId = business.id;
        this.categories = business.categories || [];
        return this.employeeService.getEmployeeDetails(this.businessId, employeeId);
      }),
      finalize(() => this.isLoading = false)
    ).subscribe({
      next: (data) => {
        const sortOrder: { [key: string]: number } = {
          'Monday': 1, 'Tuesday': 2, 'Wednesday': 3, 'Thursday': 4,
          'Friday': 5, 'Saturday': 6, 'Sunday': 7
        };
        data.workSchedules.sort((a, b) => sortOrder[a.dayOfWeek] - sortOrder[b.dayOfWeek]);
        this.employee = data;
        
        this.totalWeeklyHours = this.employee.workSchedules.reduce((acc, schedule) => {
          if (schedule.isDayOff || !schedule.startTime || !schedule.endTime) return acc;
          const start = this.parseTime(schedule.startTime);
          const end = this.parseTime(schedule.endTime);
          return acc + (end - start);
        }, 0);

        if (this.employee.financeSettings) {
          this.financeForm.patchValue({
             commissionPercentage: this.employee.financeSettings.commissionPercentage,
             hourlyRate: this.employee.financeSettings.hourlyRate,
             hasPit2Filed: this.employee.financeSettings.hasPit2Filed,
             isPensionRetired: this.employee.financeSettings.isPensionRetired,
             voluntarySicknessInsurance: this.employee.financeSettings.voluntarySicknessInsurance,
             participatesInPPK: this.employee.financeSettings.participatesInPPK,
             ppkEmployeeRate: this.employee.financeSettings.ppkEmployeeRate,
             ppkEmployerRate: this.employee.financeSettings.ppkEmployerRate,
             commuteType: this.employee.financeSettings.commuteType
          });
        }
      },
      error: () => this.toastr.error('Nie udało się załadować szczegółów pracownika.')
    });
  }

  getDailyHours(ws: { startTime?: string; endTime?: string; isDayOff: boolean }): number {
    if (ws.isDayOff || !ws.startTime || !ws.endTime) return 0;
    return this.parseTime(ws.endTime) - this.parseTime(ws.startTime);
  }

  getReservationsForDay(dayOfWeek: string): EmployeeReservationDto[] {
    if (!this.employee?.upcomingReservations) return [];
    const dayMap: Record<string, number> = {
      'Monday': 1, 'Tuesday': 2, 'Wednesday': 3, 'Thursday': 4,
      'Friday': 5, 'Saturday': 6, 'Sunday': 0
    };
    const targetDay = dayMap[dayOfWeek];
    return this.employee.upcomingReservations.filter(r => {
      const d = new Date(r.startTime);
      return d.getDay() === targetDay;
    });
  }

  openFinanceModal() {
    this.showFinanceSettingsModal = true;
    this.applyFinanceFormRules();
  }

  closeFinanceModal() {
    this.showFinanceSettingsModal = false;
  }

  getActiveContractType(): string | null {
    if (!this.employee?.contracts) return null;
    const active = this.employee.contracts.find(c => c.isActive);
    return active?.contractType || null;
  }

  isUoP(): boolean {
    return this.getActiveContractType() === 'EmploymentContract';
  }

  isStudentZusExempt(): boolean {
    return !!this.employee?.isStudent && this.getEmployeeAge() < 26
      && this.getActiveContractType() === 'MandateContract';
  }

  private applyFinanceFormRules() {
    const contractType = this.getActiveContractType();
    const age = this.getEmployeeAge();
    const isStudent = this.employee?.isStudent ?? false;

    if (contractType === 'EmploymentContract') {
      this.financeForm.get('voluntarySicknessInsurance')?.setValue(true);
      this.financeForm.get('voluntarySicknessInsurance')?.disable();
    } else {
      this.financeForm.get('voluntarySicknessInsurance')?.enable();
    }

    if (isStudent && age < 26 && contractType === 'MandateContract') {
      this.financeForm.get('voluntarySicknessInsurance')?.setValue(false);
      this.financeForm.get('voluntarySicknessInsurance')?.disable();
      this.financeForm.get('isPensionRetired')?.setValue(false);
      this.financeForm.get('isPensionRetired')?.disable();
      this.financeForm.get('participatesInPPK')?.setValue(false);
      this.financeForm.get('participatesInPPK')?.disable();
    } else if (contractType !== 'EmploymentContract') {
      this.financeForm.get('voluntarySicknessInsurance')?.enable();
      this.financeForm.get('isPensionRetired')?.enable();
      this.financeForm.get('participatesInPPK')?.enable();
    }

    if (age > 70) {
      this.financeForm.get('participatesInPPK')?.setValue(false);
      this.financeForm.get('participatesInPPK')?.disable();
    }
  }

  getEmployeeAge(): number {
    if (!this.employee?.dateOfBirth) return 0;
    const dob = new Date(this.employee.dateOfBirth);
    const today = new Date();
    let age = today.getFullYear() - dob.getFullYear();
    const m = today.getMonth() - dob.getMonth();
    if (m < 0 || (m === 0 && today.getDate() < dob.getDate())) age--;
    return age;
  }

  isStudentEligible(): boolean {
    return this.getEmployeeAge() < 26;
  }

  openPayrollId: number | null = null;
  togglePayroll(id: number) {
    this.openPayrollId = this.openPayrollId === id ? null : id;
  }
  
  totalWeeklyHours = 0;

  private parseTime(timeStr: string): number {
    const [hours, minutes] = timeStr.split(':').map(Number);
    return hours + minutes / 60;
  }

  saveFinanceSettings() {
    if (!this.businessId || !this.employee) return;
    
    this.employeeService.updateFinanceSettings(this.businessId, this.employee.id, this.financeForm.getRawValue()).subscribe({
      next: () => {
        this.toastr.success('Zaktualizowano ustawienia finansowe.');
        this.showFinanceSettingsModal = false;
        this.loadData();
      },
      error: () => this.toastr.error('Błąd podczas zapisywania ustawień.')
    });
  }

  setTab(tabId: string) {
    this.activeTab = tabId;
  }

  calculateAge(dateOfBirth: string): number {
    const today = new Date();
    const birth = new Date(dateOfBirth);
    let age = today.getFullYear() - birth.getFullYear();
    const m = today.getMonth() - birth.getMonth();
    if (m < 0 || (m === 0 && today.getDate() < birth.getDate())) age--;
    return age;
  }

  getActiveContract() {
    return this.employee?.contracts?.find(c => c.isActive);
  }

  getPayrollStatusClass(status: string): string {
    switch (status) {
      case 'Paid': return 'badge-success';
      case 'Approved': return 'badge-info';
      case 'Calculated': return 'badge-warning';
      default: return 'badge-ghost';
    }
  }

  getAbsenceTypeClass(type: string): string {
    switch (type) {
      case 'Vacation': return 'badge-success';
      case 'SickLeave': return 'badge-error';
      case 'UnpaidLeave': return 'badge-warning';
      case 'Training': return 'badge-info';
      default: return 'badge-ghost';
    }
  }


  get employeeAsBase(): Employee | null {
    if (!this.employee) return null;
    return {
      id: this.employee.id,
      firstName: this.employee.firstName,
      lastName: this.employee.lastName,
      position: this.employee.position ?? null,
      photoUrl: this.employee.photoUrl,
      dateOfBirth: this.employee.dateOfBirth,
      specialization: this.employee.specialization,
      instagramProfileUrl: this.employee.instagramProfileUrl,
      portfolioUrl: this.employee.portfolioUrl,
      isStudent: this.employee.isStudent,
      isArchived: this.employee.isArchived
    };
  }

  onEditModalClosed(result: boolean) {
    this.isEditModalVisible = false;
    if (result) this.loadData();
  }

  onScheduleModalClosed(saved: boolean) {
    this.isScheduleModalVisible = false;
    if (saved) this.loadData();
  }

  onAssignServicesModalClosed(result: boolean) {
    this.isAssignServicesModalVisible = false;
    if (result) this.loadData();
  }

  openPhotoModal(employee: EmployeeDetail) {
    this.employeeForPhoto = {
      ...employee,
      position: employee.position ?? null
    };
  }

  closePhotoModalAndRefresh(refresh: boolean) {
    this.employeeForPhoto = null;
    if (refresh) this.loadData();
  }

  openEditModal(employee: EmployeeDetail) {
    this.isEditModalVisible = true;
  }

  openScheduleModal(employee: EmployeeDetail) {
    this.isScheduleModalVisible = true;
  }

  openAssignServicesModal(employee: EmployeeDetail) {
    this.isAssignServicesModalVisible = true;
  }

  openContractModal(employee: EmployeeDetail) {
    this.generateContract();
  }



  archiveEmployee() {
    if (!this.employee || !this.businessId) return;
    const name = `${this.employee.firstName} ${this.employee.lastName}`;
    if (confirm(`Czy na pewno chcesz zarchiwizować pracownika: ${name}? Ta operacja anuluje wszystkie jego przyszłe rezerwacje.`)) {
      this.employeeService.deleteEmployee(this.businessId, this.employee.id).subscribe({
        next: () => {
          this.toastr.success('Pracownik został zarchiwizowany.');
          this.router.navigate(['/dashboard/employees']);
        },
        error: () => this.toastr.error('Wystąpił błąd.')
      });
    }
  }

  addCertificate() {
    if (!this.employee || !this.businessId || !this.newCert.name) return;
    this.employeeService.addCertificate(this.businessId, this.employee.id, this.newCert).subscribe({
      next: (cert) => {
        this.employee!.certificates.push(cert);
        this.newCert = { name: '', institution: '', dateObtained: '', isVisibleToClient: true };
        this.showCertForm = false;
        this.toastr.success('Certyfikat został dodany.');
      },
      error: () => this.toastr.error('Nie udało się dodać certyfikatu.')
    });
  }

  deleteCertificate(certId: number) {
    if (!this.employee || !this.businessId) return;
    if (!confirm('Czy na pewno chcesz usunąć ten certyfikat?')) return;
    this.employeeService.deleteCertificate(this.businessId, this.employee.id, certId).subscribe({
      next: () => {
        this.employee!.certificates = this.employee!.certificates.filter(c => c.certificateId !== certId);
        this.toastr.success('Certyfikat usunięty.');
      },
      error: () => this.toastr.error('Nie udało się usunąć certyfikatu.')
    });
  }

  addAbsence() {
    if (!this.employee || !this.businessId || !this.newAbsence.dateFrom || !this.newAbsence.dateTo) return;

    const from = this.newAbsence.dateFrom;
    const to = this.newAbsence.dateTo;
    const hasOverlap = this.employee.scheduleExceptions?.some(
      ex => ex.dateFrom <= to && ex.dateTo >= from
    );
    if (hasOverlap) {
      this.toastr.error('W wybranym okresie istnieje już inna nieobecność.');
      return;
    }
    this.employeeService.addAbsence(this.businessId, this.employee.id, this.newAbsence).subscribe({
      next: (resp: any) => {
        this.newAbsence = { dateFrom: '', dateTo: '', type: 'Vacation', reason: '', blocksCalendar: true };
        this.showAbsenceForm = false;
        const cancelled = resp.cancelledReservations || 0;
        if (cancelled > 0) {
          this.toastr.success(`Nieobecność dodana. Odwołano ${cancelled} wizyt(y) w tym okresie.`);
        } else {
          this.toastr.success('Nieobecność została dodana.');
        }
        this.loadData();
      },
      error: () => this.toastr.error('Nie udało się dodać nieobecności.')
    });
  }

  toggleAbsenceApproval(absenceId: number) {
    if (!this.employee || !this.businessId) return;
    this.employeeService.toggleAbsenceApproval(this.businessId, this.employee.id, absenceId).subscribe({
      next: (res: any) => {
        const absence = this.employee!.scheduleExceptions.find(a => a.exceptionId === absenceId);
        if (absence) absence.isApproved = res.isApproved;
        this.toastr.success(res.message || 'Status zmieniony.');
      },
      error: () => this.toastr.error('Nie udało się zmienić statusu.')
    });
  }

  deleteAbsence(absenceId: number) {
    if (!this.employee || !this.businessId) return;
    if (!confirm('Czy na pewno chcesz usunąć tę nieobecność?')) return;
    this.employeeService.deleteAbsence(this.businessId, this.employee.id, absenceId).subscribe({
      next: () => {
        this.employee!.scheduleExceptions = this.employee!.scheduleExceptions.filter(a => a.exceptionId !== absenceId);
        this.toastr.success('Nieobecność usunięta.');
      },
      error: () => this.toastr.error('Nie udało się usunąć nieobecności.')
    });
  }

  generateContract() {
    if (!this.employee) return;
    this.businessService.generateContract(this.employee.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `Umowa_${this.employee?.lastName}_${this.employee?.firstName}.docx`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.toastr.success('Pobrano umowę.');
      },
      error: () => this.toastr.error('Nie udało się wygenerować umowy. Upewnij się, że wgrano szablon.')
    });
  }
}