import { Component, OnInit } from '@angular/core';
import { SHARED_IMPORTS } from '../../../shared/shared.imports';
import { MessageService, ConfirmationService } from 'primeng/api';
import {
  JournalEntryService,
  JournalEntryListItem,
  JournalEntryDetail,
  JournalEntryLineDto,
  CreateJournalEntryCommand,
  JournalEntryStatus
} from '../../../core/services/journal-entry.service';
import { FiscalPeriodService, FiscalPeriod } from '../../../core/services/fiscal-period.service';
import { AccountService, AccountLookup } from '../../../core/services/account.service';
import { CurrencyService, Currency } from '../../../core/services/currency.service';
import { forkJoin } from 'rxjs';
import { CostCenterService, CostCenterLookup } from '../../../core/services/cost-center.service';

export interface JournalLineForm {
  accountId: string;
  accountDisplay: string;
  debit: number;
  credit: number;
  currencyId: string;
  exchangeRate: number;
  foreignDebit: number | null;
  foreignCredit: number | null;
  costCenterId: string | null;
  memo: string;
  costCenterStatus?: 'Required' | 'Optional' | 'Disabled';
}

@Component({
  selector: 'app-journal-entries',
  standalone: true,
  imports: [...SHARED_IMPORTS],
  providers: [MessageService, ConfirmationService],
  templateUrl: './journal-entries.component.html',
  styleUrl: './journal-entries.component.scss'
})
export class JournalEntriesComponent implements OnInit {

  // ── List state ──────────────────────────────────────────────
  entries: JournalEntryListItem[] = [];
  totalCount = 0;
  pageNumber = 1;
  pageSize = 10;
  isLoading = false;

  // Filters
  searchTerm = '';
  filterStatus: JournalEntryStatus | null = null;
  filterFiscalPeriodId: string | null = null;
  filterStartDate: Date | null = null;
  filterEndDate: Date | null = null;

  // Status options
  statusOptions = [
    { label: 'الكل', value: null },
    { label: 'مسودة', value: JournalEntryStatus.Draft },
    { label: 'مرحّل', value: JournalEntryStatus.Posted },
    { label: 'ملغي', value: JournalEntryStatus.Cancelled }
  ];
  JournalEntryStatus = JournalEntryStatus;

  // ── Lookup data ──────────────────────────────────────────────
  fiscalPeriods: FiscalPeriod[] = [];
  accounts: AccountLookup[] = [];
  currencies: Currency[] = [];
  costCenters: CostCenterLookup[] = [];
  localCurrency: Currency | null = null;

  // ── Create Dialog ────────────────────────────────────────────
  createDialogVisible = false;
  isSaving = false;

  newEntry: Partial<CreateJournalEntryCommand> = {};
  entryDate: Date = new Date();
  lines: JournalLineForm[] = [];

  // ── Detail Drawer ────────────────────────────────────────────
  detailDrawerVisible = false;
  selectedEntry: JournalEntryDetail | null = null;
  isLoadingDetail = false;

  // ── Username (temp) ──────────────────────────────────────────
  currentUser = 'admin';

  constructor(
    private journalEntryService: JournalEntryService,
    private fiscalPeriodService: FiscalPeriodService,
    private accountService: AccountService,
    private currencyService: CurrencyService,
    private costCenterService: CostCenterService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.loadLookups();
    this.loadEntries();
  }

  // ── Lookups ──────────────────────────────────────────────────

  loadLookups(): void {
    forkJoin({
      periods: this.fiscalPeriodService.getAll(),
      accounts: this.accountService.getDetailAccounts(),
      currencies: this.currencyService.getList(),
      costCenters: this.costCenterService.getDetailCostCenters()
    }).subscribe({
      next: ({ periods, accounts, currencies, costCenters }) => {
        this.fiscalPeriods = periods || [];
        this.accounts = accounts || [];
        this.currencies = currencies || [];
        this.costCenters = costCenters || [];
        this.localCurrency = currencies.find(c => c.isLocal) ?? currencies[0] ?? null;

        // تحذير إذا لم تكن هناك مراكز تكلفة
        if (!this.costCenters || this.costCenters.length === 0) {
          console.warn('No cost centers loaded');
        }
      },
      error: (err) => {
        console.error('Failed to load lookups:', err);
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل البيانات الأساسية' });
      }
    });
  }

  // ── List & Pagination ────────────────────────────────────────

  loadEntries(): void {
    this.isLoading = true;
    this.journalEntryService.getAll(
      this.pageNumber,
      this.pageSize,
      this.searchTerm,
      this.filterStatus,
      this.filterFiscalPeriodId,
      this.filterStartDate ? this.formatDate(this.filterStartDate) : null,
      this.filterEndDate ? this.formatDate(this.filterEndDate) : null
    ).subscribe({
      next: (res) => {
        this.entries = res.items || [];
        this.totalCount = res.totalCount || 0;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل القيود اليومية' });
      }
    });
  }

