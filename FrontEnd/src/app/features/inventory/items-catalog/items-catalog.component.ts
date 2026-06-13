import { Component, OnInit } from '@angular/core';
import { StockGroupService, StockGroup, StockGroupTreeDto } from '../../../core/services/stock-group.service';
import { ItemService, Item } from '../../../core/services/item.service';
import { AccountService, AccountDto } from '../../../core/services/account.service';
import { UnitService, Unit } from '../../../core/services/unit.service';
import { SHARED_IMPORTS } from '../../../shared/shared.imports';
import { MessageService, ConfirmationService, TreeNode } from 'primeng/api';

@Component({
  selector: 'app-items-catalog',
  standalone: true,
  imports: [...SHARED_IMPORTS],
  providers: [MessageService, ConfirmationService],
  templateUrl: './items-catalog.component.html',
  styleUrl: './items-catalog.component.scss'
})
export class ItemsCatalogComponent implements OnInit {
  // Stock Groups Tree
  treeNodes: TreeNode[] = [];
  selectedGroupNode: TreeNode | null = null;
  flatGroups: StockGroup[] = [];
  isLoadingTree = false;

  // Items
  items: Item[] = [];
  isLoadingItems = false;
  selectedGroup: StockGroup | null = null;

  // Accounts for dropdown
  inventoryAccounts: AccountDto[] = [];
  salesAccounts: AccountDto[] = [];
  cogsAccounts: AccountDto[] = [];
  expenseAccounts: AccountDto[] = [];
  isLoadingAccounts = false;

  // Units for dropdown
  units: Unit[] = [];
  isLoadingUnits = false;

  // Group Dialog
  groupDialogVisible = false;
  isGroupEditMode = false;
  currentGroup: Partial<StockGroup> & { autoGenerateAccounts?: boolean } = {};

  // Item Dialog
  itemDialogVisible = false;
  isItemEditMode = false;
  currentItem: Partial<Item> = {};

