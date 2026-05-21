import { Component, OnInit } from '@angular/core';
import { CategoryService, Category } from '../../../core/services/category.service';
import { SHARED_IMPORTS } from '../../../shared/shared.imports';
import { MessageService, ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-categories',
  standalone: true,
  imports: [...SHARED_IMPORTS],
  providers: [MessageService, ConfirmationService],
  templateUrl: './categories.component.html',
})
export class CategoriesComponent implements OnInit {
  categories: Category[] = [];
  isLoading = false;

  // Dialog state
  dialogVisible = false;
  isEditMode = false;
  currentCategory: Partial<Category> = {};

  constructor(
    private categoryService: CategoryService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit() {
    this.loadCategories();
  }

  loadCategories() {
    this.isLoading = true;
    this.categoryService.getAll(1, 100).subscribe({
      next: (res) => {
        this.categories = res.items || [];
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل التصنيفات' });
      },
    });
  }

  openAddDialog() {
    this.isEditMode = false;
    this.currentCategory = { nameAr: '', nameEn: '' };
    this.dialogVisible = true;
  }

  openEditDialog(category: Category) {
    this.isEditMode = true;
    this.currentCategory = { ...category };
    this.dialogVisible = true;
  }

  closeDialog() {
    this.dialogVisible = false;
  }

  saveCategory() {
    if (!this.currentCategory.nameAr || !this.currentCategory.nameEn) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى تعبئة الحقول المطلوبة' });
      return;
    }

    if (this.isEditMode && this.currentCategory.id) {
      this.categoryService.update(this.currentCategory.id, this.currentCategory).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم تعديل التصنيف بنجاح' });
          this.loadCategories();
          this.closeDialog();
        },
        error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تعديل التصنيف' }),
      });
    } else {
      this.categoryService.create(this.currentCategory).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تمت إضافة التصنيف بنجاح' });
          this.loadCategories();
          this.closeDialog();
        },
        error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل إضافة التصنيف' }),
      });
    }
  }

  confirmDelete(category: Category) {
    this.confirmationService.confirm({
      message: `هل أنت متأكد من حذف التصنيف "${category.nameAr}"؟`,
      header: 'تأكيد الحذف',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'حذف',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.categoryService.delete(category.id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم حذف التصنيف بنجاح' });
            this.loadCategories();
          },
          error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل حذف التصنيف' }),
        });
      },
    });
  }
}
