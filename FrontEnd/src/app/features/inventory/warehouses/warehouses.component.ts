import { Component, OnInit } from '@angular/core';
import { WarehouseService, Warehouse } from '../../../core/services/warehouse.service';
import { SHARED_IMPORTS } from '../../../shared/shared.imports';
import { MessageService, ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-warehouses',
  standalone: true,
  imports: [...SHARED_IMPORTS],
  providers: [MessageService, ConfirmationService],
  templateUrl: './warehouses.component.html',
  styleUrl: './warehouses.component.scss'
})
export class WarehousesComponent implements OnInit {
  warehouses: Warehouse[] = [];
  isLoading = false;
  dialogVisible = false;
  isEditMode = false;
  currentWarehouse: Partial<Warehouse> = {};

  constructor(
    private warehouseService: WarehouseService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit() {
    this.loadWarehouses();
  }

  loadWarehouses() {
    this.isLoading = true;
    this.warehouseService.getList().subscribe({
      next: (data) => {
        this.warehouses = data || [];
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل المستودعات' });
      }
    });
  }

  openAddDialog() {
    this.isEditMode = false;
    this.currentWarehouse = {
      code: '',
      nameAr: '',
      nameEn: '',
      location: '',
      isActive: true
    };
    this.dialogVisible = true;
  }

  openEditDialog(warehouse: Warehouse) {
    this.isEditMode = true;
    this.currentWarehouse = { ...warehouse };
    this.dialogVisible = true;
  }

  closeDialog() {
    this.dialogVisible = false;
    this.currentWarehouse = {};
  }

  saveWarehouse() {
    if (!this.currentWarehouse.code || !this.currentWarehouse.nameAr || !this.currentWarehouse.nameEn) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى تعبئة الحقول المطلوبة' });
      return;
    }

    if (this.isEditMode && this.currentWarehouse.id) {
      this.warehouseService.update(this.currentWarehouse.id, this.currentWarehouse).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم تعديل المستودع بنجاح' });
          this.loadWarehouses();
          this.closeDialog();
        },
        error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تعديل المستودع' })
      });
    } else {
      this.warehouseService.create(this.currentWarehouse).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تمت إضافة المستودع بنجاح' });
          this.loadWarehouses();
          this.closeDialog();
        },
        error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل إضافة المستودع' })
      });
    }
  }

  confirmDelete(warehouse: Warehouse) {
    this.confirmationService.confirm({
      message: `هل أنت متأكد من حذف المستودع "${warehouse.nameAr}"؟`,
      header: 'تأكيد الحذف',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'حذف',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.warehouseService.delete(warehouse.id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم حذف المستودع بنجاح' });
            this.loadWarehouses();
          },
          error: (err) => this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: err.error?.message || 'فشل الحذف (قد يكون المستودع مرتبط بحركات مخزنية)'
          })
        });
      }
    });
  }

  getStatusBadge(warehouse: Warehouse) {
    return warehouse.isActive
      ? { severity: 'success' as const, label: 'نشط' }
      : { severity: 'danger' as const, label: 'غير نشط' };
  }
}
