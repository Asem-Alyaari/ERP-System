import { Component, OnInit } from '@angular/core';
import { SHARED_IMPORTS } from '../../../shared/shared.imports';
import { MessageService, ConfirmationService } from 'primeng/api';
import { CostCenterService, CostCenterLookup, CostCenterDto } from '../../../core/services/cost-center.service';

interface TreeNode {
  key: string;
  label: string;
  data: CostCenterLookup;
  children: TreeNode[];
}

@Component({
  selector: 'app-cost-centers',
  standalone: true,
  imports: [...SHARED_IMPORTS],
  providers: [MessageService, ConfirmationService],
  templateUrl: './cost-centers.component.html',
  styleUrl: './cost-centers.component.scss'
})
export class CostCentersComponent implements OnInit {

  // ── List state ──────────────────────────────────────────────
  costCenters: CostCenterLookup[] = [];
  isLoading = false;

  // ── Create/Edit Dialog ────────────────────────────────────────
  dialogVisible = false;
  isSaving = false;
  isEditMode = false;
  
  editedCostCenter: Partial<CostCenterDto> = {};

  // ── Tree View ────────────────────────────────────────────────
  treeData: TreeNode[] = [];

  constructor(
    private costCenterService: CostCenterService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.loadCostCenters();
  }

  // ── Load Data ────────────────────────────────────────────────

  loadCostCenters(): void {
    this.isLoading = true;
    this.costCenterService.getAll(false).subscribe({
      next: (data) => {
        this.costCenters = data || [];
        this.buildTree();
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل مراكز التكلفة' });
      }
    });
  }

  buildTree(): void {
    // Build hierarchical tree from flat list
    const map = new Map<string, TreeNode>();
    const roots: TreeNode[] = [];

    this.costCenters.forEach(cc => {
      map.set(cc.id, {
        key: cc.id,
        label: `${cc.costCenterCode} - ${cc.costCenterNameAr}`,
        data: cc,
        children: []
      });
    });

    this.costCenters.forEach(cc => {
      const node = map.get(cc.id);
      if (cc.parentCostCenterId && map.has(cc.parentCostCenterId)) {
        map.get(cc.parentCostCenterId)!.children.push(node!);
      } else {
        roots.push(node!);
      }
    });

    this.treeData = roots;
  }

  // ── Create Dialog ─────────────────────────────────────────────

  openCreateDialog(): void {
    this.isEditMode = false;
    this.editedCostCenter = {
      costCenterCode: '',
      costCenterNameAr: '',
      costCenterNameEn: '',
      isDetail: true,
      parentCostCenterId: undefined
    };
    this.dialogVisible = true;
  }

  openEditDialog(costCenter: CostCenterLookup): void {
    this.isEditMode = true;
    this.editedCostCenter = {
      id: costCenter.id,
      costCenterCode: costCenter.costCenterCode,
      costCenterNameAr: costCenter.costCenterNameAr,
      costCenterNameEn: costCenter.costCenterNameEn,
      isDetail: costCenter.isDetail,
      parentCostCenterId: costCenter.parentCostCenterId
    };
    this.dialogVisible = true;
  }

  saveCostCenter(): void {
    if (!this.editedCostCenter.costCenterCode || !this.editedCostCenter.costCenterNameAr) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى ملء الحقول المطلوبة' });
      return;
    }

    this.isSaving = true;

    if (this.isEditMode) {
      this.costCenterService.update(this.editedCostCenter.id!, this.editedCostCenter).subscribe({
        next: () => {
          this.isSaving = false;
          this.dialogVisible = false;
          this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم تحديث مركز التكلفة بنجاح' });
          this.loadCostCenters();
        },
        error: (err) => {
          this.isSaving = false;
          this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل تحديث مركز التكلفة' });
        }
      });
    } else {
      this.costCenterService.create(this.editedCostCenter).subscribe({
        next: () => {
          this.isSaving = false;
          this.dialogVisible = false;
          this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم إنشاء مركز التكلفة بنجاح' });
          this.loadCostCenters();
        },
        error: (err) => {
          this.isSaving = false;
          this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل إنشاء مركز التكلفة' });
        }
      });
    }
  }

  // ── Delete ───────────────────────────────────────────────────

  confirmDelete(costCenter: CostCenterLookup): void {
    this.confirmationService.confirm({
      message: `هل تريد حذف مركز التكلفة "${costCenter.costCenterNameAr}"؟`,
      header: 'تأكيد الحذف',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'حذف',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.costCenterService.delete(costCenter.id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم حذف مركز التكلفة بنجاح' });
            this.loadCostCenters();
          },
          error: (err) => {
            this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل حذف مركز التكلفة' });
          }
        });
      }
    });
  }

  // ── Helpers ───────────────────────────────────────────────────

  getParentOptions(): CostCenterLookup[] {
    // Filter to show only parent nodes (non-detail) for selection
    return this.costCenters.filter(cc => !cc.isDetail);
  }

  getDetailLabel(isDetail: boolean): string {
    return isDetail ? 'تفصيلي' : 'رئيسي';
  }

  getDetailSeverity(isDetail: boolean): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' {
    return isDetail ? 'success' : 'info';
  }

  getParentCode(parentId: string | undefined): string {
    if (!parentId) return '-';
    const parent = this.costCenters.find(c => c.id === parentId);
    return parent ? parent.costCenterCode : '-';
  }
}
