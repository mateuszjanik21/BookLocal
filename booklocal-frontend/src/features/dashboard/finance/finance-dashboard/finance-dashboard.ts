import { Component, ElementRef, OnInit, ViewChild, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DailyFinancialReport, FinanceService } from '../../../../core/services/finance-service';
import { BusinessService } from '../../../../core/services/business-service';
import { ToastrService } from 'ngx-toastr';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import { DailyEmployeePerformance } from '../../../../types/report.model';

@Component({
  selector: 'app-finance-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './finance-dashboard.html',
})
export class FinanceDashboardComponent implements OnInit {
  private financeService = inject(FinanceService);
  private businessService = inject(BusinessService);
  private toastr = inject(ToastrService);

  @ViewChild('generateModal') generateModal!: ElementRef<HTMLDialogElement>;
  @ViewChild('performanceModal') performanceModal!: ElementRef<HTMLDialogElement>;

  reports: DailyFinancialReport[] = [];
  isLoading = true;
  isGenerating = false;

  selectedMonth: number;
  selectedYear: number;
  
  dateToGenerate: string;
  maxDate: string;
  businessId: number | null = null;
  businessName: string = '';

  generationMode: 'single' | 'range' | 'month' = 'single';
  generateMonthValue: string;
  rangeStartDate: string;
  rangeEndDate: string;

  months = [
    { value: 1, label: 'Styczeń' },
    { value: 2, label: 'Luty' },
    { value: 3, label: 'Marzec' },
    { value: 4, label: 'Kwiecień' },
    { value: 5, label: 'Maj' },
    { value: 6, label: 'Czerwiec' },
    { value: 7, label: 'Lipiec' },
    { value: 8, label: 'Sierpień' },
    { value: 9, label: 'Wrzesień' },
    { value: 10, label: 'Październik' },
    { value: 11, label: 'Listopad' },
    { value: 12, label: 'Grudzień' }
  ];

  viewMode: 'day' | 'week' | 'month' = 'day';
  
  currentDate: Date = new Date();
  
  get displayDateLabel(): string {
      const d = this.currentDate;
      if (this.viewMode === 'day') {
          return d.toLocaleDateString('pl-PL', { day: '2-digit', month: 'long', year: 'numeric' });
      } else if (this.viewMode === 'month') {
          return d.toLocaleDateString('pl-PL', { month: 'long', year: 'numeric' });
      } else {
          const start = this.getStartOfWeek(d);
          const end = new Date(start);
          end.setDate(end.getDate() + 6);
          return `${start.toLocaleDateString('pl-PL')} - ${end.toLocaleDateString('pl-PL')}`;
      }
  }

  constructor() {
    const today = new Date();
    this.currentDate = today;
    this.selectedMonth = today.getMonth() + 1;
    this.selectedYear = today.getFullYear();
    this.dateToGenerate = today.toISOString().split('T')[0];
    this.rangeStartDate = new Date(today.getFullYear(), today.getMonth(), 1).toISOString().split('T')[0]; 
    this.rangeEndDate = this.dateToGenerate;
    this.maxDate = this.dateToGenerate;
    this.generateMonthValue = this.dateToGenerate.slice(0, 7);
  }

  ngOnInit(): void {
    this.businessService.getMyBusiness().subscribe({
      next: (business) => {
        if (business) {
            this.businessId = business.id;
            this.businessName = business.name;
            this.loadData();
        }
      },
      error: () => this.toastr.error('Nie udało się pobrać danych firmy.')
    });
  }

  changeViewMode(mode: 'day' | 'week' | 'month') {
      this.viewMode = mode;
      this.currentDate = new Date(); 
      this.loadData();
  }

  changeDate(delta: number) {
      const d = new Date(this.currentDate);
      if (this.viewMode === 'day') {
          d.setDate(d.getDate() + delta);
      } else if (this.viewMode === 'month') {
          d.setMonth(d.getMonth() + delta);
      } else {
          d.setDate(d.getDate() + (delta * 7));
      }
      this.currentDate = d;
      this.loadData();
  }

  getStartOfWeek(d: Date): Date {
      const date = new Date(d);
      const day = date.getDay();
      const diff = date.getDate() - day + (day == 0 ? -6 : 1);
      date.setDate(diff);
      date.setHours(0, 0, 0, 0);
      return date;
  }

