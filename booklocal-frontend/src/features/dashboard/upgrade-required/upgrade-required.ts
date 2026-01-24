import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';

@Component({
  selector: 'app-upgrade-required',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './upgrade-required.html'
})
export class UpgradeRequiredComponent implements OnInit {
  private route = inject(ActivatedRoute);
  featureName: string = 'Zaawansowane narzędzia';

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      if (params['feature']) {
        switch(params['feature']) {
            case 'reports': this.featureName = 'Zaawansowane Raporty'; break;
            case 'marketing': this.featureName = 'Narzędzia Marketingowe'; break;
        }
      }
    });
  }
}
