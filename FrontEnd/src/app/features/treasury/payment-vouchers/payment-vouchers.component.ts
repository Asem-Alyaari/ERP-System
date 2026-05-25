import { Component, OnInit } from '@angular/core';
import { SHARED_IMPORTS } from '../../../shared/shared.imports';
import { MessageService, ConfirmationService } from 'primeng/api';
import {
  PaymentVoucherService,
  PaymentMethod,
  VoucherPartnerType,
  VoucherStatus,
  CreatePaymentVoucherCommand,
  PaymentVoucherListItem
} from '../../../core/services/payment-voucher.service';
import { AccountService, AccountLookup } from '../../../core/services/account.service';
import { CostCenterService, CostCenterLookup } from '../../../core/services/cost-center.service';
import { VendorService, Vendor } from '../../../core/services/vendor.service';
import { CustomerService, Customer } from '../../../core/services/customer.service';
import { CurrencyService, Currency } from '../../../core/services/currency.service';
import { forkJoin } from 'rxjs';

export interface VoucherForm {
  voucherNumber: string;
  voucherDate: Date;
  paymentMethod: PaymentMethod;
  sourceAccountId: string;
  sourceAccountDisplay: string;
  destinationType: string;
  vendorId: string | null;
  vendorDisplay: string;
  customerId: string | null;
  customerDisplay: string;
  destinationAccountId: string | null;
  destinationAccountDisplay: string;
  amount: number;
  costCenterId: string | null;
  costCenterDisplay: string;
  costCenterStatus?: 'Required' | 'Optional' | 'Disabled';
  notes: string;
}

@Component({
  selector: 'app-payment-vouchers',
  standalone: true,
  imports: [...SHARED_IMPORTS],
  providers: [MessageService, ConfirmationService],
  templateUrl: './payment-vouchers.component.html',
  styleUrl: './payment-vouchers.component.scss'
})
export class PaymentVouchersComponent implements OnInit {
  // ── List state ──────────────────────────────────────────────
  vouchers: PaymentVoucherListItem[] = [];
  isLoading = false;

  // ── Lookup data ──────────────────────────────────────────────
  cashAccounts: AccountLookup[] = [];
  bankAccounts: AccountLookup[] = [];
  personalAccounts: AccountLookup[] = []; // For Account type (personal, employee, partner)
  vendors: Vendor[] = [];
  customers: Customer[] = [];
  costCenters: CostCenterLookup[] = [];
  localCurrency: Currency | null = null;

  // ── Tab State ─────────────────────────────────────────────────
  activeTab: 'payment' | 'receipt' = 'payment';

  // ── Create Dialog ────────────────────────────────────────────
  createDialogVisible = false;
  isSaving = false;

  // ── View Details Dialog ───────────────────────────────────────
  viewDetailsDialogVisible = false;
  selectedVoucher: PaymentVoucherListItem | null = null;

  voucherForm: VoucherForm = {
    voucherNumber: '',
    voucherDate: new Date(),
    paymentMethod: PaymentMethod.Cash,
    sourceAccountId: '',
    sourceAccountDisplay: '',
    destinationType: '2',
    vendorId: null,
    vendorDisplay: '',
    customerId: null,
    customerDisplay: '',
    destinationAccountId: null,
    destinationAccountDisplay: '',
    amount: 0,
    costCenterId: null,
    costCenterDisplay: '',
    costCenterStatus: 'Optional',
    notes: ''
  };

  // ── Enums for template ─────────────────────────────────────────
  PaymentMethod = PaymentMethod;
  VoucherPartnerType = VoucherPartnerType;
  VoucherStatus = VoucherStatus;

  // ── Destination Type Options ─────────────────────────────────────
  get destinationTypeOptions() {
    if (this.activeTab === 'payment') {
      return [
        { label: 'عميل', value: '1' },
        { label: 'مورد', value: '2' },
        { label: 'حساب ذمم/عهد', value: '3' }
      ];
    } else {
      return [
        { label: 'عميل', value: '1' },
        { label: 'حساب ذمم/عهد', value: '3' }
      ];
    }
  }

  // ── Username (temp) ──────────────────────────────────────────
  currentUser = 'admin';

