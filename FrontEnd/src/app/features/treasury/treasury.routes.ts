import { Routes } from '@angular/router';

export const TREASURY_ROUTES: Routes = [
  {
    path: 'payment-vouchers',
    loadComponent: () => import('./payment-vouchers/payment-vouchers.component').then(m => m.PaymentVouchersComponent)
  },
  {
    path: '',
    redirectTo: 'payment-vouchers',
    pathMatch: 'full'
  }
];