  loadData() {
      if (!this.businessId) return;

      let start: string;
      let end: string;

      if (this.viewMode === 'day') {
          const dateStr = this.currentDate.toISOString().split('T')[0];
          start = dateStr;
          end = dateStr;
      } else if (this.viewMode === 'month') {
          const y = this.currentDate.getFullYear();
          const m = this.currentDate.getMonth();
          start = new Date(y, m, 1).toISOString().split('T')[0];
          end = new Date(y, m + 1, 0).toISOString().split('T')[0];
      } else {
          const s = this.getStartOfWeek(new Date(this.currentDate));
          const e = new Date(s);
          e.setDate(e.getDate() + 6);
          start = s.toISOString().split('T')[0];
          end = e.toISOString().split('T')[0];
      }

      this.loadEmployeePerformance(start, end);
      
      this.selectedMonth = this.currentDate.getMonth() + 1;
      this.selectedYear = this.currentDate.getFullYear();
      this.loadReports(); 
  }

  loadReports(): void {
    if (!this.businessId) return;

    this.isLoading = true;
    this.financeService.getReports(this.businessId, this.selectedMonth, this.selectedYear).subscribe({
      next: (data) => {
        this.reports = data;
        this.isLoading = false;
      },
      error: () => {
        this.toastr.error('Nie udało się pobrać raportów.');
        this.isLoading = false;
      }
    });
  }
  
  employeePerformance: DailyEmployeePerformance[] = [];
  isLoadingPerformance = false;

  loadEmployeePerformance(start: string, end: string) {
      if (!this.businessId) return;
      this.isLoadingPerformance = true;
      
      this.financeService.getEmployeePerformance(this.businessId, undefined, start, end).subscribe({
          next: (data) => {
              this.employeePerformance = data;
              this.isLoadingPerformance = false;
          },
          error: () => this.isLoadingPerformance = false
      });
  }


  openGenerateModal(): void {
    this.generateModal.nativeElement.showModal();
  }

  confirmGenerate(): void {
      if (this.generationMode === 'single') {
        if (!this.dateToGenerate) return;
        this.executeGenerate(this.dateToGenerate);
      } else if (this.generationMode === 'month') {
        if (!this.generateMonthValue) return;
        
        const [year, month] = this.generateMonthValue.split('-').map(Number);
        const startDate = new Date(year, month - 1, 1);
        const endDate = new Date(year, month, 0);

        const formatDate = (d: Date) => {
            const y = d.getFullYear();
            const m = String(d.getMonth() + 1).padStart(2, '0');
            const day = String(d.getDate()).padStart(2, '0');
            return `${y}-${m}-${day}`;
        };

        this.rangeStartDate = formatDate(startDate);
        this.rangeEndDate = formatDate(endDate);
        
        this.executeGenerateRange();
      } else {
        if (!this.rangeStartDate || !this.rangeEndDate) return;
        this.executeGenerateRange();
      }
  }

  regenerateReport(date: string): void {
      this.dateToGenerate = date;
      this.generationMode = 'single';
      this.executeGenerate(date);
  }

  private executeGenerate(date: string) {
    if (!this.businessId) return;

    this.isGenerating = true;
    this.financeService.generateDailyReport(this.businessId, date).subscribe({
        next: (report) => {
            this.toastr.success(`Raport za ${date} wygenerowany pomyślnie.`);
            this.isGenerating = false;
            this.generateModal.nativeElement.close();
            this.loadData();
        },
        error: (err) => {
            this.toastr.error('Błąd generowania raportu.');
            this.isGenerating = false;
        }
    });
  }

  private executeGenerateRange() {
      if (!this.businessId) return;
      
      this.isGenerating = true;
      this.financeService.generateReportRange(this.businessId, this.rangeStartDate, this.rangeEndDate).subscribe({
          next: () => {
              this.toastr.success('Raporty zostały wygenerowane.');
              this.isGenerating = false;
              this.generateModal.nativeElement.close();
              this.loadData();
          },
          error: (err) => {
              this.toastr.error(err.error || 'Błąd generowania zakresu.');
              this.isGenerating = false;
          }
      });
  }

