import { Component, OnInit } from '@angular/core';
import { FiscalPeriodService, FiscalPeriod } from '../../../core/services/fiscal-period.service';
import { SHARED_IMPORTS } from '../../../shared/shared.imports';
import { MessageService, ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-fiscal-periods',
  standalone: true,
  imports: [...SHARED_IMPORTS],
  providers: [MessageService, ConfirmationService],
  templateUrl: './fiscal-periods.component.html',
  styleUrl: './fiscal-periods.component.scss'
})
export class FiscalPeriodsComponent implements OnInit {
  periods: FiscalPeriod[] = [];
  isLoading = false;

  // Dialog state
  dialogVisible = false;
  currentPeriod: Partial<FiscalPeriod> = {};

  constructor(
    private fiscalPeriodService: FiscalPeriodService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit() {
    this.loadPeriods();
  }

  loadPeriods() {
    this.isLoading = true;
    this.fiscalPeriodService.getAll().subscribe({
      next: (res) => {
        this.periods = res || [];
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل تحميل الفترات المالية'
        });
      }
    });
  }

  openAddDialog() {
    // Smart Suggestion for the next fiscal period!
    let suggestedYear = '';
    let suggestedStart = '';
    let suggestedEnd = '';

    if (this.periods.length > 0) {
      // Find the latest period by end date
      const sortedPeriods = [...this.periods].sort(
        (a, b) => new Date(a.endDate).getTime() - new Date(b.endDate).getTime()
      );
      const latestPeriod = sortedPeriods[sortedPeriods.length - 1];
      
      const latestEndDate = new Date(latestPeriod.endDate);
      
      // Suggest start date = end date + 1 day
      const nextStartDate = new Date(latestEndDate);
      nextStartDate.setDate(nextStartDate.getDate() + 1);
      
      // Suggest end date = start date + 1 year - 1 day
      const nextEndDate = new Date(nextStartDate);
      nextEndDate.setFullYear(nextEndDate.getFullYear() + 1);
      nextEndDate.setDate(nextEndDate.getDate() - 1);

      suggestedStart = this.formatDate(nextStartDate);
      suggestedEnd = this.formatDate(nextEndDate);

      // Try to parse year and increment
      const parsedYear = parseInt(latestPeriod.yearName, 10);
      if (!isNaN(parsedYear)) {
        suggestedYear = (parsedYear + 1).toString();
      } else {
        suggestedYear = nextStartDate.getFullYear().toString();
      }
    } else {
      // Default to current calendar year
      const currentYear = new Date().getFullYear();
      suggestedYear = currentYear.toString();
      suggestedStart = `${currentYear}-01-01`;
      suggestedEnd = `${currentYear}-12-31`;
    }

    this.currentPeriod = {
      yearName: suggestedYear,
      startDate: suggestedStart,
      endDate: suggestedEnd
    };
    this.dialogVisible = true;
  }

  formatDate(date: Date): string {
    const yyyy = date.getFullYear();
    const mm = String(date.getMonth() + 1).padStart(2, '0');
    const dd = String(date.getDate()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
  }

  closeDialog() {
    this.dialogVisible = false;
  }

  savePeriod() {
    if (!this.currentPeriod.yearName || !this.currentPeriod.startDate || !this.currentPeriod.endDate) {
      this.messageService.add({
        severity: 'warn',
        summary: 'تنبيه',
        detail: 'يرجى تعبئة جميع الحقول الإلزامية'
      });
      return;
    }

    this.fiscalPeriodService.create(this.currentPeriod).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'تم',
          detail: 'تم إنشاء الفترة المالية بنجاح'
        });
        this.loadPeriods();
        this.closeDialog();
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: err.error?.message || 'فشل إنشاء الفترة المالية.'
        });
      }
    });
  }

  togglePeriodStatus(period: FiscalPeriod) {
    const actionText = period.isClosed ? 'فتح الفترة' : 'إغلاق الفترة';
    const confirmMessage = period.isClosed 
      ? `هل أنت متأكد من إعادة فتح السنة المالية "${period.yearName}"؟`
      : `هل أنت متأكد من إغلاق السنة المالية "${period.yearName}"؟ عند الإغلاق، سيتم قفل إدخال أي قيود يومية أو حركات محاسبية في هذه الفترة.`;

    this.confirmationService.confirm({
      message: confirmMessage,
      header: 'تأكيد تغيير حالة الفترة',
      icon: period.isClosed ? 'pi pi-lock-open' : 'pi pi-lock',
      acceptLabel: actionText,
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: period.isClosed ? 'p-button-success' : 'p-button-warning',
      accept: () => {
        const req = period.isClosed 
          ? this.fiscalPeriodService.open(period.id)
          : this.fiscalPeriodService.close(period.id);

        req.subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'تم',
              detail: `تم ${period.isClosed ? 'فتح' : 'إغلاق'} الفترة المالية بنجاح`
            });
            this.loadPeriods();
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: 'خطأ',
              detail: err.error?.message || 'فشل تغيير حالة الفترة المالية.'
            });
          }
        });
      }
    });
  }

  confirmDelete(period: FiscalPeriod) {
    this.confirmationService.confirm({
      message: `هل أنت متأكد من حذف السنة المالية "${period.yearName}"؟`,
      header: 'تأكيد الحذف',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'حذف',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.fiscalPeriodService.delete(period.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'تم',
              detail: 'تم حذف الفترة المالية بنجاح'
            });
            this.loadPeriods();
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: 'خطأ',
              detail: err.error?.message || 'فشل حذف الفترة المالية.'
            });
          }
        });
      }
    });
  }
}
