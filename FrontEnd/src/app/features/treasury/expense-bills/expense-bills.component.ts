import { Component, OnInit } from '@angular/core';
import { SHARED_IMPORTS } from '../../../shared/shared.imports';
import { MessageService, ConfirmationService } from 'primeng/api';
import {
  ExpenseBillService,
  ExpenseBillPaymentMethod,
  ExpenseBillStatus,
  CreateExpenseBillCommand,
  CreateExpenseBillLineCommand,
  ExpenseBillListItem
} from '../../../core/services/expense-bill.service';
import { AccountService, AccountLookup } from '../../../core/services/account.service';
import { CostCenterService, CostCenterLookup } from '../../../core/services/cost-center.service';
import { VendorService, Vendor } from '../../../core/services/vendor.service';
import { forkJoin } from 'rxjs';

export interface ExpenseBillLineForm {
  accountId: string;
  accountDisplay: string;
  amount: number;
  costCenterId: string;
  costCenterDisplay: string;
  notes: string;
}

export interface ExpenseBillForm {
  billNumber: string;
  transactionDate: Date;
  paymentMethod: ExpenseBillPaymentMethod;
  vendorId: string | null;
  vendorDisplay: string;
  supplierName: string;
  paymentAccountId: string | null;
  paymentAccountDisplay: string;
  totalAmount: number;
  taxAmount: number;
  netAmount: number;
  notes: string;
  lines: ExpenseBillLineForm[];
}

@Component({
  selector: 'app-expense-bills',
  standalone: true,
  imports: [...SHARED_IMPORTS],
  providers: [MessageService, ConfirmationService],
  templateUrl: './expense-bills.component.html',
  styleUrl: './expense-bills.component.scss'
})
export class ExpenseBillsComponent implements OnInit {
  // ── List state ──────────────────────────────────────────────
  bills: ExpenseBillListItem[] = [];
  isLoading = false;

  // ── Lookup data ──────────────────────────────────────────────
  cashAccounts: AccountLookup[] = [];
  bankAccounts: AccountLookup[] = [];
  expenseAccounts: AccountLookup[] = []; // Only 5xxx accounts
  vendors: Vendor[] = [];
  costCenters: CostCenterLookup[] = [];

  // ── Create Dialog ────────────────────────────────────────────
  createDialogVisible = false;
  isSaving = false;

  // ── View Details Dialog ───────────────────────────────────────
  viewDetailsDialogVisible = false;
  selectedBill: ExpenseBillListItem | null = null;

  expenseBillForm: ExpenseBillForm = {
    billNumber: '',
    transactionDate: new Date(),
    paymentMethod: ExpenseBillPaymentMethod.Cash,
    vendorId: null,
    vendorDisplay: '',
    supplierName: '',
    paymentAccountId: null,
    paymentAccountDisplay: '',
    totalAmount: 0,
    taxAmount: 0,
    netAmount: 0,
    notes: '',
    lines: []
  };

  // ── Enums for template ─────────────────────────────────────────
  ExpenseBillPaymentMethod = ExpenseBillPaymentMethod;
  ExpenseBillStatus = ExpenseBillStatus;

  // ── Payment Method Options ─────────────────────────────────────
  get paymentMethodOptions() {
    return [
      { label: 'نقدي', value: ExpenseBillPaymentMethod.Cash },
      { label: 'بنكي', value: ExpenseBillPaymentMethod.Bank },
      { label: 'آجل/ذمم', value: ExpenseBillPaymentMethod.Credit }
    ];
  }

  // ── Username (temp) ──────────────────────────────────────────
  currentUser = 'admin';

  constructor(
    private expenseBillService: ExpenseBillService,
    private accountService: AccountService,
    private costCenterService: CostCenterService,
    private vendorService: VendorService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.loadLookups();
    this.loadBills();
  }

  // ── Lookups ──────────────────────────────────────────────────

  loadLookups(): void {
    forkJoin({
      accounts: this.accountService.getDetailAccounts(),
      costCenters: this.costCenterService.getDetailCostCenters(),
      vendors: this.vendorService.getAllList()
    }).subscribe({
      next: ({ accounts, costCenters, vendors }) => {
        // تصفية الحسابات حسب النوع
        this.cashAccounts = accounts.filter(a => a.accountCode.startsWith('110101')); // الصندوق
        this.bankAccounts = accounts.filter(a => a.accountCode.startsWith('110102')); // البنوك
        // حسابات المصروفات فقط (5xxx)
        this.expenseAccounts = accounts.filter(a => a.accountCode.startsWith('5'));

        this.costCenters = costCenters || [];
        this.vendors = vendors || [];

        // تعيين افتراضي
        if (this.cashAccounts.length > 0) {
          this.expenseBillForm.paymentAccountId = this.cashAccounts[0].id;
          this.expenseBillForm.paymentAccountDisplay = `${this.cashAccounts[0].accountCode} - ${this.cashAccounts[0].accountNameAr}`;
        }
      },
      error: (err) => {
        console.error('Failed to load lookups:', err);
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل البيانات الأساسية' });
      }
    });
  }