  constructor(
    private groupService: StockGroupService,
    private itemService: ItemService,
    private accountService: AccountService,
    private unitService: UnitService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit() {
    this.loadTree();
    this.loadFlatGroups();
    this.loadAccounts();
    this.loadUnits();
  }

  // ── Stock Groups Tree ─────────────────────────────────────────────
  loadTree() {
    this.isLoadingTree = true;
    this.groupService.getTree().subscribe({
      next: (roots) => {
        this.treeNodes = roots.map(n => this.mapToTreeNode(n));
        this.isLoadingTree = false;
      },
      error: () => {
        this.isLoadingTree = false;
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل شجرة المجموعات' });
      }
    });
  }

  loadFlatGroups() {
    this.groupService.getList().subscribe({
      next: (res) => { this.flatGroups = res || []; }
    });
  }

  mapToTreeNode(dto: StockGroupTreeDto): TreeNode {
    return {
      key: dto.id,
      label: dto.groupNameAr,
      data: dto,
      type: dto.isDetail ? 'detail' : 'parent',
      expanded: true,
      icon: dto.isDetail ? 'pi pi-tag' : 'pi pi-folder',
      children: (dto.subGroups || []).map(c => this.mapToTreeNode(c))
    };
  }

  onNodeSelect(event: any) {
    const node = event.node;
    if (node.type === 'detail') {
      this.selectedGroupNode = node;
      this.selectedGroup = node.data as StockGroup;
      this.loadItems(this.selectedGroup.id);
    } else {
      this.selectedGroupNode = null;
      this.selectedGroup = null;
      this.items = [];
    }
  }

  // ── Items ─────────────────────────────────────────────────────────
  loadItems(groupId: string) {
    this.isLoadingItems = true;
    this.itemService.getAll(1, 1000).subscribe({
      next: (response) => {
        this.items = (response.items || []).filter((item: Item) => item.stockGroupId === groupId);
        this.isLoadingItems = false;
      },
      error: () => {
        this.isLoadingItems = false;
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل الأصناف' });
      }
    });
  }

  // ── Accounts ───────────────────────────────────────────────────────
  loadAccounts() {
    this.isLoadingAccounts = true;
    this.accountService.getAll().subscribe({
      next: (accounts: any[]) => {
        this.inventoryAccounts = accounts.filter((a: any) => a.accountCode.startsWith('12'));
        this.salesAccounts = accounts.filter((a: any) => a.accountCode.startsWith('4'));
        this.cogsAccounts = accounts.filter((a: any) => a.accountCode.startsWith('5'));
        this.expenseAccounts = accounts.filter((a: any) => a.accountCode.startsWith('5'));
        this.isLoadingAccounts = false;
      },
      error: () => {
        this.isLoadingAccounts = false;
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل الحسابات' });
      }
    });
  }

  // ── Units ─────────────────────────────────────────────────────────
  loadUnits() {
    this.isLoadingUnits = true;
    this.unitService.getAll(1, 1000).subscribe({
      next: (response: any) => {
        this.units = response.items || [];
        this.isLoadingUnits = false;
      },
      error: () => {
        this.isLoadingUnits = false;
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل الوحدات' });
      }
    });
  }

  // ── Group Dialog ───────────────────────────────────────────────────
  openAddGroupDialog() {
    this.isGroupEditMode = false;
    this.currentGroup = {
      groupCode: '',
      groupNameAr: '',
      groupNameEn: '',
      isDetail: true,
      parentGroupId: undefined,
      inventoryAccountId: undefined,
      salesAccountId: undefined,
      costOfGoodsSoldAccountId: undefined,
      expenseAccountId: undefined,
      autoGenerateAccounts: true
    };
    this.groupDialogVisible = true;
  }

  openEditGroupDialog(group: StockGroup) {
    this.isGroupEditMode = true;
    this.currentGroup = { ...group, autoGenerateAccounts: false };
    this.groupDialogVisible = true;
  }

  closeGroupDialog() {
    this.groupDialogVisible = false;
    this.currentGroup = {};
  }

  saveGroup() {
    if (!this.currentGroup.groupCode || !this.currentGroup.groupNameAr || !this.currentGroup.groupNameEn) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى تعبئة الحقول المطلوبة' });
      return;
    }

    if (this.currentGroup.autoGenerateAccounts) {
      this.currentGroup.inventoryAccountId = undefined;
      this.currentGroup.salesAccountId = undefined;
      this.currentGroup.costOfGoodsSoldAccountId = undefined;
      this.currentGroup.expenseAccountId = undefined;
    }

    const { autoGenerateAccounts, ...payload } = this.currentGroup;

    if (this.isGroupEditMode && this.currentGroup.id) {
      this.groupService.update(this.currentGroup.id, payload).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم تعديل المجموعة بنجاح' });
          this.loadTree();
          this.loadFlatGroups();
          this.closeGroupDialog();
        },
        error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تعديل المجموعة' })
      });
    } else {
      this.groupService.create(payload).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تمت إضافة المجموعة بنجاح' });
          this.loadTree();
          this.loadFlatGroups();
          this.closeGroupDialog();
        },
        error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل إضافة المجموعة' })
      });
    }
  }

  confirmDeleteGroup(group: StockGroup) {
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
            this.loadTree();
            this.loadFlatGroups();
          },
          error: (err) => this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: err.error?.message || 'فشل الحذف (قد تكون المجموعة مرتبطة بأصناف)'
          })
        });
      }
    });
  }

  // ── Item Dialog ───────────────────────────────────────────────────
  openAddItemDialog() {
    if (!this.selectedGroup) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى اختيار مجموعة أصناف أولاً' });
      return;
    }

    this.isItemEditMode = false;
    this.currentItem = {
      stockGroupId: this.selectedGroup.id,
      itemCode: '',
      itemNameAr: '',
      itemNameEn: '',
      barcode: '',
      defaultPurchasePrice: 0,
      salesPrice: 0,
      isActive: true,
      reorderLevel: 0,
      minimumQuantity: 0,
      maximumQuantity: 0,
      standardCost: 0,
      averageCost: 0
    };
    this.itemDialogVisible = true;
  }

  openEditItemDialog(item: Item) {
    this.isItemEditMode = true;
    this.currentItem = { ...item };
    this.itemDialogVisible = true;
  }

  closeItemDialog() {
    this.itemDialogVisible = false;
    this.currentItem = {};
  }

  saveItem() {
    if (!this.currentItem.itemCode || !this.currentItem.itemNameAr || !this.currentItem.itemNameEn) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى تعبئة الحقول المطلوبة' });
      return;
    }

    if (this.isItemEditMode && this.currentItem.id) {
      this.itemService.update(this.currentItem.id, this.currentItem).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم تعديل الصنف بنجاح' });
          this.loadItems(this.selectedGroup!.id);
          this.closeItemDialog();
        },
        error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تعديل الصنف' })
      });
    } else {
      this.itemService.create(this.currentItem).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تمت إضافة الصنف بنجاح' });
          this.loadItems(this.selectedGroup!.id);
          this.closeItemDialog();
        },
        error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل إضافة الصنف' })
      });
    }
  }

  confirmDeleteItem(item: Item) {
    this.confirmationService.confirm({
      message: `هل أنت متأكد من حذف الصنف "${item.itemNameAr}"؟`,
      header: 'تأكيد الحذف',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'حذف',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.itemService.delete(item.id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم حذف الصنف بنجاح' });
            this.loadItems(this.selectedGroup!.id);
          },
          error: (err) => this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: err.error?.message || 'فشل الحذف (قد يكون الصنف مرتبط بحركات مخزنية)'
          })
        });
      }
    });
  }

  getAccountName(accountId: string | undefined, accounts: AccountDto[]): string {
    const account = accounts.find(a => a.id === accountId);
    return account ? `${account.accountCode} - ${account.accountNameAr}` : '-';
  }

  getUnitName(unitId: string | undefined): string {
    const unit = this.units.find(u => u.id === unitId);
    return unit ? unit.nameAr : '-';
  }
}
