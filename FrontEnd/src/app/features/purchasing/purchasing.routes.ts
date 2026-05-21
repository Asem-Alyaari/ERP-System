import { Routes } from '@angular/router';

export const PURCHASING_ROUTES: Routes = [
  {
    path: 'vendors',
    loadComponent: () => import('./vendors/vendors.component').then(m => m.VendorsComponent)
  },
  {
    path: '',
    redirectTo: 'vendors',
    pathMatch: 'full'
  }
];
