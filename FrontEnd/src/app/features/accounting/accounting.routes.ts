import { Routes } from '@angular/router';

export const ACCOUNTING_ROUTES: Routes = [
  {
    path: 'chart-of-accounts',
    loadComponent: () => import('./chart-of-accounts/chart-of-accounts.component').then(m => m.ChartOfAccountsComponent)
  },
  {
    path: 'journal-entries',
    loadComponent: () => import('./journal-entries/journal-entries.component').then(m => m.JournalEntriesComponent)
  },
  {
    path: 'ledger-report',
    loadComponent: () => import('./ledger-report/ledger-report.component').then(m => m.LedgerReportComponent)
  },
  {
    path: 'cost-centers',
    loadComponent: () => import('./cost-centers/cost-centers.component').then(m => m.CostCentersComponent)
  },
  {
    path: 'fiscal-periods',
    loadComponent: () => import('./fiscal-periods/fiscal-periods.component').then(m => m.FiscalPeriodsComponent)
  },
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
    redirectTo: 'chart-of-accounts',
    pathMatch: 'full'
  }
];
