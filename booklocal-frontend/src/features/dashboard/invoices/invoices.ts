import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { InvoiceDto, InvoiceService } from '../../../core/services/invoice-service';
import { BusinessService } from '../../../core/services/business-service';
import { ToastrService } from 'ngx-toastr';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';

@Component({
  selector: 'app-invoices-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './invoices.html',
  styleUrl: './invoices.css',
  animations: [
    trigger('expandCollapse', [
      state('void', style({ height: '0', opacity: 0, overflow: 'hidden' })),
      state('*', style({ height: '*', opacity: 1, overflow: 'hidden' })),
      transition('void <=> *', [animate('250ms ease-in-out')])
    ])
  ]
})
export class InvoicesListComponent implements OnInit {
  private invoiceService = inject(InvoiceService);
  private businessService = inject(BusinessService);
  private toastr = inject(ToastrService);

  invoices: InvoiceDto[] = [];
  isLoading = true;
  showSpinner = false;
  private spinnerTimeout: any;
  businessId: number | null = null;
  businessName: string = '';
  businessNip: string = '';
  businessAddress: string = '';
  businessOwner: string = '';
  
  currentPage = 1;
  pageSize = 15;
  totalCount = 0;
  totalPages = 0;

  searchQuery = '';
  monthFilter = '';
  private searchSubject = new Subject<string>();

  expandedInvoiceId: number | null = null;

  totalGrossSum = 0;

  paymentMethods: Record<string | number, string> = {
    0: 'Gotówka',
    1: 'Karta',
    2: 'Online',
    3: 'Inne',
    'Cash': 'Gotówka',
    'Card': 'Karta',
    'Online': 'Online',
    'Other': 'Inne'
  };

  ngOnInit(): void {
    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe(() => {
      this.currentPage = 1;
      this.loadInvoices();
    });

    this.businessService.getMyBusiness().subscribe({
      next: (business) => {
        if (business) {
          this.businessId = business.id;
          this.businessName = business.name;
          this.businessNip = business.nip || '';
          this.businessAddress = `${business.address || ''}, ${business.city || ''}`.replace(/^, | , $/g, '');
          this.businessOwner = business.owner ? `${business.owner.firstName} ${business.owner.lastName || ''}`.trim() : '';
          this.loadInvoices();
        }
      },
      error: () => this.toastr.error('Nie udało się pobrać danych firmy.')
    });
  }

  onSearchInput(): void {
    this.searchSubject.next(this.searchQuery);
  }

  onMonthChange(): void {
    this.currentPage = 1;
    this.loadInvoices();
  }

  clearFilters(): void {
    this.searchQuery = '';
    this.monthFilter = '';
    this.currentPage = 1;
    this.loadInvoices();
  }

  loadInvoices(): void {
    if (!this.businessId) return;
    this.isLoading = true;
    this.showSpinner = false;
    clearTimeout(this.spinnerTimeout);
    this.spinnerTimeout = setTimeout(() => {
      if (this.isLoading) this.showSpinner = true;
    }, 500);
    this.invoiceService.getInvoices(
      this.businessId,
      this.currentPage,
      this.pageSize,
      this.searchQuery || undefined,
      this.monthFilter || undefined
    ).subscribe({
      next: (data) => {
        this.invoices = data.items;
        this.totalCount = data.totalCount;
        this.totalPages = data.totalPages;

        this.totalGrossSum = data.totalGrossSum ?? 0;
        setTimeout(() => {
          this.isLoading = false;
          this.showSpinner = false;
        }, 300);
      },
      error: () => {
        this.toastr.error('Błąd pobierania faktur.');
        setTimeout(() => {
          this.isLoading = false;
          this.showSpinner = false;
        }, 300);
      }
    });
  }

  toggleDetails(invoiceId: number): void {
    this.expandedInvoiceId = this.expandedInvoiceId === invoiceId ? null : invoiceId;
  }

  changePage(newPage: number): void {
    if (newPage < 1 || newPage > this.totalPages) return;
    this.currentPage = newPage;
    this.loadInvoices();
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxVisible = 5;
    let start = Math.max(1, this.currentPage - Math.floor(maxVisible / 2));
    let end = Math.min(this.totalPages, start + maxVisible - 1);
    if (end - start + 1 < maxVisible) {
      start = Math.max(1, end - maxVisible + 1);
    }
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  }