  // ── Bills List ───────────────────────────────────────────────

  loadBills(): void {
    this.isLoading = true;
    this.expenseBillService.getAll().subscribe({
      next: (data) => {
        this.bills = data || [];
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load bills:', err);
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل فواتير المصروفات' });
        this.isLoading = false;
      }
    });
  }

  postBill(billId: string): void {
    this.expenseBillService.post(billId, this.currentUser).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم ترحيل الفاتورة بنجاح' });
        this.loadBills(); // Refresh the list
      },
      error: (err) => {
        console.error('Failed to post bill:', err);
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل ترحيل الفاتورة' });
      }
    });
  }

  // ── Form Actions ─────────────────────────────────────────────

  openCreateDialog(): void {
    this.expenseBillForm = {
      billNumber: this.generateBillNumber(),
      transactionDate: new Date(),
      paymentMethod: ExpenseBillPaymentMethod.Cash,
      vendorId: null,
      vendorDisplay: '',
      supplierName: '',
      paymentAccountId: this.cashAccounts[0]?.id || null,
      paymentAccountDisplay: this.cashAccounts[0] ? `${this.cashAccounts[0].accountCode} - ${this.cashAccounts[0].accountNameAr}` : '',
      totalAmount: 0,
      taxAmount: 0,
      netAmount: 0,
      notes: '',
      lines: []
    };
    this.createDialogVisible = true;
  }

  generateBillNumber(): string {
    const now = new Date();
    const y = now.getFullYear();
    const m = String(now.getMonth() + 1).padStart(2, '0');
    const d = String(now.getDate()).padStart(2, '0');
    const rand = String(Math.floor(Math.random() * 9000) + 1000);
    return `EB-${y}${m}${d}-${rand}`;
  }

  onPaymentMethodChange(event: any): void {
    const method = event.value;
    this.expenseBillForm.paymentMethod = method;
    this.expenseBillForm.vendorId = null;
    this.expenseBillForm.vendorDisplay = '';
    this.expenseBillForm.supplierName = '';
    this.expenseBillForm.paymentAccountId = null;
    this.expenseBillForm.paymentAccountDisplay = '';

    // Set default payment account based on method
    if (method === ExpenseBillPaymentMethod.Cash && this.cashAccounts.length > 0) {
      this.expenseBillForm.paymentAccountId = this.cashAccounts[0].id;
      this.expenseBillForm.paymentAccountDisplay = `${this.cashAccounts[0].accountCode} - ${this.cashAccounts[0].accountNameAr}`;
    } else if (method === ExpenseBillPaymentMethod.Bank && this.bankAccounts.length > 0) {
      this.expenseBillForm.paymentAccountId = this.bankAccounts[0].id;
      this.expenseBillForm.paymentAccountDisplay = `${this.bankAccounts[0].accountCode} - ${this.bankAccounts[0].accountNameAr}`;
    }
  }

  onVendorChange(event: any): void {
    const vendorId = event.value;
    const vendor = this.vendors.find(v => v.id === vendorId);
    this.expenseBillForm.vendorId = vendorId;
    this.expenseBillForm.vendorDisplay = vendor ? `${vendor.vendorCode} - ${vendor.nameAr}` : '';
  }

  onPaymentAccountChange(event: any): void {
    const accountId = event.value;
    const account = [...this.cashAccounts, ...this.bankAccounts].find(a => a.id === accountId);
    this.expenseBillForm.paymentAccountId = accountId;
    this.expenseBillForm.paymentAccountDisplay = account ? `${account.accountCode} - ${account.accountNameAr}` : '';
  }

  // ── Lines Management ─────────────────────────────────────────

  addLine(): void {
    this.expenseBillForm.lines.push({
      accountId: '',
      accountDisplay: '',
      amount: 0,
      costCenterId: '',
      costCenterDisplay: '',
      notes: ''
    });
  }

  removeLine(index: number): void {
    this.expenseBillForm.lines.splice(index, 1);
    this.calculateTotals();
  }

  onLineAccountChange(index: number, event: any): void {
    const accountId = event.value;
    const account = this.expenseAccounts.find(a => a.id === accountId);
    this.expenseBillForm.lines[index].accountId = accountId;
    this.expenseBillForm.lines[index].accountDisplay = account ? `${account.accountCode} - ${account.accountNameAr}` : '';
  }

  onLineCostCenterChange(index: number, event: any): void {
    const costCenterId = event.value;
    const costCenter = this.costCenters.find(c => c.id === costCenterId);
    this.expenseBillForm.lines[index].costCenterId = costCenterId;
    this.expenseBillForm.lines[index].costCenterDisplay = costCenter ? `${costCenter.costCenterCode} - ${costCenter.costCenterNameAr}` : '';
  }

  onLineAmountChange(index: number, event: any): void {
    const amount = event.value;
    this.expenseBillForm.lines[index].amount = amount || 0;
    this.calculateTotals();
  }

  calculateTotals(): void {
    const linesTotal = this.expenseBillForm.lines.reduce((sum, line) => sum + (line.amount || 0), 0);
    this.expenseBillForm.netAmount = linesTotal;
    this.expenseBillForm.totalAmount = linesTotal + this.expenseBillForm.taxAmount;
  }

  onTaxAmountChange(event: any): void {
    const taxAmount = event.value;
    this.expenseBillForm.taxAmount = taxAmount || 0;
    this.calculateTotals();
  }

  // ── Save ─────────────────────────────────────────────────────

  saveBill(): void {
    // Validation
    if (!this.expenseBillForm.billNumber) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى إدخال رقم الفاتورة' });
      return;
    }

    // Validate based on payment method
    if (this.expenseBillForm.paymentMethod === ExpenseBillPaymentMethod.Credit && !this.expenseBillForm.vendorId) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى اختيار المورد للمصروفات الآجلة' });
      return;
    }

    if ((this.expenseBillForm.paymentMethod === ExpenseBillPaymentMethod.Cash || 
         this.expenseBillForm.paymentMethod === ExpenseBillPaymentMethod.Bank) && 
        !this.expenseBillForm.paymentAccountId) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى اختيار حساب الدفع' });
      return;
    }

    // Validate lines
    if (!this.expenseBillForm.lines || this.expenseBillForm.lines.length === 0) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى إضافة سطر واحد على الأقل' });
      return;
    }

    for (let i = 0; i < this.expenseBillForm.lines.length; i++) {
      const line = this.expenseBillForm.lines[i];
      if (!line.accountId) {
        this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: `يرجى اختيار حساب المصروف للسطر ${i + 1}` });
        return;
      }
      if (!line.costCenterId) {
        this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: `يرجى اختيار مركز التكلفة للسطر ${i + 1}` });
        return;
      }
      if (line.amount <= 0) {
        this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: `يرجى إدخال مبلغ صحيح للسطر ${i + 1}` });
        return;
      }
    }

    if (this.expenseBillForm.netAmount <= 0) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'المبلغ الصافي يجب أن يكون أكبر من صفر' });
      return;
    }

    const lines: CreateExpenseBillLineCommand[] = this.expenseBillForm.lines.map(line => ({
      accountId: line.accountId,
      amount: line.amount,
      costCenterId: line.costCenterId,
      notes: line.notes || undefined
    }));

    const command: CreateExpenseBillCommand = {
      billNumber: this.expenseBillForm.billNumber,
      transactionDate: this.formatDate(this.expenseBillForm.transactionDate),
      paymentMethod: this.expenseBillForm.paymentMethod,
      totalAmount: this.expenseBillForm.totalAmount,
      taxAmount: this.expenseBillForm.taxAmount,
      netAmount: this.expenseBillForm.netAmount,
      createdBy: this.currentUser,
      notes: this.expenseBillForm.notes || undefined,
      vendorId: this.expenseBillForm.vendorId ? this.expenseBillForm.vendorId as any : undefined,
      supplierName: this.expenseBillForm.supplierName || undefined,
      paymentAccountId: this.expenseBillForm.paymentAccountId ? this.expenseBillForm.paymentAccountId as any : undefined,
      lines
    };

    this.isSaving = true;

    this.expenseBillService.create(command).subscribe({
      next: () => {
        this.isSaving = false;
        this.createDialogVisible = false;
        this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم إنشاء الفاتورة بنجاح' });
        this.loadBills(); // Refresh the bill list
      },
      error: (err) => {
        this.isSaving = false;
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل إنشاء الفاتورة' });
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

  getPaymentMethodLabel(method: ExpenseBillPaymentMethod): string {
    switch (method) {
      case ExpenseBillPaymentMethod.Cash: return 'نقدي';
      case ExpenseBillPaymentMethod.Bank: return 'بنكي';
      case ExpenseBillPaymentMethod.Credit: return 'آجل/ذمم';
      default: return '';
    }
  }

  getStatusLabel(status: ExpenseBillStatus): string {
    switch (status) {
      case ExpenseBillStatus.Draft: return 'مسودة';
      case ExpenseBillStatus.Posted: return 'مرحل';
      case ExpenseBillStatus.Cancelled: return 'ملغي';
      default: return '';
    }
  }

  getStatusSeverity(status: ExpenseBillStatus): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' | null {
    switch (status) {
      case ExpenseBillStatus.Draft: return 'info';
      case ExpenseBillStatus.Posted: return 'success';
      case ExpenseBillStatus.Cancelled: return 'danger';
      default: return null;
    }
  }

  // ── View Details ────────────────────────────────────────────────

  viewBillDetails(bill: ExpenseBillListItem): void {
    this.selectedBill = bill;
    this.viewDetailsDialogVisible = true;
  }
}