  deleteReport(date: string): void {
      if (!this.businessId) return;
      if (!confirm(`Czy na pewno chcesz usunąć raport z dnia ${date}?`)) return;

      this.financeService.deleteReport(this.businessId, date).subscribe({
          next: () => {
              this.toastr.success('Raport usunięty.');
              this.loadReports();
          },
          error: () => this.toastr.error('Nie udało się usunąć raportu.')
      });
  }

  async downloadFinancialPdf(): Promise<void> {
    const doc = new jsPDF();
    await this.setupPdfFont(doc);

    doc.setFontSize(18);
    doc.text(`Raport Finansowy: ${this.displayDateLabel}`, 14, 20);
    
    doc.setFontSize(12);
    doc.text(`Firma: ${this.businessName}`, 14, 30);
    doc.text(`Data: ${new Date().toLocaleDateString()}`, 14, 36);

    doc.setFontSize(14);
    doc.text('Podsumowanie', 14, 50);
    
    const summaryData = [
        ['Całkowity Przychód', `${this.monthTotalRevenue.toFixed(2)} PLN`],
        ['Gotówka', `${this.monthCashRevenue.toFixed(2)} PLN`],
        ['Karta', `${this.monthCardRevenue.toFixed(2)} PLN`],
        ['Online', `${this.monthOnlineRevenue.toFixed(2)} PLN`],
        ['Liczba Wizyt', `${this.monthCompletedAppointments}`],
        ['Prowizja Systemu', `${this.monthTotalCommission.toFixed(2)} PLN`],
        ['Nowi Klienci', `${this.monthNewCustomers}`]
    ];

    autoTable(doc, {
        startY: 55,
        head: [['Metryka', 'Wartość']],
        body: summaryData,
        theme: 'striped',
        headStyles: { fillColor: [66, 66, 66], font: 'Roboto' }, 
        bodyStyles: { font: 'Roboto' },
        styles: { font: 'Roboto', fontStyle: 'normal' }
    });

    const lastY = (doc as any).lastAutoTable.finalY || 100;
    
    doc.text('Szczegółowe Raporty', 14, lastY + 15);

    const tableData = this.visibleReports.map(r => [
        r.reportDate,
        `${r.totalRevenue.toFixed(2)}`,
        `${r.cashRevenue.toFixed(2)}`,
        `${r.cardRevenue.toFixed(2)}`,
        `${r.onlineRevenue.toFixed(2)}`,
        `${r.completedAppointments}`,
        `${(r as any).totalCommission?.toFixed(2) ?? '0.00'}`,
        r.topSellingServiceName || '-'
    ]);

    autoTable(doc, {
        startY: lastY + 20,
        head: [['Data', 'Suma', 'Gotówka', 'Karta', 'Online', 'Wizyty', 'Prowizja', 'Top Usługa']],
        body: tableData,
        theme: 'grid',
        headStyles: { fillColor: [41, 128, 185], font: 'Roboto' },
        bodyStyles: { font: 'Roboto' },
        styles: { fontSize: 8, font: 'Roboto', fontStyle: 'normal' }
    });

    doc.save(`Raport_Finansowy_${this.currentDate.toISOString().slice(0,10)}.pdf`);
  }

