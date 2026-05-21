import { Component, OnInit } from '@angular/core';
import { ItemService, Item } from '../../../core/services/item.service';
import { StockGroupService, StockGroup } from '../../../core/services/stock-group.service';
import { CategoryService, Category } from '../../../core/services/category.service';
import { UnitService, Unit } from '../../../core/services/unit.service';
import { SHARED_IMPORTS } from '../../../shared/shared.imports';
import { MessageService, ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-items',
  standalone: true,
  imports: [...SHARED_IMPORTS],
  providers: [MessageService, ConfirmationService],
  templateUrl: './items.component.html',
})
export class ItemsComponent implements OnInit {
  items: Item[] = [];
  stockGroups: StockGroup[] = [];
  categories: Category[] = [];
  units: Unit[] = [];
  isLoading = false;

  // Dialog state
  dialogVisible = false;
  isEditMode = false;
  currentItem: Partial<Item> = {};

  // Multi-UOM state
  tempUnitId = '';
  tempParentUnitId = '';
  tempMultiplier = 1;
  tempPrice = 0;

  // Dialog tab state
  activeTab = 0;

  constructor(
    private itemService: ItemService,
    private groupService: StockGroupService,
    private categoryService: CategoryService,
    private unitService: UnitService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit() {
    this.loadItems();
    this.loadDropdowns();
  }

  loadItems() {
    this.isLoading = true;
    this.itemService.getAll(1, 100).subscribe({
      next: (res) => {
        this.items = res.items || [];
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل تحميل الأصناف المضافة',
        });
      },
    });
  }

  loadDropdowns() {
    // 1. Load Detail Stock Groups
    this.groupService.getList().subscribe({
      next: (res) => {
        this.stockGroups = (res || []).filter(g => g.isDetail);
      }
    });

    // 2. Load Categories
    this.categoryService.getAll(1, 200).subscribe({
      next: (res) => {
        this.categories = res.items || [];
      }
    });

    // 3. Load Units
    this.unitService.getAll(1, 200).subscribe({
      next: (res) => {
        this.units = res.items || [];
      }
    });
  }

  getUnitName(id: string): string {
    const u = this.units.find(x => x.id === id);
    return u ? u.nameAr : '';
  }

  addUnit() {
    if (!this.tempUnitId) {
      this.messageService.add({
        severity: 'warn',
        summary: 'تنبيه',
        detail: 'يرجى اختيار وحدة القياس أولاً',
      });
      return;
    }

    if (!this.currentItem.itemUnits) {
      this.currentItem.itemUnits = [];
    }

    if (this.currentItem.itemUnits.some(u => u.unitId === this.tempUnitId)) {
      this.messageService.add({
        severity: 'warn',
        summary: 'تنبيه',
        detail: 'هذه الوحدة مضافة بالفعل للمنتج',
      });
      return;
    }

    const isBase = this.currentItem.itemUnits.length === 0;
    
    if (isBase) {
      const newUnit = {
        unitId: this.tempUnitId,
        unitNameAr: this.getUnitName(this.tempUnitId),
        isBaseUnit: true,
        conversionRate: 1,
        multiplier: 1,
        price: this.currentItem.salesPrice || 0
      };
      this.currentItem.itemUnits.push(newUnit);
      // Automatically set safety stock unit to the base unit
      if (!this.currentItem.safetyStockUnitId) {
        this.currentItem.safetyStockUnitId = this.tempUnitId;
      }
    } else {
      if (!this.tempParentUnitId) {
        this.messageService.add({
          severity: 'warn',
          summary: 'تنبيه',
          detail: 'يرجى اختيار الوحدة التي تحتوي عليها هذه الوحدة',
        });
        return;
      }

      if (this.tempMultiplier <= 0) {
        this.messageService.add({
          severity: 'warn',
          summary: 'تنبيه',
          detail: 'يجب أن يكون معامل التعبئة أكبر من صفر',
        });
        return;
      }

      const parentUnit = this.currentItem.itemUnits.find(u => u.unitId === this.tempParentUnitId);
      if (!parentUnit) return;

      const rate = this.tempMultiplier * parentUnit.conversionRate;

      const calculatedPrice = (this.currentItem.salesPrice || 0) * rate;
      const newUnit = {
        unitId: this.tempUnitId,
        unitNameAr: this.getUnitName(this.tempUnitId),
        isBaseUnit: false,
        conversionRate: rate,
        multiplier: this.tempMultiplier,
        parentUnitId: this.tempParentUnitId,
        price: this.tempPrice || calculatedPrice
      };
      this.currentItem.itemUnits.push(newUnit);
    }

    // Reset fields
    this.tempUnitId = '';
    this.tempParentUnitId = '';
    this.tempMultiplier = 1;
    this.tempPrice = 0;
  }

  deleteUnit(unit: any) {
    if (!this.currentItem.itemUnits) return;

    // Prevent deleting base unit if other units exist
    if (unit.isBaseUnit && this.currentItem.itemUnits.length > 1) {
      this.messageService.add({
        severity: 'error',
        summary: 'خطأ',
        detail: 'لا يمكن حذف الوحدة الأساسية وهناك وحدات فرعية مضافة. يرجى حذف الوحدات الأخرى أولاً',
      });
      return;
    }

    // Prevent deleting a unit if it is a parent to another unit
    const hasChildren = this.currentItem.itemUnits.some(u => u.parentUnitId === unit.unitId);
    if (hasChildren) {
      this.messageService.add({
        severity: 'error',
        summary: 'خطأ',
        detail: 'لا يمكن حذف هذه الوحدة لأن هناك وحدة أخرى تعتمد عليها كمرجع للتعبئة. يرجى حذف الوحدات التابعة أولاً',
      });
      return;
    }

    this.currentItem.itemUnits = this.currentItem.itemUnits.filter(u => u.unitId !== unit.unitId);
    
    // Clear safetyStockUnitId if empty
    if (this.currentItem.itemUnits.length === 0) {
      this.currentItem.safetyStockUnitId = undefined;
    }
  }

  reconstructSequentialUnits(itemUnits: any[]): any[] {
    if (!itemUnits || itemUnits.length === 0) return [];
    
    // Sort by conversion rate ascending
    const sorted = [...itemUnits].sort((a, b) => a.conversionRate - b.conversionRate);
    
    // Set base unit properties
    sorted[0].isBaseUnit = true;
    sorted[0].conversionRate = 1;
    sorted[0].multiplier = 1;
    sorted[0].parentUnitId = undefined;

    for (let i = 1; i < sorted.length; i++) {
      const current = sorted[i];
      const parent = sorted[i - 1];
      
      current.isBaseUnit = false;
      current.parentUnitId = parent.unitId;
      current.multiplier = current.conversionRate / parent.conversionRate;
    }
    
    return sorted;
  }

  openAddDialog() {
    this.isEditMode = false;
    this.currentItem = {
      itemCode: '',
      itemNameAr: '',
      itemNameEn: '',
      stockGroupId: '',
      categoryId: undefined,
      sku: '',
      barcode: '',
      defaultPurchasePrice: 0,
      salesPrice: 0,
      isActive: true,
      reorderLevel: 0,
      minimumQuantity: 0,
      maximumQuantity: 0,
      safetyStockUnitId: undefined,
      itemUnits: []
    };
    // Reset temp inputs
    this.tempUnitId = '';
    this.tempParentUnitId = '';
    this.tempMultiplier = 1;
    this.tempPrice = 0;
    this.activeTab = 0;
    this.dialogVisible = true;
  }

  openEditDialog(item: Item) {
    this.isEditMode = true;
    this.currentItem = { 
      ...item,
      itemUnits: this.reconstructSequentialUnits(item.itemUnits || [])
    };
    // Reset temp inputs
    this.tempUnitId = '';
    this.tempParentUnitId = '';
    this.tempMultiplier = 1;
    this.tempPrice = 0;
    this.activeTab = 0;
    this.dialogVisible = true;
  }

  closeDialog() {
    this.dialogVisible = false;
  }

  saveItem() {
    if (!this.currentItem.itemCode || !this.currentItem.itemNameAr || !this.currentItem.itemNameEn || !this.currentItem.stockGroupId) {
      this.messageService.add({
        severity: 'warn',
        summary: 'تنبيه',
        detail: 'يرجى تعبئة الحقول المطلوبة الكود والاسم والمجموعة المخزنية',
      });
      return;
    }

    if (this.isEditMode && this.currentItem.id) {
      this.itemService.update(this.currentItem.id, this.currentItem).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم تعديل بيانات المنتج بنجاح' });
          this.loadItems();
          this.closeDialog();
        },
        error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تعديل المنتج' }),
      });
    } else {
      this.itemService.create(this.currentItem).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم إضافة المنتج بنجاح' });
          this.loadItems();
          this.closeDialog();
        },
        error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل إضافة المنتج' }),
      });
    }
  }

  confirmDelete(item: Item) {
    this.confirmationService.confirm({
      message: `هل أنت متأكد من حذف المنتج "${item.itemNameAr}"؟`,
      header: 'تأكيد الحذف',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'حذف',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.itemService.delete(item.id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم حذف المنتج بنجاح' });
            this.loadItems();
          },
          error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل حذف المنتج (قد يكون مرتبطاً بحركات مخزنية)' }),
        });
      },
    });
  }

  getStockGroupName(id: string): string {
    const sg = this.stockGroups.find(x => x.id === id);
    return sg ? sg.groupNameAr : '';
  }

  getCategoryName(id?: string): string {
    if (!id) return '';
    const cat = this.categories.find(x => x.id === id);
    return cat ? cat.nameAr : '';
  }
}
