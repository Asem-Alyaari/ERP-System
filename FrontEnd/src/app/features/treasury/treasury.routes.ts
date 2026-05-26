import { Routes } from '@angular/router';

export const TREASURY_ROUTES: Routes = [
  {
    path: 'payment-vouchers',
    loadComponent: () => import('./payment-vouchers/payment-vouchers.component').then(m => m.PaymentVouchersComponent)
  },
  {
    path: 'receipt-vouchers',
    loadComponent: () => import('./receipt-vouchers/receipt-vouchers.component').then(m => m.ReceiptVouchersComponent)
  },
  {
    path: 'expense-bills',
    loadComponent: () => import('./expense-bills/expense-bills.component').then(m => m.ExpenseBillsComponent)
  },
  {
    path: '',
    redirectTo: 'payment-vouchers',
    pathMatch: 'full'
  }
];