  async downloadPerformancePdf(): Promise<void> {
      const doc = new jsPDF();
      await this.setupPdfFont(doc);

      doc.setFontSize(18);
      doc.text(`Raport Wydajności: ${this.displayDateLabel}`, 14, 20);
      doc.setFontSize(12);
      doc.text(`Firma: ${this.businessName}`, 14, 30);

      doc.setFontSize(14);
      doc.text('Ranking Pracowników', 14, 55);

      const maxRev = Math.max(...this.employeePerformance.map(e => e.totalRevenue), 1);

      const empData = this.employeePerformance.map(e => [
          e.fullName,
          `${e.totalRevenue.toFixed(2)}`,
          `${e.commission.toFixed(2)}`,
          `${e.completedAppointments} / ${e.totalAppointments}`,
          e.averageRating > 0 ? e.averageRating.toFixed(2) : '-'
      ]);

      autoTable(doc, {
          startY: 60,
          head: [['Pracownik', 'Przychód (PLN)', 'Prowizja (PLN)', 'Wizyty (OK/All)', 'Ocena']],
          body: empData,
          theme: 'grid',
          headStyles: { fillColor: [41, 128, 185], font: 'Roboto' },
          bodyStyles: { font: 'Roboto' }
      });

      let chartY = (doc as any).lastAutoTable.finalY + 15;
      
      doc.setFontSize(14);
      doc.text('Wizualizacja Przychodu', 14, chartY);
      chartY += 10;
      doc.setFontSize(10);

      this.employeePerformance.forEach(emp => {
          if (chartY > 270) {
              doc.addPage();
              chartY = 20;
          }
           
          doc.text(emp.fullName, 14, chartY + 5);
          
          const barWidth = (emp.totalRevenue / maxRev) * 100;
          if (barWidth > 0) {
            doc.setFillColor(76, 175, 80);
            doc.rect(60, chartY, barWidth, 8, 'F');
          }
          
          doc.setTextColor(0);
          doc.text(`${emp.totalRevenue.toFixed(0)} PLN`, 60 + barWidth + 2, chartY + 5);
          
          chartY += 12;
      });

      doc.save(`Raport_Wydajnosci_${this.currentDate.toISOString().slice(0,10)}.pdf`);
  }

  private async setupPdfFont(doc: jsPDF) {
      try {
        const fontResponse = await fetch('/assets/fonts/Roboto-Regular.ttf');
        if (fontResponse.ok) {
            const fontBlob = await fontResponse.blob();
            const reader = new FileReader();
            return new Promise<void>((resolve) => {
                reader.onloadend = () => {
                    const fontBase64 = (reader.result as string).split(',')[1];
                    doc.addFileToVFS('Roboto-Regular.ttf', fontBase64);
                    doc.addFont('Roboto-Regular.ttf', 'Roboto', 'normal');
                    doc.setFont('Roboto');
                    resolve();
                };
                reader.readAsDataURL(fontBlob);
            });
        }
      } catch (e) {
          console.warn('Font load fail', e);
      }
  }

  get visibleReports(): DailyFinancialReport[] {
      if (this.viewMode === 'month') return this.reports;

      if (this.viewMode === 'day') {
          const d = this.currentDate.toISOString().split('T')[0];
          return this.reports.filter(r => r.reportDate === d);
      }

      const start = this.getStartOfWeek(new Date(this.currentDate));
      const end = new Date(start);
      end.setDate(end.getDate() + 7);
      return this.reports.filter(r => {
          const rd = new Date(r.reportDate);
          return rd >= start && rd <= end;
      });
  }

  get monthTotalRevenue(): number {
      return this.visibleReports.reduce((sum, r) => sum + r.totalRevenue, 0);
  }
  get monthCashRevenue(): number {
    return this.visibleReports.reduce((sum, r) => sum + r.cashRevenue, 0);
  }
  get monthCardRevenue(): number {
      return this.visibleReports.reduce((sum, r) => sum + r.cardRevenue, 0);
  }
  get monthOnlineRevenue(): number {
    return this.visibleReports.reduce((sum, r) => sum + r.onlineRevenue, 0);
  }
  get monthTotalAppointments(): number {
      return this.visibleReports.reduce((sum, r) => sum + r.totalAppointments, 0);
  }
  get monthCompletedAppointments(): number {
      return this.visibleReports.reduce((sum, r) => sum + r.completedAppointments, 0);
  }
  get monthTotalCommission(): number {
      return this.visibleReports.reduce((sum, r) => sum + (r as any).totalCommission, 0);
  }

  get monthNewCustomers(): number {
      return this.visibleReports.reduce((sum, r) => sum + r.newCustomersCount, 0);
  }

  get monthAverageTicketValue(): number {
      const revenue = this.monthTotalRevenue;
      const count = this.monthCompletedAppointments;
      return count > 0 ? revenue / count : 0;
  }

  get topEmployee(): DailyEmployeePerformance | null {
      if (!this.employeePerformance || this.employeePerformance.length === 0) return null;
      return this.employeePerformance.reduce((prev, current) => (prev.totalRevenue > current.totalRevenue) ? prev : current);
  }

  goToDay(dateStr: string) {
      this.currentDate = new Date(dateStr);
      this.viewMode = 'day';
      this.loadData();
      window.scrollTo({ top: 0, behavior: 'smooth' });
  }
}
