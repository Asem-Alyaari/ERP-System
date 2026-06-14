import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { InventoryTransactionService, TransactionType, TransactionStatus, InventoryTransaction, TransactionLine, ItemStockInfo } from '../../../core/services/inventory-transaction.service';
import { WarehouseService, Warehouse } from '../../../core/services/warehouse.service';
import { ItemService, Item } from '../../../core/services/item.service';
import { SHARED_IMPORTS } from '../../../shared/shared.imports';
import { MessageService, ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-inventory-transactions',
  standalone: true,
  imports: [...SHARED_IMPORTS],
  providers: [MessageService, ConfirmationService],
  templateUrl: './inventory-transactions.component.html',
  styleUrl: './inventory-transactions.component.scss'
})
export class InventoryTransactionsComponent implements OnInit {
  // Transactions List
  transactions: InventoryTransaction[] = [];
  isLoadingTransactions = false;

  // Dropdown Data
  warehouses: Warehouse[] = [];
  items: Item[] = [];
  isLoadingWarehouses = false;
  isLoadingItems = false;

  // Transaction Type Options
  transactionTypeOptions = [
    { label: 'توريد', value: TransactionType.StockIn },
    { label: 'صرف', value: TransactionType.StockOut },
    { label: 'تحويل', value: TransactionType.Transfer }
  ];

  // Dialog State
  transactionDialogVisible = false;
  isEditMode = false;
  currentTransaction: Partial<InventoryTransaction> = {};

  // Form
  transactionForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private transactionService: InventoryTransactionService,
    private warehouseService: WarehouseService,
    private itemService: ItemService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {
    this.transactionForm = this.createTransactionForm();
  }

  ngOnInit() {
    this.loadTransactions();
    this.loadWarehouses();
    this.loadItems();
  }

  // ── Form Creation ─────────────────────────────────────────────────────
  createTransactionForm(): FormGroup {
    return this.fb.group({
      transactionDate: [new Date().toISOString().split('T')[0], Validators.required],
      transactionType: [TransactionType.StockIn, Validators.required],
      sourceWarehouseId: ['', Validators.required],
      destinationWarehouseId: [''],
      notes: [''],
      lines: this.fb.array([])
    });
  }

  get linesArray(): FormArray {
    return this.transactionForm.get('lines') as FormArray;
  }

  createLineForm(): FormGroup {
    return this.fb.group({
      itemId: ['', Validators.required],
      quantity: [1, [Validators.required, Validators.min(0.01)]],
      unitCost: [0, [Validators.required, Validators.min(0)]],
      totalCost: [{ value: 0, disabled: true }],
      batchNumber: [''],
      availableQty: [{ value: 0, disabled: true }]
    });
  }

  // ── Data Loading ───────────────────────────────────────────────────────
  loadTransactions() {
    this.isLoadingTransactions = true;
    this.transactionService.getAll(1, 100).subscribe({
      next: (response) => {
        this.transactions = response.items || [];
        this.isLoadingTransactions = false;
      },
      error: () => {
        this.isLoadingTransactions = false;
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل الحركات المخزنية' });
      }
    });
  }

  loadWarehouses() {
    this.isLoadingWarehouses = true;
    this.warehouseService.getList().subscribe({
      next: (warehouses) => {
        this.warehouses = warehouses || [];
        this.isLoadingWarehouses = false;
      },
      error: () => {
        this.isLoadingWarehouses = false;
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل المخازن' });
      }
    });
  }

  loadItems() {
    this.isLoadingItems = true;
    this.itemService.getAll(1, 1000).subscribe({
      next: (response) => {
        this.items = response.items || [];
        this.isLoadingItems = false;
      },
      error: () => {
        this.isLoadingItems = false;
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل الأصناف' });
      }
    });
  }

  // ── Dialog Management ─────────────────────────────────────────────────
  openAddTransactionDialog() {
    this.isEditMode = false;
    this.currentTransaction = {};
    this.transactionForm = this.createTransactionForm();
    this.transactionDialogVisible = true;
  }

