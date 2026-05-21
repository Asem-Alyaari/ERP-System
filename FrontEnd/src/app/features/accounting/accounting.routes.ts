import { Routes } from '@angular/router';

export const ACCOUNTING_ROUTES: Routes = [
  {
    path: 'currencies',
    loadComponent: () => import('./currencies/currencies.component').then(m => m.CurrenciesComponent)
  },
  {
    path: 'exchange-rates',
    loadComponent: () => import('./currency-exchange-rates/currency-exchange-rates.component').then(m => m.CurrencyExchangeRatesComponent)
  },
  {
    path: '',
    redirectTo: 'currencies',
    pathMatch: 'full'
  }
];
