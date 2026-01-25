import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InvoiceDto, InvoiceService } from '../../../core/services/invoice-service';
import { BusinessService } from '../../../core/services/business-service';
import { ToastrService } from 'ngx-toastr';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

@Component({
  selector: 'app-invoices-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './invoices.html',
})
export class InvoicesListComponent implements OnInit {
  private invoiceService = inject(InvoiceService);
  private businessService = inject(BusinessService);
  private toastr = inject(ToastrService);

  invoices: InvoiceDto[] = [];
  isLoading = true;
  businessId: number | null = null;
  businessName: string = '';
  
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;

  ngOnInit(): void {
    this.businessService.getMyBusiness().subscribe({
        next: (business) => {
            if (business) {
                this.businessId = business.id;
                this.businessName = business.name;
                this.loadInvoices();
            }
        }
    });
  }

  loadInvoices(): void {
    if (!this.businessId) return;
    this.isLoading = true;
    this.invoiceService.getInvoices(this.businessId, this.currentPage, this.pageSize).subscribe({
      next: (data) => {
        this.invoices = data.items;
        this.totalCount = data.totalCount;
        this.totalPages = data.totalPages;
        this.isLoading = false;
      },
      error: () => {
        this.toastr.error('Błąd pobierania faktur.');
        this.isLoading = false;
      }
    });
  }

  changePage(newPage: number): void {
      if (newPage < 1 || newPage > this.totalPages) return;
      this.currentPage = newPage;
      this.loadInvoices();
  }

  async downloadPdf(invoice: InvoiceDto): Promise<void> {
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
    doc.text(`Data wystawienia: ${new Date(invoice.issueDate).toLocaleDateString()}`, 140, 20);
    doc.text(`Data sprzedaży: ${new Date(invoice.saleDate).toLocaleDateString()}`, 140, 26);

    doc.line(14, 35, 196, 35);
    
    doc.text('Sprzedawca:', 14, 42);
    doc.setFontSize(11);
    doc.text(this.businessName, 14, 48);
    
    doc.setFontSize(10);
    doc.text('Nabywca:', 110, 42);
    doc.setFontSize(11);
    doc.text(invoice.customerName, 110, 48);
    if (invoice.customerNip) doc.text(`NIP: ${invoice.customerNip}`, 110, 54);

    const tableData = invoice.items.map(item => [
        item.name,
        item.quantity,
        `${item.unitPriceNet.toFixed(2)}`,
        `${(item.vatRate * 100).toFixed(0)}%`,
        `${item.netValue.toFixed(2)}`,
        `${item.grossValue.toFixed(2)}`
    ]);

    autoTable(doc, {
        startY: 65,
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
    
    const paymentMethodText = ['Gotówka', 'Karta', 'Online', 'Inne'][invoice.paymentMethod] || 'Inne';
    doc.text(`Metoda płatności: ${paymentMethodText}`, 14, lastY);

    doc.save(`Faktura_${invoice.invoiceNumber.replace(/\//g, '_')}.pdf`);
  }
}
