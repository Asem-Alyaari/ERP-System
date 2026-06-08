import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MessageService, ConfirmationService } from 'primeng/api';
import { SHARED_IMPORTS } from '../../../shared/shared.imports';
import { FiscalPeriodService, FiscalPeriod } from '../../../core/services/fiscal-period.service';
import { OpeningBalancesService, OpeningBalance as OpeningBalanceDto } from '../../../core/services/opening-balances.service';
import { AccountService, AccountDto } from '../../../core/services/account.service';

interface OpeningBalance {
  accountId: string;
  accountCode: string;
  accountNameAr: string;
  debit: number;
  credit: number;
}

@Component({
  selector: 'app-opening-balances',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ...SHARED_IMPORTS
  ],
  templateUrl: './opening-balances.component.html',
  styleUrls: ['./opening-balances.component.scss']
})
export class OpeningBalancesComponent implements OnInit {
  fiscalPeriods: FiscalPeriod[] = [];
  accounts: AccountDto[] = [];
  selectedFiscalPeriodId: string | null = null;
  balances: OpeningBalance[] = [];
  loading = false;
  saving = false;

  constructor(
    private fiscalPeriodService: FiscalPeriodService,
    private openingBalancesService: OpeningBalancesService,
    private accountService: AccountService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.loadFiscalPeriods();
    this.loadAccounts();
  }

  loadFiscalPeriods(): void {
    this.fiscalPeriodService.getAll().subscribe(data => {
      this.fiscalPeriods = data.filter(p => !p.isClosed);
    });
  }

  loadAccounts(): void {
    this.accountService.getAll().subscribe(data => {
      this.accounts = data.filter(a => a.isDetail);
    });
  }

  onFiscalPeriodChange(): void {
    if (this.selectedFiscalPeriodId) {
      this.initializeBalances();
    } else {
      this.balances = [];
    }
  }

  initializeBalances(): void {
    this.balances = this.accounts.map(account => ({
      accountId: account.id,
      accountCode: account.accountCode,
      accountNameAr: account.accountNameAr,
      debit: 0,
      credit: 0
    }));
  }

  get totalDebit(): number {
    return this.balances.reduce((sum, b) => sum + (b.debit || 0), 0);
  }

  get totalCredit(): number {
    return this.balances.reduce((sum, b) => sum + (b.credit || 0), 0);
  }

  get isBalanced(): boolean {
    return Math.abs(this.totalDebit - this.totalCredit) < 0.01 && this.totalDebit > 0;
  }

  saveBalances(): void {
    if (!this.selectedFiscalPeriodId) {
      this.messageService.add({
        severity: 'warn',
        summary: 'تحذير',
        detail: 'يرجى اختيار الفترة المالية'
      });
      return;
    }

    if (!this.isBalanced) {
      this.messageService.add({
        severity: 'warn',
        summary: 'تحذير',
        detail: 'الأرصدة غير متوازنة. إجمالي المدين يجب أن يساوي إجمالي الدائن'
      });
      return;
    }

    this.confirmationService.confirm({
      message: 'هل أنت متأكد من حفظ الأرصدة الافتتاحية؟',
      header: 'تأكيد الحفظ',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.executeSave();
      }
    });
  }

  executeSave(): void {
    this.saving = true;

    const command = {
      fiscalPeriodId: this.selectedFiscalPeriodId!,
      balances: this.balances.filter(b => b.debit > 0 || b.credit > 0).map(b => ({
        accountId: b.accountId,
        debit: b.debit,
        credit: b.credit
      } as OpeningBalanceDto)),
      createdBy: 'System' // TODO: Get from auth context
    };

    this.openingBalancesService.setOpeningBalances(this.selectedFiscalPeriodId!, command)
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'نجح',
            detail: 'تم حفظ الأرصدة الافتتاحية بنجاح'
          });
          this.saving = false;
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: error.error?.message || 'حدث خطأ أثناء حفظ الأرصدة'
          });
          this.saving = false;
        }
      });
  }

  formatNumber(value: number): string {
    return new Intl.NumberFormat('ar-SA', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(value);
  }
}
