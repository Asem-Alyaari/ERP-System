import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { LayoutComponent } from './layout/layout.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { 
    path: '', 
    component: LayoutComponent,
    children: [
      { path: '', component: DashboardComponent },
      { 
        path: 'inventory', 
        loadChildren: () => import('./features/inventory/inventory.routes').then(m => m.INVENTORY_ROUTES) 
      },
      { 
        path: 'purchases', 
        loadChildren: () => import('./features/purchasing/purchasing.routes').then(m => m.PURCHASING_ROUTES) 
      },
      { 
        path: 'sales', 
        loadChildren: () => import('./features/sales/sales.routes').then(m => m.SALES_ROUTES) 
      },
      {
        path: 'accounting',
        loadChildren: () => import('./features/accounting/accounting.routes').then(m => m.ACCOUNTING_ROUTES)
      },
      {
        path: 'treasury',
        loadChildren: () => import('./features/treasury/treasury.routes').then(m => m.TREASURY_ROUTES)
      }

    ]
  },
  { path: '**', redirectTo: '' }
];