  constructor(
    private paymentVoucherService: PaymentVoucherService,
    private accountService: AccountService,
    private costCenterService: CostCenterService,
    private vendorService: VendorService,
    private customerService: CustomerService,
    private currencyService: CurrencyService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.loadLookups();
    this.loadVouchers();
  }

  // ── Lookups ──────────────────────────────────────────────────

  loadLookups(): void {
    forkJoin({
      accounts: this.accountService.getDetailAccounts(),
      costCenters: this.costCenterService.getDetailCostCenters(),
      vendors: this.vendorService.getAllList(),
      customers: this.customerService.getAllList(),
      currencies: this.currencyService.getList()
    }).subscribe({
      next: ({ accounts, costCenters, vendors, customers, currencies }) => {
        // تصفية الحسابات حسب النوع - تطبيق قواعد العمل الصارمة
        this.cashAccounts = accounts.filter(a => a.accountCode.startsWith('110101')); // الصندوق
        this.bankAccounts = accounts.filter(a => a.accountCode.startsWith('110102')); // البنوك
        // حسابات شخصية/عهدية فقط - استبعاد الصناديق والبنوك والمصروفات والإيرادات
        this.personalAccounts = accounts.filter((a: AccountLookup) =>
          !a.accountCode.startsWith('1101') && // استبعاد الصناديق والبنوك
          !a.accountCode.startsWith('5') &&      // استبعاد المصروفات
          !a.accountCode.startsWith('4') &&      // استبعاد الإيرادات
          a.accountCode.length > 4               // فقط الحسابات التفصيلية
        );

        this.costCenters = costCenters || [];
        this.vendors = vendors || [];
        this.customers = customers || [];
        this.localCurrency = currencies.find(c => c.isLocal) ?? currencies[0] ?? null;

        // تعيين افتراضي
        if (this.cashAccounts.length > 0) {
          this.voucherForm.sourceAccountId = this.cashAccounts[0].id;
          this.voucherForm.sourceAccountDisplay = `${this.cashAccounts[0].accountCode} - ${this.cashAccounts[0].accountNameAr}`;
          if (this.cashAccounts[0].accountCode.startsWith('110101')) {
            this.voucherForm.paymentMethod = PaymentMethod.Cash;
          }
        }
      },
      error: (err) => {
        console.error('Failed to load lookups:', err);
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل البيانات الأساسية' });
      }
    });
  }

  // ── Voucher List ───────────────────────────────────────────────

  loadVouchers(): void {
    this.isLoading = true;
    this.paymentVoucherService.getAll().subscribe({
      next: (data) => {
        this.vouchers = data || [];
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load vouchers:', err);
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل سندات الصرف' });
        this.isLoading = false;
      }
    });
  }

