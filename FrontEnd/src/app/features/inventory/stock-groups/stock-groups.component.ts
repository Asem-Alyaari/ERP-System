import { Component, OnInit } from '@angular/core';
import { StockGroupService, StockGroup, StockGroupTreeDto } from '../../../core/services/stock-group.service';
import { SHARED_IMPORTS } from '../../../shared/shared.imports';
import { MessageService, ConfirmationService, TreeNode } from 'primeng/api';

@Component({
  selector: 'app-stock-groups',
  standalone: true,
  imports: [...SHARED_IMPORTS],
  providers: [MessageService, ConfirmationService],
  templateUrl: './stock-groups.component.html',
  styleUrl: './stock-groups.component.scss',
})
export class StockGroupsComponent implements OnInit {
  // ── بيانات الشجرة ────────────────────────────────────────────────────
  treeNodes: TreeNode[] = [];
  flatGroups: StockGroup[] = [];   // كل المجموعات (للـ Parent Selector)
  isLoading = false;

  // ── حالة الـ Dialog ───────────────────────────────────────────────────
  dialogVisible   = false;
  isEditMode      = false;
  currentGroup: Partial<StockGroup> & { autoGenerateAccounts?: boolean } = {};

  constructor(
    private groupService: StockGroupService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit() {
    this.loadTree();
    this.loadFlatGroups();
  }

  // ── تحميل الشجرة من الـ API ──────────────────────────────────────────
  loadTree() {
    this.isLoading = true;
    this.groupService.getTree().subscribe({
      next: (roots) => {
        this.treeNodes = roots.map(n => this.mapToTreeNode(n));
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل شجرة المجموعات' });
      },
    });
  }

  // ── تحميل القائمة المسطحة للـ Dropdown ──────────────────────────────
  loadFlatGroups() {
    this.groupService.getList().subscribe({
      next: (res) => { this.flatGroups = res || []; },
    });
  }

  // ── تحويل الـ DTO الشجري إلى PrimeNG TreeNode ────────────────────────
  mapToTreeNode(dto: StockGroupTreeDto): TreeNode {
    return {
      key:      dto.id,
      label:    dto.groupNameAr,
      data:     dto,
      type:     dto.isDetail ? 'detail' : 'parent',
      expanded: true,
      icon:     dto.isDetail ? 'pi pi-tag' : 'pi pi-folder',
      children: (dto.subGroups || []).map(c => this.mapToTreeNode(c)),
    };
  }

  // ── Dialog: إضافة ────────────────────────────────────────────────────
  openAddDialog() {
    this.isEditMode   = false;
    this.currentGroup = {
      groupCode: '', groupNameAr: '', groupNameEn: '',
      isDetail: true, parentGroupId: undefined,
      inventoryAccountId: undefined,
      salesAccountId: undefined,
      costOfGoodsSoldAccountId: undefined,
      autoGenerateAccounts: true,
    };
    this.dialogVisible = true;
  }

  // ── Dialog: تعديل ────────────────────────────────────────────────────
  openEditDialog(group: StockGroup) {
    this.isEditMode   = true;
    this.currentGroup = { ...group, autoGenerateAccounts: false };
    this.dialogVisible = true;
  }

  closeDialog() { this.dialogVisible = false; }

  // ── حفظ المجموعة ─────────────────────────────────────────────────────
  saveGroup() {
    if (!this.currentGroup.groupCode || !this.currentGroup.groupNameAr || !this.currentGroup.groupNameEn) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى تعبئة الحقول المطلوبة' });
      return;
    }

    // إذا كان التوليد التلقائي مفعّلاً، أفرِغ الحسابات اليدوية
    if (this.currentGroup.autoGenerateAccounts) {
      this.currentGroup.inventoryAccountId       = undefined;
      this.currentGroup.salesAccountId           = undefined;
      this.currentGroup.costOfGoodsSoldAccountId = undefined;
    }

    const { autoGenerateAccounts, ...payload } = this.currentGroup;

    if (this.isEditMode && this.currentGroup.id) {
      this.groupService.update(this.currentGroup.id, payload).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم تعديل المجموعة بنجاح' });
          this.loadTree(); this.loadFlatGroups(); this.closeDialog();
        },
        error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تعديل المجموعة' }),
      });
    } else {
      this.groupService.create(payload).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تمت إضافة المجموعة بنجاح' });
          this.loadTree(); this.loadFlatGroups(); this.closeDialog();
        },
        error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل إضافة المجموعة' }),
      });
    }
  }

  // ── حذف المجموعة ─────────────────────────────────────────────────────
  confirmDelete(group: StockGroup) {
    this.confirmationService.confirm({
      message:  `هل أنت متأكد من حذف المجموعة "${group.groupNameAr}"؟`,
      header:   'تأكيد الحذف',
      icon:     'pi pi-exclamation-triangle',
      acceptLabel: 'حذف',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.groupService.delete(group.id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم حذف المجموعة بنجاح' });
            this.loadTree(); this.loadFlatGroups();
          },
          error: (err) => this.messageService.add({
            severity: 'error', summary: 'خطأ',
            detail: err.error?.message || 'فشل الحذف (قد تكون المجموعة مرتبطة بأصناف)',
          }),
        });
      },
    });
  }
}
