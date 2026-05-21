import { Component, OnInit } from '@angular/core';
import { StockGroupService, StockGroup } from '../../../core/services/stock-group.service';
import { SHARED_IMPORTS } from '../../../shared/shared.imports';
import { MessageService, ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-stock-groups',
  standalone: true,
  imports: [...SHARED_IMPORTS],
  providers: [MessageService, ConfirmationService],
  templateUrl: './stock-groups.component.html',
})
export class StockGroupsComponent implements OnInit {
  groups: StockGroup[] = [];
  flatGroups: StockGroup[] = []; // for dropdown selector
  isLoading = false;

  // Dialog state
  dialogVisible = false;
  isEditMode = false;
  currentGroup: Partial<StockGroup> = {};

  constructor(
    private groupService: StockGroupService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) { }

  ngOnInit() {
    this.loadGroups();
    this.loadFlatGroups();
  }

  loadGroups() {
    this.isLoading = true;
    this.groupService.getAll(1, 100).subscribe({
      next: (res) => {
        this.groups = res.items || [];
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل تحميل مجموعات الأصناف',
        });
      },
    });
  }

  loadFlatGroups() {
    this.groupService.getList().subscribe({
      next: (res) => {
        this.flatGroups = res || [];
      },
    });
  }

  openAddDialog() {
    this.isEditMode = false;
    this.currentGroup = {
      groupCode: '',
      groupNameAr: '',
      groupNameEn: '',
      isDetail: true,
      parentGroupId: undefined,
      inventoryAccountId: undefined,
      salesAccountId: undefined,
      costOfGoodsSoldAccountId: undefined
    };
    this.dialogVisible = true;
  }

  openEditDialog(group: StockGroup) {
    this.isEditMode = true;
    this.currentGroup = { ...group };
    this.dialogVisible = true;
  }

  closeDialog() {
    this.dialogVisible = false;
  }

  saveGroup() {
    if (!this.currentGroup.groupCode || !this.currentGroup.groupNameAr || !this.currentGroup.groupNameEn) {
      this.messageService.add({
        severity: 'warn',
        summary: 'تنبيه',
        detail: 'يرجى تعبئة الحقول المطلوبة',
      });
      return;
    }

    if (this.isEditMode && this.currentGroup.id) {
      this.groupService.update(this.currentGroup.id, this.currentGroup).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم تعديل المجموعة بنجاح' });
          this.loadGroups();
          this.loadFlatGroups();
          this.closeDialog();
        },
        error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تعديل المجموعة' }),
      });
    } else {
      this.groupService.create(this.currentGroup).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تمت إضافة المجموعة بنجاح' });
          this.loadGroups();
          this.loadFlatGroups();
          this.closeDialog();
        },
        error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل إضافة المجموعة' }),
      });
    }
  }

  confirmDelete(group: StockGroup) {
    this.confirmationService.confirm({
      message: `هل أنت متأكد من حذف المجموعة "${group.groupNameAr}"؟`,
      header: 'تأكيد الحذف',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'حذف',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.groupService.delete(group.id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم حذف المجموعة بنجاح' });
            this.loadGroups();
            this.loadFlatGroups();
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: 'خطأ',
              detail: err.error?.message || 'فشل حذف المجموعة (قد تكون مرتبطة بأصناف)'
            });
          },
        });
      },
    });
  }

  getParentGroupName(parentId?: string): string {
    if (!parentId) return '';
    const parent = this.flatGroups.find(g => g.id === parentId);
    return parent ? parent.groupNameAr : '';
  }
}