  onPageChange(event: any): void {
    this.pageNumber = Math.floor(event.first / event.rows) + 1;
    this.pageSize = event.rows;
    this.loadEntries();
  }

  onSearch(): void {
    this.pageNumber = 1;
    this.loadEntries();
  }

  resetFilters(): void {
    this.searchTerm = '';
    this.filterStatus = null;
    this.filterFiscalPeriodId = null;
    this.filterStartDate = null;
    this.filterEndDate = null;
    this.pageNumber = 1;
    this.loadEntries();
  }

  // ── Detail Drawer ─────────────────────────────────────────────

  openDetail(entry: JournalEntryListItem): void {
    this.detailDrawerVisible = true;
    this.selectedEntry = null;
    this.isLoadingDetail = true;
    this.journalEntryService.getById(entry.id).subscribe({
      next: (detail) => {
        this.selectedEntry = detail;
        this.isLoadingDetail = false;
      },
      error: () => {
        this.isLoadingDetail = false;
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل تفاصيل القيد' });
      }
    });
  }

  // ── Create Dialog ─────────────────────────────────────────────

  openCreateDialog(): void {
    const openPeriod = this.fiscalPeriods.find(p => !p.isClosed);
    this.newEntry = {
      voucherNumber: this.generateVoucherNumber(),
      fiscalPeriodId: openPeriod?.id ?? '',
      createdBy: this.currentUser,
      description: ''
    };
    this.entryDate = new Date();
    this.lines = [];
    this.addLine();
    this.addLine();
    this.createDialogVisible = true;
  }

  generateVoucherNumber(): string {
    const now = new Date();
    const y = now.getFullYear();
    const m = String(now.getMonth() + 1).padStart(2, '0');
    const d = String(now.getDate()).padStart(2, '0');
    const rand = String(Math.floor(Math.random() * 9000) + 1000);
    return `JV-${y}${m}${d}-${rand}`;
  }

  addLine(): void {
    this.lines.push({
      accountId: '',
      accountDisplay: '',
      debit: 0,
      credit: 0,
      currencyId: this.localCurrency?.id ?? '',
      exchangeRate: 1,
      foreignDebit: null,
      foreignCredit: null,
      costCenterId: null,
      memo: '',
      costCenterStatus: 'Optional'
    });
  }

  removeLine(index: number): void {
    if (this.lines.length > 2) {
      this.lines.splice(index, 1);
    } else {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يجب أن يحتوي القيد على سطرين على الأقل' });
    }
  }

  onAccountChange(line: JournalLineForm, accountId: string): void {
    const acc = this.accounts.find(a => a.id === accountId);
    line.accountDisplay = acc ? `${acc.accountCode} - ${acc.accountNameAr}` : '';
    // تحديث حالة مركز التكلفة بناءً على الحساب المختار
    // التأكد من أن القيمة من النوع الصحيح
    const status = acc?.costCenterStatus || 'Optional';
    line.costCenterStatus = (status === 'Required' || status === 'Optional' || status === 'Disabled')
      ? status
      : 'Optional';
    // إذا كان مركز التكلفة معطل، مسح القيمة المختارة
    if (line.costCenterStatus === 'Disabled') {
      line.costCenterId = null;
    }
  }

  onCurrencyChange(line: JournalLineForm, currencyId: string): void {
    const curr = this.currencies.find(c => c.id === currencyId);
    line.exchangeRate = (curr?.isLocal) ? 1 : line.exchangeRate;
  }

  get totalDebit(): number {
    return this.lines.reduce((s, l) => s + (l.debit || 0), 0);
  }

  get totalCredit(): number {
    return this.lines.reduce((s, l) => s + (l.credit || 0), 0);
  }

  get isBalanced(): boolean {
    return Math.abs(this.totalDebit - this.totalCredit) < 0.001 && this.totalDebit > 0;
  }

