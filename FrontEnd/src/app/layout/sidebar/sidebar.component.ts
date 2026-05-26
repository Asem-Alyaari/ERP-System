import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { SHARED_IMPORTS } from '../../shared/shared.imports';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [...SHARED_IMPORTS],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss'
})
export class SidebarComponent {
  @Input() collapsed = false;
  @Output() toggleCollapse = new EventEmitter<void>();

  menuItems = [
    { label: 'الرئيسية', icon: 'pi pi-home', route: '/' },
    { label: 'الأصناف (المنتجات)', icon: 'pi pi-box', route: '/inventory/items' },
    { label: 'مجموعات الأصناف', icon: 'pi pi-folder', route: '/inventory/stock-groups' },
    { label: 'التصنيفات', icon: 'pi pi-tags', route: '/inventory/categories' },
    { label: 'الوحدات', icon: 'pi pi-percentage', route: '/inventory/units' },
    { label: 'العملاء', icon: 'pi pi-user-plus', route: '/sales/customers' },
    { label: 'الموردين', icon: 'pi pi-users', route: '/purchases/vendors' },
    { label: 'دليل الحسابات', icon: 'pi pi-sitemap', route: '/accounting' },
    { label: 'القيود اليومية', icon: 'pi pi-book', route: '/accounting/journal-entries' },
    { label: 'كشف الحساب', icon: 'pi pi-file-text', route: '/accounting/ledger-report' },
    { label: 'مراكز التكلفة', icon: 'pi pi-sitemap', route: '/accounting/cost-centers' },
    { label: 'الفترات المالية', icon: 'pi pi-calendar', route: '/accounting/fiscal-periods' },
    { label: 'أسعار الصرف', icon: 'pi pi-sync', route: '/accounting/exchange-rates' },
    { label: 'سندات الصرف', icon: 'pi pi-money-bill', route: '/treasury/payment-vouchers' },
    { label: 'سندات القبض', icon: 'pi pi-wallet', route: '/treasury/receipt-vouchers' },
    { label: 'فواتير المصروفات', icon: 'pi pi-file-invoice-dollar', route: '/treasury/expense-bills' },
    { label: 'التقارير', icon: 'pi pi-chart-bar', route: '/reports' },
    { label: 'الإعدادات', icon: 'pi pi-cog', route: '/settings' }
  ];


  constructor(public router: Router) {}
}