  openEditTransactionDialog(transaction: InventoryTransaction) {
    if (transaction.status === TransactionStatus.Posted) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'لا يمكن تعديل الحركات المرحلة' });
      return;
    }

    this.isEditMode = true;
    this.currentTransaction = { ...transaction };
    this.transactionForm = this.createTransactionForm();
    
    // Populate form
    this.transactionForm.patchValue({
      transactionDate: transaction.transactionDate.split('T')[0],
      transactionType: transaction.transactionType,
      sourceWarehouseId: transaction.sourceWarehouseId,
      destinationWarehouseId: transaction.destinationWarehouseId || '',
      notes: transaction.notes || ''
    });

    // Populate lines
    this.linesArray.clear();
    for (const line of transaction.lines) {
      const lineForm = this.createLineForm();
      lineForm.patchValue({
        itemId: line.itemId,
        quantity: line.quantity,
        unitCost: line.unitCost,
        totalCost: line.totalCost,
        batchNumber: line.batchNumber || '',
        availableQty: line.availableQty || 0
      });
      this.linesArray.push(lineForm);
    }

    this.transactionDialogVisible = true;
  }

  closeTransactionDialog() {
    this.transactionDialogVisible = false;
    this.transactionForm = this.createTransactionForm();
    this.currentTransaction = {};
  }

  // ── Line Management ────────────────────────────────────────────────────
  addLine() {
    this.linesArray.push(this.createLineForm());
  }

  removeLine(index: number) {
    this.linesArray.removeAt(index);
  }

  // ── Dynamic Field Behavior ────────────────────────────────────────────
  onTransactionTypeChange() {
    const type = this.transactionForm.get('transactionType')?.value;
    const destWarehouseControl = this.transactionForm.get('destinationWarehouseId');

    if (type === TransactionType.Transfer) {
      destWarehouseControl?.setValidators([Validators.required]);
    } else {
      destWarehouseControl?.clearValidators();
      destWarehouseControl?.setValue('');
    }
    destWarehouseControl?.updateValueAndValidity();

    // Update unit costs for existing lines
    this.linesArray.controls.forEach((lineForm, index) => {
      this.onItemChange(index);
    });
  }

  async onItemChange(lineIndex: number) {
    const lineForm = this.linesArray.at(lineIndex);
    const itemId = lineForm.get('itemId')?.value;
    const sourceWarehouseId = this.transactionForm.get('sourceWarehouseId')?.value;
    const transactionType = this.transactionForm.get('transactionType')?.value;

    if (!itemId || !sourceWarehouseId) return;

    try {
      const stockInfo = await this.transactionService.getItemStockInfo(itemId, sourceWarehouseId).toPromise();
      
      if (stockInfo) {
        lineForm.patchValue({
          availableQty: stockInfo.availableQty
        });

        // Set unit cost based on transaction type
        if (transactionType === TransactionType.StockIn) {
          lineForm.get('unitCost')?.enable();
          lineForm.patchValue({ unitCost: 0 });
        } else {
          lineForm.get('unitCost')?.disable();
          lineForm.patchValue({ unitCost: stockInfo.averageCost });
        }

        this.calculateLineTotal(lineIndex);
      }
    } catch (error) {
      console.error('Failed to fetch stock info:', error);
    }
  }

  onQuantityChange(lineIndex: number) {
    this.calculateLineTotal(lineIndex);
    this.validateLineQuantity(lineIndex);
  }

  onUnitCostChange(lineIndex: number) {
    this.calculateLineTotal(lineIndex);
  }

  calculateLineTotal(lineIndex: number) {
    const lineForm = this.linesArray.at(lineIndex);
    const quantity = lineForm.get('quantity')?.value || 0;
    const unitCost = lineForm.get('unitCost')?.value || 0;
    const total = quantity * unitCost;
    lineForm.patchValue({ totalCost: total });
  }

  validateLineQuantity(lineIndex: number) {
    const lineForm = this.linesArray.at(lineIndex);
    const transactionType = this.transactionForm.get('transactionType')?.value;
    const quantity = lineForm.get('quantity')?.value || 0;
    const availableQty = lineForm.get('availableQty')?.value || 0;

    if (transactionType === TransactionType.StockOut || transactionType === TransactionType.Transfer) {
      if (quantity > availableQty) {
        lineForm.get('quantity')?.setErrors({ exceedsAvailable: true });
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: `الكمية المدخلة (${quantity}) تتجاوز الرصيد المتاح (${availableQty})`
        });
      } else {
        lineForm.get('quantity')?.setErrors(null);
      }
    }
  }

  // ── Save Transaction ───────────────────────────────────────────────────
  saveTransaction() {
    if (this.transactionForm.invalid) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى تعبئة الحقول المطلوبة' });
      return;
    }

    const lines = this.linesArray.controls.map(lineForm => {
      const itemId = lineForm.get('itemId')?.value;
      const item = this.items.find(i => i.id === itemId);
      return {
        itemId: itemId,
        itemNameAr: item?.itemNameAr,
        itemNameEn: item?.itemNameEn,
        quantity: lineForm.get('quantity')?.value,
        unitCost: lineForm.get('unitCost')?.value,
        totalCost: lineForm.get('totalCost')?.value,
        batchNumber: lineForm.get('batchNumber')?.value
      };
    });

    if (lines.length === 0) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى إضافة صنف واحد على الأقل' });
      return;
    }

    const payload = {
      transactionDate: this.transactionForm.get('transactionDate')?.value,
      transactionType: this.transactionForm.get('transactionType')?.value,
      sourceWarehouseId: this.transactionForm.get('sourceWarehouseId')?.value,
      destinationWarehouseId: this.transactionForm.get('destinationWarehouseId')?.value || undefined,
      notes: this.transactionForm.get('notes')?.value,
      lines: lines
    };

    if (this.isEditMode && this.currentTransaction.id) {
      this.transactionService.update(this.currentTransaction.id, payload).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم تعديل الحركة بنجاح' });
          this.loadTransactions();
          this.closeTransactionDialog();
        },
        error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تعديل الحركة' })
      });
    } else {
      this.transactionService.create(payload).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم إنشاء الحركة بنجاح' });
          this.loadTransactions();
          this.closeTransactionDialog();
        },
        error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل إنشاء الحركة' })
      });
    }
  }

  // ── Post Transaction ───────────────────────────────────────────────────
  confirmPostTransaction(transaction: InventoryTransaction) {
    if (transaction.status === TransactionStatus.Posted) {
      this.messageService.add({ severity: 'info', summary: 'معلومات', detail: 'هذه الحركة مرحلة بالفعل' });
      return;
    }

    this.confirmationService.confirm({
      message: 'هل أنت متأكد من ترحيل هذه الحركة المخزنية؟\n\nسيؤدي الترحيل إلى:\n• قفل المستند ومنع التعديل\n• تحديث تكاليف الأصناف\n• إنشاء قيود محاسبية تلقائياً',
      header: 'تأكيد ترحيل الحركة',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'ترحيل',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-warning',
      accept: () => {
        this.postTransaction(transaction.id);
      }
    });
  }

  postTransaction(id: string) {
    this.transactionService.post(id).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم ترحيل الحركة بنجاح' });
        this.loadTransactions();
      },
      error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل ترحيل الحركة' })
    });
  }

  // ── Delete Transaction ─────────────────────────────────────────────────
  confirmDeleteTransaction(transaction: InventoryTransaction) {
    if (transaction.status === TransactionStatus.Posted) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'لا يمكن حذف الحركات المرحلة' });
      return;
    }

    this.confirmationService.confirm({
      message: `هل أنت متأكد من حذف الحركة "${transaction.documentNumber}"؟`,
      header: 'تأكيد الحذف',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'حذف',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.deleteTransaction(transaction.id);
      }
    });
  }

  deleteTransaction(id: string) {
    this.transactionService.delete(id).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم حذف الحركة بنجاح' });
        this.loadTransactions();
      },
      error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل حذف الحركة' })
    });
  }

  // ── Helper Methods ────────────────────────────────────────────────────
  getTransactionTypeName(type: TransactionType): string {
    switch (type) {
      case TransactionType.StockIn: return 'توريد';
      case TransactionType.StockOut: return 'صرف';
      case TransactionType.Transfer: return 'تحويل';
      default: return '-';
    }
  }

  getTransactionStatusName(status: TransactionStatus): string {
    switch (status) {
      case TransactionStatus.Draft: return 'مسودة';
      case TransactionStatus.Posted: return 'مرحل';
      default: return '-';
    }
  }

  getTransactionStatusSeverity(status: TransactionStatus): string {
    switch (status) {
      case TransactionStatus.Draft: return 'info';
      case TransactionStatus.Posted: return 'success';
      default: return 'secondary';
    }
  }

  getWarehouseName(warehouseId: string): string {
    const warehouse = this.warehouses.find(w => w.id === warehouseId);
    return warehouse ? warehouse.nameAr : '-';
  }

  getItemName(itemId: string): string {
    const item = this.items.find(i => i.id === itemId);
    return item ? item.itemNameAr : '-';
  }

  getFilteredItems(event: any): Item[] {
    const query = event.query?.toLowerCase() || '';
    return this.items.filter(item => 
      item.itemNameAr.toLowerCase().includes(query) ||
      item.itemNameEn.toLowerCase().includes(query) ||
      item.itemCode.toLowerCase().includes(query)
    );
  }
}
