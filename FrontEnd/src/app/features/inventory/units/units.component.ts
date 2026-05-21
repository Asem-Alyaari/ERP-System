import { Component, OnInit } from '@angular/core';
import { UnitService, Unit } from '../../../core/services/unit.service';
import { SHARED_IMPORTS } from '../../../shared/shared.imports';
import { MessageService, ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-units',
  standalone: true,
  imports: [...SHARED_IMPORTS],
  providers: [MessageService, ConfirmationService],
  templateUrl: './units.component.html',
})
export class UnitsComponent implements OnInit {
  units: Unit[] = [];
  isLoading = false;

  // Dialog state
  dialogVisible = false;
  isEditMode = false;
  currentUnit: Partial<Unit> = {};

  constructor(
    private unitService: UnitService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit() {
    this.loadUnits();
  }

  loadUnits() {
    this.isLoading = true;
    this.unitService.getAll(1, 100).subscribe({
      next: (res) => {
        this.units = res.items || [];
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل تحميل الوحدات',
        });
      },
    });
  }

  openAddDialog() {
    this.isEditMode = false;
    this.currentUnit = { nameAr: '', nameEn: '', shortName: '' };
    this.dialogVisible = true;
  }

  openEditDialog(unit: Unit) {
    this.isEditMode = true;
    this.currentUnit = { ...unit };
    this.dialogVisible = true;
  }

  closeDialog() {
    this.dialogVisible = false;
  }

  saveUnit() {
    if (!this.currentUnit.nameAr || !this.currentUnit.nameEn) {
      this.messageService.add({
        severity: 'warn',
        summary: 'تنبيه',
        detail: 'يرجى تعبئة الحقول المطلوبة',
      });
      return;
    }

    if (this.isEditMode && this.currentUnit.id) {
      this.unitService.update(this.currentUnit.id, this.currentUnit).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم تعديل الوحدة بنجاح' });
          this.loadUnits();
          this.closeDialog();
        },
        error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تعديل الوحدة' }),
      });
    } else {
      this.unitService.create(this.currentUnit).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تمت إضافة الوحدة بنجاح' });
          this.loadUnits();
          this.closeDialog();
        },
        error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل إضافة الوحدة' }),
      });
    }
  }

  confirmDelete(unit: Unit) {
    this.confirmationService.confirm({
      message: `هل أنت متأكد من حذف الوحدة "${unit.nameAr}"؟`,
      header: 'تأكيد الحذف',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'حذف',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.unitService.delete(unit.id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم حذف الوحدة بنجاح' });
            this.loadUnits();
          },
          error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل حذف الوحدة' }),
        });
      },
    });
  }
}
