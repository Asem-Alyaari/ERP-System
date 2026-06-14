import { Routes } from '@angular/router';

export const INVENTORY_ROUTES: Routes = [
  {
    path: 'warehouses',
    loadComponent: () => import('./warehouses/warehouses.component').then(m => m.WarehousesComponent)
  },
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
    loadComponent: () => import('./items-catalog/items-catalog.component').then(m => m.ItemsCatalogComponent)
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

