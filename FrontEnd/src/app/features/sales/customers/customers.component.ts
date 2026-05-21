import { Component, OnInit } from '@angular/core';
import { CustomerService, Customer } from '../../../core/services/customer.service';
import { SHARED_IMPORTS } from '../../../shared/shared.imports';
import { MessageService, ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [...SHARED_IMPORTS],
  providers: [MessageService, ConfirmationService],
  templateUrl: './customers.component.html',
})
export class CustomersComponent implements OnInit {
  customers: Customer[] = [];
  isLoading = false;

  // Dialog state
  dialogVisible = false;
  isEditMode = false;
  currentCustomer: Partial<Customer> = {};

  constructor(
    private customerService: CustomerService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit() {
    this.loadCustomers();
  }

  loadCustomers() {
    this.isLoading = true;
    this.customerService.getAll(1, 100).subscribe({
      next: (res) => {
        this.customers = res.items || [];
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل تحميل العملاء'
        });
      },
    });
  }

  openAddDialog() {
    this.isEditMode = false;
    this.currentCustomer = {
      customerCode: this.generateCustomerCode(),
      nameAr: '',
      nameEn: '',
      taxNumber: '',
      phone: '',
      email: ''
    };
    this.dialogVisible = true;
  }

  openEditDialog(customer: Customer) {
    this.isEditMode = true;
    this.currentCustomer = { ...customer };
    this.dialogVisible = true;
  }

  closeDialog() {
    this.dialogVisible = false;
  }

  generateCustomerCode(): string {
    const nextNum = this.customers.length + 1;
    return `CUST-${nextNum.toString().padStart(3, '0')}`;
  }

  saveCustomer() {
    if (!this.currentCustomer.customerCode || !this.currentCustomer.nameAr || !this.currentCustomer.nameEn) {
      this.messageService.add({
        severity: 'warn',
        summary: 'تنبيه',
        detail: 'يرجى تعبئة جميع الحقول المطلوبة'
      });
      return;
    }

    if (this.isEditMode && this.currentCustomer.id) {
      this.customerService.update(this.currentCustomer.id, this.currentCustomer).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'تم بنجاح',
            detail: 'تم تعديل بيانات العميل بنجاح'
          });
          this.loadCustomers();
          this.closeDialog();
        },
        error: (err) => {
          const detail = err.error?.detail || err.error || 'فشل تعديل العميل';
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: typeof detail === 'string' ? detail : 'فشل تعديل العميل'
          });
        },
      });
    } else {
      this.customerService.create(this.currentCustomer).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'تم بنجاح',
            detail: 'تمت إضافة العميل بنجاح'
          });
          this.loadCustomers();
          this.closeDialog();
        },
        error: (err) => {
          const detail = err.error?.detail || err.error || 'فشل إضافة العميل';
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: typeof detail === 'string' ? detail : 'فشل إضافة العميل'
          });
        },
      });
    }
  }

  confirmDelete(customer: Customer) {
    this.confirmationService.confirm({
      message: `هل أنت متأكد من حذف العميل "${customer.nameAr}"؟`,
      header: 'تأكيد الحذف',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'حذف',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.customerService.delete(customer.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'تم بنجاح',
              detail: 'تم حذف العميل بنجاح'
            });
            this.loadCustomers();
          },
          error: (err) => {
            const detail = err.error?.detail || err.error || 'فشل حذف العميل';
            this.messageService.add({
              severity: 'error',
              summary: 'خطأ',
              detail: typeof detail === 'string' ? detail : 'فشل حذف العميل'
            });
          },
        });
      },
    });
  }
}
