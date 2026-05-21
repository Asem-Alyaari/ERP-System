import { Component, OnInit } from '@angular/core';
import { CurrencyService, Currency } from '../../../core/services/currency.service';
import { SHARED_IMPORTS } from '../../../shared/shared.imports';
import { MessageService, ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-currencies',
  standalone: true,
  imports: [...SHARED_IMPORTS],
  providers: [MessageService, ConfirmationService],
  templateUrl: './currencies.component.html',
})
export class CurrenciesComponent implements OnInit {
  currencies: Currency[] = [];
  isLoading = false;

  // Dialog state
  dialogVisible = false;
  isEditMode = false;
  currentCurrency: Partial<Currency> = {};

  constructor(
    private currencyService: CurrencyService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit() {
    this.loadCurrencies();
  }

  loadCurrencies() {
    this.isLoading = true;
    this.currencyService.getAll(1, 100).subscribe({
      next: (res) => {
        this.currencies = res.items || [];
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل تحميل العملات',
        });
      },
    });
  }

  openAddDialog() {
    this.isEditMode = false;
    this.currentCurrency = { code: '', name: '', symbol: '', isLocal: false };
    this.dialogVisible = true;
  }

  openEditDialog(currency: Currency) {
    this.isEditMode = true;
    this.currentCurrency = { ...currency };
    this.dialogVisible = true;
  }

  closeDialog() {
    this.dialogVisible = false;
  }

  saveCurrency() {
    if (!this.currentCurrency.code || !this.currentCurrency.name || !this.currentCurrency.symbol) {
      this.messageService.add({
        severity: 'warn',
        summary: 'تنبيه',
        detail: 'يرجى تعبئة جميع الحقول الإلزامية',
      });
      return;
    }

    if (this.isEditMode && this.currentCurrency.id) {
      this.currencyService.update(this.currentCurrency.id, this.currentCurrency).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم تعديل العملة بنجاح' });
          this.loadCurrencies();
          this.closeDialog();
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: err.error?.message || 'فشل تعديل العملة (قد تكون هناك عملة محلية أخرى)'
          });
        },
      });
    } else {
      this.currencyService.create(this.currentCurrency).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم إضافة العملة بنجاح' });
          this.loadCurrencies();
          this.closeDialog();
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: err.error?.message || 'فشل إضافة العملة (قد تكون هناك عملة محلية أخرى)'
          });
        },
      });
    }
  }

  confirmDelete(currency: Currency) {
    this.confirmationService.confirm({
      message: `هل أنت متأكد من حذف العملة "${currency.name}"؟`,
      header: 'تأكيد الحذف',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'حذف',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.currencyService.delete(currency.id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم حذف العملة بنجاح' });
            this.loadCurrencies();
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: 'خطأ',
              detail: err.error?.message || 'فشل حذف العملة (قد تكون عملة محلية أو مرتبطة بقيود مالية)'
            });
          },
        });
      },
    });
  }
}