  get rangeStart(): number {
    return (this.currentPage - 1) * this.pageSize + 1;
  }

  get rangeEnd(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalCount);
  }

  async previewPdf(invoice: InvoiceDto): Promise<void> {
    const doc = await this.buildPdfDoc(invoice);
    const blobUrl = doc.output('bloburl');
    window.open(blobUrl as unknown as string, '_blank');
  }

  async downloadPdf(invoice: InvoiceDto): Promise<void> {
    const doc = await this.buildPdfDoc(invoice);
    doc.save(`Faktura_${invoice.invoiceNumber.replace(/\//g, '_')}.pdf`);
  }

  private async buildPdfDoc(invoice: InvoiceDto) {
    const { default: jsPDF } = await import('jspdf');
    const { default: autoTable } = await import('jspdf-autotable');

    const doc = new jsPDF();

    try {
      const fontResponse = await fetch('/assets/fonts/Roboto-Regular.ttf');
      if (fontResponse.ok) {
        const fontBlob = await fontResponse.blob();
        const reader = new FileReader();
        reader.readAsDataURL(fontBlob);
        await new Promise<void>((resolve) => {
          reader.onloadend = () => {
            const fontBase64 = (reader.result as string).split(',')[1];
            doc.addFileToVFS('Roboto-Regular.ttf', fontBase64);
            doc.addFont('Roboto-Regular.ttf', 'Roboto', 'normal');
            doc.setFont('Roboto');
            resolve();
          };
        });
      }
    } catch (e) { console.warn('Font load failed, using default'); }

    doc.setFontSize(22);
    doc.text('FAKTURA VAT', 14, 20);
    doc.setFontSize(12);
    doc.text(`Nr: ${invoice.invoiceNumber}`, 14, 28);
    
    doc.setFontSize(10);
    doc.text(`Data wystawienia: ${new Date(invoice.issueDate).toLocaleDateString()}`, 110, 20);
    doc.text(`Data sprzedaży: ${new Date(invoice.saleDate).toLocaleDateString()}`, 110, 26);
    doc.text(`Wystawił(a): ${this.businessOwner || 'Samoobsługa/System'}`, 110, 32);

    doc.line(14, 38, 196, 38);
    
    doc.setFontSize(11);
    doc.text('Sprzedawca:', 14, 46);
    doc.setFontSize(10);
    doc.text(this.businessName || 'Brak nazwy', 14, 52);
    doc.text(this.businessAddress || '-', 14, 57);
    doc.text(`NIP: ${this.businessNip || '-'}`, 14, 62);
    
    doc.setFontSize(11);
    doc.text('Nabywca:', 110, 46);
    doc.setFontSize(10);
    doc.text(invoice.customerName || 'Nieznany', 110, 52);
    if (invoice.customerNip) doc.text(`NIP: ${invoice.customerNip}`, 110, 57);

    const tableData = invoice.items.map(item => [
      item.name,
      item.quantity,
      `${item.unitPriceNet.toFixed(2)}`,
      `${(item.vatRate * 100).toFixed(0)}%`,
      `${item.netValue.toFixed(2)}`,
      `${item.grossValue.toFixed(2)}`
    ]);

    autoTable(doc, {
      startY: 75,
      head: [['Nazwa', 'Ilość', 'Cena Netto', 'VAT', 'Wartość Netto', 'Wartość Brutto']],
      body: tableData,
      theme: 'grid',
      headStyles: { fillColor: [66, 66, 66], font: 'Roboto' },
      bodyStyles: { font: 'Roboto' },
      styles: { font: 'Roboto', fontStyle: 'normal' }
    });

    const lastY = (doc as any).lastAutoTable.finalY + 10;

    doc.setFontSize(12);
    doc.text(`Do zapłaty: ${invoice.totalGross.toFixed(2)} PLN`, 140, lastY);
    doc.setFontSize(10);
    
    const paymentMethodText = this.paymentMethods[invoice.paymentMethod] || 'Inne';
    doc.text(`Metoda płatności: ${paymentMethodText}`, 14, lastY);

    return doc;
  }
}
