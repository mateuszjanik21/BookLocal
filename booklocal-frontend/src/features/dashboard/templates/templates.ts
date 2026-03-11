import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BusinessService } from '../../../core/services/business-service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-templates',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './templates.html'
})
export class TemplatesComponent {
  private businessService = inject(BusinessService);
  private toastr = inject(ToastrService);

  isUploading = false;
  selectedFile: File | null = null;

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) {
      if (!file.name.endsWith('.docx')) {
        this.toastr.error('Dozwolone są tylko pliki .docx');
        return;
      }
      this.selectedFile = file;
    }
  }

  uploadTemplate(templateName: string) {
    if (!this.selectedFile) return;

    this.isUploading = true;
    const formData = new FormData();
    formData.append('File', this.selectedFile);
    formData.append('TemplateName', templateName);

    this.businessService.uploadTemplate(formData).subscribe({
      next: () => {
        this.toastr.success(`Szablon ${templateName} wgrany pomyślnie.`);
        this.isUploading = false;
        this.selectedFile = null;
      },
      error: () => {
        this.toastr.error('Błąd podczas wgrywania szablonu.');
        this.isUploading = false;
      }
    });
  }

  readonly sampleContractTemplate = `{{TypUmowy}}
Zawarta w dniu {{DataGenerowania}}, pomiędzy:
Pracodawcą: {{DaneFirmy}} reprezentowanym przez: {{Reprezentant}} zwanym dalej „Pracodawcą”,
a
Pracownikiem: Panem/Panią {{Pracownik}}, urodzonym/ą w dniu: {{DataUrodzenia}}, zwanym/ą dalej „Pracownikiem”.
§ 1 Przedmiot Umowy
1. Pracodawca powierza, a Pracownik przyjmuje do wykonania obowiązki na stanowisku: {{Stanowisko}}.
2. Miejscem wykonywania pracy jest siedziba Pracodawcy lub inne miejsce wskazane przez przełożonego.
3. Termin rozpoczęcia pracy ustala się na dzień: {{DataRozpoczecia}}.
§ 2 Czas Pracy i Warunki
1. Umowa zostaje zawarta na podstawie: {{TypUmowy}}.
2. Szczegółowy zakres obowiązków zostanie przedstawiony Pracownikowi przez przełożonego.
§ 3 Wynagrodzenie
1. Z tytułu wykonywania obowiązków określonych w niniejszej umowie, Pracownikowi przysługuje wynagrodzenie w wysokości:
   • Wynagrodzenie zasadnicze: {{Wynagrodzenie}} PLN.
2. Wynagrodzenie płatne jest z dołu, w terminie do 10. dnia następnego miesiąca.
§ 4 Postanowienia Końcowe
1. Wszelkie zmiany niniejszej umowy wymagają formy pisemnej pod rygorem nieważności.
2. W sprawach nieuregulowanych niniejszą umową mają zastosowanie przepisy Kodeksu Pracy oraz Kodeksu Cywilnego.
3. Umowę sporządzono w dwóch jednobrzmiących egzemplarzach.`;

  copyTemplateToClipboard() {
    const textToCopy = this.sampleContractTemplate + '\n\n..............................................                   ..............................................\n(Podpis Pracodawcy)                                          (Podpis Pracownika)';
    navigator.clipboard.writeText(textToCopy).then(() => {
      this.toastr.success('Szablon skopiowany do schowka!');
    }).catch(err => {
      this.toastr.error('Nie udało się skopiować szablonu.');
      console.error('Błąd kopiowania:', err);
    });
  }
}
