import { Component, OnInit } from '@angular/core';
import { VendorService, Vendor } from '../../../core/services/vendor.service';
import { SHARED_IMPORTS } from '../../../shared/shared.imports';
import { MessageService, ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-vendors',
  standalone: true,
  imports: [...SHARED_IMPORTS],
  providers: [MessageService, ConfirmationService],
  templateUrl: './vendors.component.html',
})
export class VendorsComponent implements OnInit {
  vendors: Vendor[] = [];
  isLoading = false;

  // Dialog state
  dialogVisible = false;
  isEditMode = false;
  currentVendor: Partial<Vendor> = {};

  constructor(
    private vendorService: VendorService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit() {
    this.loadVendors();
  }

  loadVendors() {
    this.isLoading = true;
    this.vendorService.getAll(1, 100).subscribe({
      next: (res) => {
        this.vendors = res.items || [];
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل تحميل الموردين'
        });
      },
    });
  }

  openAddDialog() {
    this.isEditMode = false;
    this.currentVendor = {
      vendorCode: this.generateVendorCode(),
      nameAr: '',
      nameEn: '',
      taxNumber: '',
      phone: '',
      email: ''
    };
    this.dialogVisible = true;
  }

  openEditDialog(vendor: Vendor) {
    this.isEditMode = true;
    this.currentVendor = { ...vendor };
    this.dialogVisible = true;
  }

  closeDialog() {
    this.dialogVisible = false;
  }

  generateVendorCode(): string {
    // Generate a default code like VND-001 depending on current count
    const nextNum = this.vendors.length + 1;
    return `VND-${nextNum.toString().padStart(3, '0')}`;
  }

  saveVendor() {
    if (!this.currentVendor.vendorCode || !this.currentVendor.nameAr || !this.currentVendor.nameEn) {
      this.messageService.add({
        severity: 'warn',
        summary: 'تنبيه',
        detail: 'يرجى تعبئة جميع الحقول المطلوبة'
      });
      return;
    }

    if (this.isEditMode && this.currentVendor.id) {
      this.vendorService.update(this.currentVendor.id, this.currentVendor).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'تم بنجاح',
            detail: 'تم تعديل بيانات المورد بنجاح'
          });
          this.loadVendors();
          this.closeDialog();
        },
        error: (err) => {
          const detail = err.error?.detail || err.error || 'فشل تعديل المورد';
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: typeof detail === 'string' ? detail : 'فشل تعديل المورد'
          });
        },
      });
    } else {
      this.vendorService.create(this.currentVendor).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'تم بنجاح',
            detail: 'تمت إضافة المورد بنجاح'
          });
          this.loadVendors();
          this.closeDialog();
        },
        error: (err) => {
          const detail = err.error?.detail || err.error || 'فشل إضافة المورد';
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: typeof detail === 'string' ? detail : 'فشل إضافة المورد'
          });
        },
      });
    }
  }

  confirmDelete(vendor: Vendor) {
    this.confirmationService.confirm({
      message: `هل أنت متأكد من حذف المورد "${vendor.nameAr}"؟`,
      header: 'تأكيد الحذف',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'حذف',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.vendorService.delete(vendor.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'تم بنجاح',
              detail: 'تم حذف المورد بنجاح'
            });
            this.loadVendors();
          },
          error: (err) => {
            const detail = err.error?.detail || err.error || 'فشل حذف المورد';
            this.messageService.add({
              severity: 'error',
              summary: 'خطأ',
              detail: typeof detail === 'string' ? detail : 'فشل حذف المورد'
            });
          },
        });
      },
    });
  }
}
