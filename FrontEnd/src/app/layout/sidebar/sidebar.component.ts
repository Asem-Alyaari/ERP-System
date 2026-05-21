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
    { label: 'الحسابات', icon: 'pi pi-wallet', route: '/accounting' },
    { label: 'التقارير', icon: 'pi pi-chart-bar', route: '/reports' },
    { label: 'الإعدادات', icon: 'pi pi-cog', route: '/settings' }
  ];


  constructor(public router: Router) {}
}