  postVoucher(voucherId: string): void {
    this.paymentVoucherService.post(voucherId, this.currentUser).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم ترحيل السند بنجاح' });
        this.loadVouchers(); // Refresh the list
      },
      error: (err) => {
        console.error('Failed to post voucher:', err);
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل ترحيل السند' });
      }
    });
  }

  // ── Tab Actions ───────────────────────────────────────────────

  onTabChange(tab: 'payment' | 'receipt'): void {
    this.activeTab = tab;
    // Reset form when switching tabs
    this.voucherForm = {
      voucherNumber: '',
      voucherDate: new Date(),
      paymentMethod: PaymentMethod.Cash,
      sourceAccountId: this.cashAccounts[0]?.id || '',
      sourceAccountDisplay: this.cashAccounts[0] ? `${this.cashAccounts[0].accountCode} - ${this.cashAccounts[0].accountNameAr}` : '',
      destinationType: tab === 'payment' ? '2' : '1', // Set based on new tab value
      vendorId: null,
      vendorDisplay: '',
      customerId: null,
      customerDisplay: '',
      destinationAccountId: null,
      destinationAccountDisplay: '',
      amount: 0,
      costCenterId: null,
      costCenterDisplay: '',
      costCenterStatus: 'Optional',
      notes: ''
    };
  }

  // ── Form Actions ─────────────────────────────────────────────

  openCreateDialog(): void {
    this.voucherForm = {
      voucherNumber: this.generateVoucherNumber(),
      voucherDate: new Date(),
      paymentMethod: PaymentMethod.Cash,
      sourceAccountId: this.cashAccounts[0]?.id || '',
      sourceAccountDisplay: this.cashAccounts[0] ? `${this.cashAccounts[0].accountCode} - ${this.cashAccounts[0].accountNameAr}` : '',
      destinationType: this.activeTab === 'payment' ? '2' : '1', // Default to Vendor for payment, Customer for receipt
      vendorId: null,
      vendorDisplay: '',
      customerId: null,
      customerDisplay: '',
      destinationAccountId: null,
      destinationAccountDisplay: '',
      amount: 0,
      costCenterId: null,
      costCenterDisplay: '',
      costCenterStatus: 'Optional',
      notes: ''
    };
    this.createDialogVisible = true;
  }

  generateVoucherNumber(): string {
    const now = new Date();
    const y = now.getFullYear();
    const m = String(now.getMonth() + 1).padStart(2, '0');
    const d = String(now.getDate()).padStart(2, '0');
    const rand = String(Math.floor(Math.random() * 9000) + 1000);
    return `PV-${y}${m}${d}-${rand}`;
  }

  onSourceAccountChange(event: any): void {
    const accountId = event;
    const account = [...this.cashAccounts, ...this.bankAccounts].find(a => a.id === accountId);
    this.voucherForm.sourceAccountId = accountId;
    this.voucherForm.sourceAccountDisplay = account ? `${account.accountCode} - ${account.accountNameAr}` : '';

    // تحديث طريقة الدفع بناءً على نوع الحساب
    if (account?.accountCode.startsWith('110101')) {
      this.voucherForm.paymentMethod = PaymentMethod.Cash;
    } else if (account?.accountCode.startsWith('110102')) {
      this.voucherForm.paymentMethod = PaymentMethod.BankTransfer;
    }
  }

  onDestinationTypeChange(event: any): void {
    const type = event;
    this.voucherForm.destinationType = type;
    this.voucherForm.vendorId = null;
    this.voucherForm.vendorDisplay = '';
    this.voucherForm.customerId = null;
    this.voucherForm.customerDisplay = '';
    this.voucherForm.destinationAccountId = null;
    this.voucherForm.destinationAccountDisplay = '';
    this.voucherForm.costCenterId = null;
    this.voucherForm.costCenterDisplay = '';
    this.voucherForm.costCenterStatus = 'Optional';
  }

  onVendorChange(event: any): void {
    const vendorId = event;
    const vendor = this.vendors.find(v => v.id === vendorId);
    this.voucherForm.vendorId = vendorId;
    this.voucherForm.vendorDisplay = vendor ? `${vendor.vendorCode} - ${vendor.nameAr}` : '';
  }

  onCustomerChange(event: any): void {
    const customerId = event;
    const customer = this.customers.find(c => c.id === customerId);
    this.voucherForm.customerId = customerId;
    this.voucherForm.customerDisplay = customer ? `${customer.customerCode} - ${customer.nameAr}` : '';
  }

  onDestinationAccountChange(event: any): void {
    const accountId = event;
    const account = this.personalAccounts.find((a: AccountLookup) => a.id === accountId);
    this.voucherForm.destinationAccountId = accountId;
    this.voucherForm.destinationAccountDisplay = account ? `${account.accountCode} - ${account.accountNameAr}` : '';

    // تحديث حالة مركز التكلفة
    const status = account?.costCenterStatus || 'Optional';
    this.voucherForm.costCenterStatus = (status === 'Required' || status === 'Optional' || status === 'Disabled')
      ? status
      : 'Optional';

    if (this.voucherForm.costCenterStatus === 'Disabled') {
      this.voucherForm.costCenterId = null;
      this.voucherForm.costCenterDisplay = '';
    }
  }

  onCostCenterChange(event: any): void {
    const costCenterId = event;
    const costCenter = this.costCenters.find(c => c.id === costCenterId);
    this.voucherForm.costCenterId = costCenterId;
    this.voucherForm.costCenterDisplay = costCenter ? `${costCenter.costCenterCode} - ${costCenter.costCenterNameAr}` : '';
  }

  saveVoucher(): void {
    // Temporarily disable receipt voucher creation until backend is ready
    if (this.activeTab === 'receipt') {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'سندات القبض غير متاحة حالياً - قيد التطوير' });
      return;
    }

    // Validation
    if (!this.voucherForm.sourceAccountId) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى اختيار الحساب المصدر' });
      return;
    }

    // Validate based on destination type
    if (this.voucherForm.destinationType === '3' && !this.voucherForm.destinationAccountId) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى اختيار الحساب الوجهة' });
      return;
    }

    if (this.voucherForm.destinationType === '2' && !this.voucherForm.vendorId) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى اختيار المورد' });
      return;
    }

    if (this.voucherForm.destinationType === '1' && !this.voucherForm.customerId) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى اختيار العميل' });
      return;
    }

    if (this.voucherForm.amount <= 0) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى إدخال مبلغ صحيح أكبر من صفر' });
      return;
    }

    // التحقق من مركز التكلفة الإلزامي
    if (this.voucherForm.destinationType === '3' &&
        this.voucherForm.costCenterStatus === 'Required' &&
        (!this.voucherForm.costCenterId || this.voucherForm.costCenterId === '')) {
      this.messageService.add({ severity: 'warn', summary: 'مراكز تكلفة مفقودة', detail: 'يرجى تحديد مركز تكلفة لهذا السند' });
      return;
    }

    // التحقق من مركز التكلفة الإلزامي إذا تم اختيار حساب وجهة
    if (this.voucherForm.destinationAccountId &&
        this.voucherForm.costCenterStatus === 'Required' &&
        (!this.voucherForm.costCenterId || this.voucherForm.costCenterId === '')) {
      this.messageService.add({ severity: 'warn', summary: 'مراكز تكلفة مفقودة', detail: 'يرجى تحديد مركز تكلفة لهذا السند' });
      return;
    }

    const command: CreatePaymentVoucherCommand = {
      voucherNumber: this.voucherForm.voucherNumber,
      voucherDate: this.formatDate(this.voucherForm.voucherDate),
      paymentMethod: this.voucherForm.paymentMethod,
      sourceAccountId: this.voucherForm.sourceAccountId,
      destinationType: parseInt(this.voucherForm.destinationType) as any,
      amount: this.voucherForm.amount,
      createdBy: this.currentUser,
      notes: this.voucherForm.notes || undefined,
      vendorId: this.voucherForm.vendorId ? this.voucherForm.vendorId as any : undefined,
      customerId: this.voucherForm.customerId ? this.voucherForm.customerId as any : undefined,
      destinationAccountId: this.voucherForm.destinationAccountId ? this.voucherForm.destinationAccountId as any : undefined,
      costCenterId: (this.voucherForm.costCenterId && this.voucherForm.costCenterId !== '') ? this.voucherForm.costCenterId as any : undefined
    };

    console.log('Command being sent to backend:', command);
    console.log('Form state:', this.voucherForm);
    this.isSaving = true;

    // Since receipt vouchers are temporarily disabled, only use payment endpoint
    this.paymentVoucherService.create(command).subscribe({
      next: () => {
        this.isSaving = false;
        this.createDialogVisible = false;
        this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم إنشاء السند بنجاح' });
        this.loadVouchers(); // Refresh the voucher list
      },
      error: (err) => {
        this.isSaving = false;
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل إنشاء السند' });
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

  getPaymentMethodLabel(method: PaymentMethod): string {
    switch (method) {
      case PaymentMethod.Cash: return 'نقدي';
      case PaymentMethod.BankTransfer: return 'تحويل بنكي';
      case PaymentMethod.Cheque: return 'شيك';
      default: return '';
    }
  }

  getDestinationTypeLabel(type: number): string {
    switch (type) {
      case 1: return 'عميل';
      case 2: return 'مورد';
      case 3: return 'حساب مباشر';
      default: return 'غير معروف';
    }
  }

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

  // ── View Details ────────────────────────────────────────────────

  viewVoucherDetails(voucher: PaymentVoucherListItem): void {
    this.selectedVoucher = voucher;
    this.viewDetailsDialogVisible = true;
  }
}
