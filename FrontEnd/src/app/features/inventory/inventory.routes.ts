import { Routes } from '@angular/router';

export const INVENTORY_ROUTES: Routes = [
  {
    path: 'categories',
    loadComponent: () => import('./categories/categories.component').then(m => m.CategoriesComponent)
  },
  {
    path: 'units',
    loadComponent: () => import('./units/units.component').then(m => m.UnitsComponent)
  },
  {
    path: 'stock-groups',
    loadComponent: () => import('./stock-groups/stock-groups.component').then(m => m.StockGroupsComponent)
  },
  {
    path: 'items',
    loadComponent: () => import('./items/items.component').then(m => m.ItemsComponent)
  },
  {
    path: '',
    redirectTo: 'items',
    pathMatch: 'full'
  }
];

