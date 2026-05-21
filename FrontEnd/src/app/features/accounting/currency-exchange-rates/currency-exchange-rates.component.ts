import { Component, OnInit } from '@angular/core';
import { CurrencyExchangeRateService, CurrencyExchangeRate } from '../../../core/services/currency-exchange-rate.service';
import { CurrencyService, Currency } from '../../../core/services/currency.service';
import { SHARED_IMPORTS } from '../../../shared/shared.imports';
import { MessageService, ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-currency-exchange-rates',
  standalone: true,
  imports: [...SHARED_IMPORTS],
  providers: [MessageService, ConfirmationService],
  templateUrl: './currency-exchange-rates.component.html',
})
export class CurrencyExchangeRatesComponent implements OnInit {
  exchangeRates: CurrencyExchangeRate[] = [];
  currencies: Currency[] = [];
  isLoading = false;

  // Dialog state
  dialogVisible = false;
  isEditMode = false;
  currentRate: Partial<CurrencyExchangeRate> = {};

  constructor(
    private exchangeRateService: CurrencyExchangeRateService,
    private currencyService: CurrencyService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit() {
    this.loadExchangeRates();
    this.loadCurrencies();
  }

  loadExchangeRates() {
    this.isLoading = true;
    this.exchangeRateService.getAll(1, 1000).subscribe({
      next: (res) => {
        this.exchangeRates = res.items || [];
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل تحميل أسعار الصرف',
        });
      },
    });
  }

  loadCurrencies() {
    this.currencyService.getAll(1, 100).subscribe({
      next: (res) => {
        this.currencies = res.items || [];
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل تحميل العملات المتاحة',
        });
      },
    });
  }

  getCurrencyName(currencyId: string): string {
    const currency = this.currencies.find(c => c.id === currencyId);
    return currency ? `${currency.name} (${currency.code})` : 'عملة غير معروفة';
  }

  openAddDialog() {
    this.isEditMode = false;
    const today = new Date();
    const formattedDate = today.toISOString().split('T')[0];
    this.currentRate = {
      currencyId: '',
      rate: undefined,
      effectiveDate: formattedDate
    };
    this.dialogVisible = true;
  }

  openEditDialog(rate: CurrencyExchangeRate) {
    this.isEditMode = true;
    let formattedDate = '';
    if (rate.effectiveDate) {
      formattedDate = rate.effectiveDate.split('T')[0];
    }
    this.currentRate = {
      ...rate,
      effectiveDate: formattedDate
    };
    this.dialogVisible = true;
  }

  closeDialog() {
    this.dialogVisible = false;
  }

  saveExchangeRate() {
    if (!this.currentRate.currencyId || this.currentRate.rate === undefined || !this.currentRate.effectiveDate) {
      this.messageService.add({
        severity: 'warn',
        summary: 'تنبيه',
        detail: 'يرجى تعبئة جميع الحقول الإلزامية',
      });
      return;
    }

    if (this.currentRate.rate <= 0) {
      this.messageService.add({
        severity: 'warn',
        summary: 'تنبيه',
        detail: 'سعر الصرف يجب أن يكون أكبر من الصفر',
      });
      return;
    }

    const payload = {
      ...this.currentRate,
      effectiveDate: new Date(this.currentRate.effectiveDate).toISOString()
    };

    if (this.isEditMode && this.currentRate.id) {
      this.exchangeRateService.update(this.currentRate.id, payload).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم تعديل سعر الصرف بنجاح' });
          this.loadExchangeRates();
          this.closeDialog();
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: err.error?.message || 'فشل تعديل سعر الصرف'
          });
        },
      });
    } else {
      this.exchangeRateService.create(payload).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم إضافة سعر الصرف بنجاح' });
          this.loadExchangeRates();
          this.closeDialog();
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: err.error?.message || 'فشل إضافة سعر الصرف'
          });
        },
      });
    }
  }

  confirmDelete(rate: CurrencyExchangeRate) {
    this.confirmationService.confirm({
      message: `هل أنت متأكد من حذف سعر الصرف المحدد؟`,
      header: 'تأكيد الحذف',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'حذف',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.exchangeRateService.delete(rate.id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم حذف سعر الصرف بنجاح' });
            this.loadExchangeRates();
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: 'خطأ',
              detail: err.error?.message || 'فشل حذف سعر الصرف'
            });
          },
        });
      },
    });
  }
}
