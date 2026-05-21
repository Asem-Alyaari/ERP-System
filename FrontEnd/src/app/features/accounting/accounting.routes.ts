import { Routes } from '@angular/router';

export const ACCOUNTING_ROUTES: Routes = [
  {
    path: 'currencies',
    loadComponent: () => import('./currencies/currencies.component').then(m => m.CurrenciesComponent)
  },
  {
    path: '',
    redirectTo: 'currencies',
    pathMatch: 'full'
  }
];
