import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'reservationStatus',
  standalone: true
})
export class ReservationStatusPipe implements PipeTransform {
  transform(value: string): string {
    const lowerCaseStatus = value?.toLowerCase();
    switch (lowerCaseStatus) {
      case 'confirmed':
        return 'Potwierdzona';
      case 'completed':
        return 'Ukończona';
      case 'cancelled':
        return 'Anulowana';
      default:
        return value; 
    }
  }
}