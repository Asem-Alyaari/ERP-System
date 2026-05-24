import { Component, OnInit } from '@angular/core';
import { SHARED_IMPORTS } from '../../../shared/shared.imports';
import { MessageService } from 'primeng/api';
import { LedgerService, LedgerReport, LedgerTransaction } from '../../../core/services/ledger.service';
import { AccountService, AccountLookup } from '../../../core/services/account.service';
import { CostCenterService, CostCenterLookup } from '../../../core/services/cost-center.service';

@Component({
  selector: 'app-ledger-report',
  standalone: true,
  imports: [...SHARED_IMPORTS],
  providers: [MessageService],
  templateUrl: './ledger-report.component.html',
  styleUrl: './ledger-report.component.scss'
})
export class LedgerReportComponent implements OnInit {

  // ── Filter State ──────────────────────────────────────────────
  selectedAccountId: string | null = null;
  selectedCostCenterId: string | null = null;
  fromDate: Date | null = null;
  toDate: Date | null = null;

  // ── Lookup Data ────────────────────────────────────────────────
  accounts: AccountLookup[] = [];
  costCenters: CostCenterLookup[] = [];
  isLoadingLookups = false;

  // ── Report Data ────────────────────────────────────────────────
  report: LedgerReport | null = null;
  isLoadingReport = false;
  hasGeneratedReport = false;

  // ── Export State ──────────────────────────────────────────────
  isExporting = false;

  constructor(
    private ledgerService: LedgerService,
    private accountService: AccountService,
    private costCenterService: CostCenterService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.loadLookups();
    this.setDefaultDateRange();
  }

  // ── Load Lookup Data ────────────────────────────────────────────

  loadLookups(): void {
    this.isLoadingLookups = true;
    this.accountService.getDetailAccounts().subscribe({
      next: (accounts) => {
        this.accounts = accounts || [];
        this.costCenterService.getDetailCostCenters().subscribe({
          next: (costCenters) => {
            this.costCenters = costCenters || [];
            this.isLoadingLookups = false;
          },
          error: () => {
            this.isLoadingLookups = false;
            this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل بيانات مراكز التكلفة' });
          }
        });
      },
      error: () => {
        this.isLoadingLookups = false;
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل بيانات الحسابات' });
      }
    });
  }

  setDefaultDateRange(): void {
    const today = new Date();
    const firstDayOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);
    this.fromDate = firstDayOfMonth;
    this.toDate = today;
  }

  // ── Generate Report ─────────────────────────────────────────────

  generateReport(): void {
    if (!this.selectedAccountId) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى اختيار الحساب' });
      return;
    }

    this.isLoadingReport = true;
    this.hasGeneratedReport = false;

    const fromDateStr = this.fromDate ? this.formatDate(this.fromDate) : undefined;
    const toDateStr = this.toDate ? this.formatDate(this.toDate) : undefined;

    this.ledgerService.getReport(
      this.selectedAccountId,
      this.selectedCostCenterId || undefined,
      fromDateStr,
      toDateStr
    ).subscribe({
      next: (report) => {
        this.report = report;
        this.hasGeneratedReport = true;
        this.isLoadingReport = false;
      },
      error: (err) => {
        this.isLoadingReport = false;
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: err.error?.message || 'فشل تحميل تقرير كشف الحساب'
        });
      }
    });
  }

  // ── Export Report ──────────────────────────────────────────────

  exportReport(): void {
    if (!this.selectedAccountId) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى اختيار الحساب أولاً' });
      return;
    }

    this.isExporting = true;

    const fromDateStr = this.fromDate ? this.formatDate(this.fromDate) : undefined;
    const toDateStr = this.toDate ? this.formatDate(this.toDate) : undefined;

    this.ledgerService.exportReport(
      this.selectedAccountId,
      this.selectedCostCenterId || undefined,
      fromDateStr,
      toDateStr
    ).subscribe({
      next: (blob) => {
        const account = this.accounts.find(a => a.id === this.selectedAccountId);
        const fileName = `كشف_حساب_${account?.accountNameAr || 'حساب'}_${this.formatDate(new Date())}.pdf`;
        
        // Create download link using browser's native capability
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
        
        this.isExporting = false;
        this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم تصدير التقرير بنجاح' });
      },
      error: (err) => {
        this.isExporting = false;
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: err.error?.message || 'فشل تصدير التقرير'
        });
      }
    });
  }

  // ── Reset Filters ──────────────────────────────────────────────

  resetFilters(): void {
    this.selectedAccountId = null;
    this.selectedCostCenterId = null;
    this.setDefaultDateRange();
    this.report = null;
    this.hasGeneratedReport = false;
  }

  // ── Helpers ────────────────────────────────────────────────────

  formatDate(date: Date): string {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('ar-SA', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(value);
  }

  getAccountDisplay(accountId: string): string {
    const account = this.accounts.find(a => a.id === accountId);
    return account ? `${account.accountCode} - ${account.accountNameAr}` : '';
  }

  getCostCenterDisplay(costCenterId: string): string {
    const costCenter = this.costCenters.find(c => c.id === costCenterId);
    return costCenter ? `${costCenter.costCenterCode} - ${costCenter.costCenterNameAr}` : '';
  }

  // ── Summary Card Helpers ────────────────────────────────────────

  get summaryCards() {
    if (!this.report) return [];
    return [
      {
        label: 'الرصيد الافتتاحي',
        value: this.report.summary.openingBalance,
        icon: 'pi pi-wallet',
        color: 'blue'
      },
      {
        label: 'إجمالي المدين',
        value: this.report.summary.totalDebit,
        icon: 'pi pi-arrow-down',
        color: 'red'
      },
      {
        label: 'إجمالي الدائن',
        value: this.report.summary.totalCredit,
        icon: 'pi pi-arrow-up',
        color: 'green'
      },
      {
        label: 'الرصيد النهائي',
        value: this.report.summary.closingBalance,
        icon: 'pi pi-check-circle',
        color: this.report.summary.closingBalance >= 0 ? 'green' : 'red'
      }
    ];
  }

  getSummaryCardClass(color: string): string {
    const colorMap: { [key: string]: string } = {
      blue: 'gl-card-blue',
      red: 'gl-card-red',
      green: 'gl-card-green',
      yellow: 'gl-card-yellow'
    };
    return colorMap[color] || 'gl-card-blue';
  }
}