  saveEntry(): void {
    if (!this.newEntry.fiscalPeriodId) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى اختيار الفترة المالية' });
      return;
    }
    if (!this.isBalanced) {
      this.messageService.add({ severity: 'warn', summary: 'قيد غير متوازن', detail: `إجمالي المدين: ${this.totalDebit.toFixed(2)}، إجمالي الدائن: ${this.totalCredit.toFixed(2)}` });
      return;
    }
    const hasEmptyAccount = this.lines.some(l => !l.accountId);
    if (hasEmptyAccount) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى اختيار حساب لجميع الأسطر' });
      return;
    }

    // التحقق من مراكز التكلفة الإلزامية
    const missingRequiredCostCenters = this.lines.filter(
      l => l.costCenterStatus === 'Required' && (!l.costCenterId || l.costCenterId === '')
    );
    if (missingRequiredCostCenters.length > 0) {
      const accNames = missingRequiredCostCenters
        .map(l => l.accountDisplay)
        .filter(name => name)
        .join('، ');
      this.messageService.add({
        severity: 'warn',
        summary: 'مراكز تكلفة مفقودة',
        detail: `يرجى تحديد مركز تكلفة للحسابات التالية: ${accNames}`
      });
      return;
    }

    const command: CreateJournalEntryCommand = {
      voucherNumber: this.newEntry.voucherNumber!,
      transactionDate: this.formatDate(this.entryDate),
      description: this.newEntry.description ?? '',
      fiscalPeriodId: this.newEntry.fiscalPeriodId!,
      createdBy: this.currentUser,
      lines: this.lines.map(l => ({
        accountId: l.accountId,
        debit: l.debit || 0,
        credit: l.credit || 0,
        currencyId: l.currencyId,
        exchangeRate: l.exchangeRate || 1,
        foreignDebit: l.foreignDebit ?? undefined,
        foreignCredit: l.foreignCredit ?? undefined,
        costCenterId: (l.costCenterId && l.costCenterId !== '') ? l.costCenterId : undefined,
        memo: l.memo || undefined
      } as JournalEntryLineDto))
    };

    this.isSaving = true;
    this.journalEntryService.create(command).subscribe({
      next: () => {
        this.isSaving = false;
        this.createDialogVisible = false;
        this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم حفظ القيد اليومي كمسودة بنجاح' });
        this.loadEntries();
      },
      error: (err) => {
        this.isSaving = false;
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل حفظ القيد اليومي' });
      }
    });
  }

  // ── Post / Unpost ─────────────────────────────────────────────

  confirmPost(entry: JournalEntryListItem): void {
    this.confirmationService.confirm({
      message: `هل تريد ترحيل القيد رقم "${entry.voucherNumber}"؟ بعد الترحيل سيتم تحديث الأرصدة المحاسبية.`,
      header: 'تأكيد الترحيل',
      icon: 'pi pi-send',
      acceptLabel: 'ترحيل',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-success',
      accept: () => {
        this.journalEntryService.post(entry.id, this.currentUser).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم ترحيل القيد بنجاح وتحديث الأرصدة' });
            this.loadEntries();
            if (this.selectedEntry?.id === entry.id) this.openDetail(entry);
          },
          error: (err) => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل ترحيل القيد' })
        });
      }
    });
  }

  confirmUnpost(entry: JournalEntryListItem): void {
    this.confirmationService.confirm({
      message: `هل تريد إلغاء ترحيل القيد رقم "${entry.voucherNumber}"؟ سيتم عكس تأثيره على الأرصدة.`,
      header: 'تأكيد إلغاء الترحيل',
      icon: 'pi pi-undo',
      acceptLabel: 'إلغاء الترحيل',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-warning',
      accept: () => {
        this.journalEntryService.unpost(entry.id, this.currentUser).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم إلغاء ترحيل القيد وعكس الأرصدة' });
            this.loadEntries();
            if (this.selectedEntry?.id === entry.id) this.openDetail(entry);
          },
          error: (err) => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل إلغاء الترحيل' })
        });
      }
    });
  }

  // ── Helpers ───────────────────────────────────────────────────

  formatDate(date: Date): string {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }

  getStatusLabel(status: JournalEntryStatus): string {
    switch (status) {
      case JournalEntryStatus.Draft: return 'مسودة';
      case JournalEntryStatus.Posted: return 'مرحّل';
      case JournalEntryStatus.Cancelled: return 'ملغي';
      default: return '';
    }
  }

  getStatusSeverity(status: JournalEntryStatus): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' {
    switch (status) {
      case JournalEntryStatus.Draft: return 'warn';
      case JournalEntryStatus.Posted: return 'success';
      case JournalEntryStatus.Cancelled: return 'danger';
      default: return 'secondary';
    }
  }

  // Helpers for the detail drawer (avoids `as any` in templates)
  confirmPostSelected(): void {
    if (!this.selectedEntry) return;
    this.confirmPost(this.selectedEntry as unknown as JournalEntryListItem);
  }

  confirmUnpostSelected(): void {
    if (!this.selectedEntry) return;
    this.confirmUnpost(this.selectedEntry as unknown as JournalEntryListItem);
  }

  getDetailTotalDebit(entry: JournalEntryDetail): number {
    return entry.lines.reduce((s, l) => s + l.debit, 0);
  }

  getDetailTotalCredit(entry: JournalEntryDetail): number {
    return entry.lines.reduce((s, l) => s + l.credit, 0);
  }

  // ── Cost Center Helpers ─────────────────────────────────────────

  getCostCenterStatusClass(status: string): string {
    switch (status) {
      case 'Required': return 'cc-status-required';
      case 'Optional': return 'cc-status-optional';
      case 'Disabled': return 'cc-status-disabled';
      default: return '';
    }
  }

  getCostCenterStatusLabel(status: string): string {
    switch (status) {
      case 'Required': return 'إلزامي';
      case 'Optional': return 'اختياري';
      case 'Disabled': return 'معطل';
      default: return '';
    }
  }

  isCostCenterRequired(status: string): boolean {
    return status === 'Required';
  }

  isCostCenterDisabled(status: string): boolean {
    return status === 'Disabled';
  }
}
