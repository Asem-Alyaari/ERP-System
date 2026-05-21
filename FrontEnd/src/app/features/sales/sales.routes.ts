import { Routes } from '@angular/router';

export const SALES_ROUTES: Routes = [
  {
    path: 'customers',
    loadComponent: () => import('./customers/customers.component').then(m => m.CustomersComponent)
  },
  {
    path: '',
    redirectTo: 'customers',
    pathMatch: 'full'
  }
];
