import { Component, EventEmitter, Input, OnDestroy, OnInit, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ScheduleService } from '../../../core/services/schedule';
import { ToastrService } from 'ngx-toastr';
import { Employee } from '../../../types/business.model';
import { WorkSchedule } from '../../../types/schedule.model';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-schedule-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './schedule-modal.html',
})
export class ScheduleModalComponent implements OnInit, OnDestroy {
  @Input() employee!: Employee;
  @Output() closed = new EventEmitter<void>();

  private fb = inject(FormBuilder);
  private scheduleService = inject(ScheduleService);
  private toastr = inject(ToastrService);

  private valueChangesSub = new Subscription();

  scheduleForm!: FormGroup;
  daysOfWeek = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];
  isLoading = true;
  isSaving = false;

  get dayForms() {
    return this.scheduleForm.get('days') as FormArray;
  }

  ngOnInit(): void {
    this.scheduleForm = this.fb.group({
      days: this.fb.array([])
    });

    this.scheduleService.getSchedule(this.employee.id).subscribe(schedules => {
      this.populateForm(schedules);
      this.isLoading = false;
    });
  }

  populateForm(schedules: WorkSchedule[]): void {
    this.daysOfWeek.forEach(dayName => {
      const scheduleForDay = schedules.find(s => s.dayOfWeek.toLowerCase() === dayName.toLowerCase());
      
      const dayGroup = this.fb.group({
        dayOfWeek: [dayName],
        startTime: [{ value: scheduleForDay?.startTime || '09:00', disabled: scheduleForDay?.isDayOff || false }],
        endTime: [{ value: scheduleForDay?.endTime || '17:00', disabled: scheduleForDay?.isDayOff || false }],
        isDayOff: [scheduleForDay?.isDayOff || false]
      });

      const sub = dayGroup.get('isDayOff')?.valueChanges.subscribe(isOff => {
        const startTimeCtrl = dayGroup.get('startTime');
        const endTimeCtrl = dayGroup.get('endTime');

        if (isOff) {
          startTimeCtrl?.disable();
          endTimeCtrl?.disable();
        } else {
          startTimeCtrl?.enable();
          endTimeCtrl?.enable();
        }
      });
      this.valueChangesSub.add(sub);

      this.dayForms.push(dayGroup);
    });
  }

  onSubmit(): void {
    this.isSaving = true;
    const scheduleData = this.dayForms.getRawValue().map((day: any) => ({
      ...day,
      startTime: day.isDayOff ? null : day.startTime,
      endTime: day.isDayOff ? null : day.endTime,
    }));

    this.scheduleService.updateSchedule(this.employee.id, scheduleData).subscribe({
      next: () => {
        this.toastr.success('Grafik pracy został zaktualizowany.');
        this.isSaving = false;
        this.closed.emit();
      },
      error: () => {
        this.toastr.error('Wystąpił błąd podczas zapisu grafiku.');
        this.isSaving = false;
      }
    });
  }

  ngOnDestroy(): void {
    this.valueChangesSub.unsubscribe();
  }
}