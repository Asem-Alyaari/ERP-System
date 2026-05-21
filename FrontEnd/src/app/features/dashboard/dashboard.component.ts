import { Component } from '@angular/core';
import { SHARED_IMPORTS } from '../../shared/shared.imports';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [...SHARED_IMPORTS],
  template: `
    <div class="animate-fade-in">

      <!-- Page Header -->
      <div class="mb-6">
        <h2 class="page-title">لوحة التحكم</h2>
        <p class="page-subtitle">نظرة عامة على أداء النظام</p>
      </div>

      <!-- KPI Cards -->
      <div class="kpi-grid">

        <div class="kpi-card kpi-card--blue animate-slide-up">
          <div class="kpi-card__icon">
            <i class="pi pi-shopping-cart"></i>
          </div>
          <div class="kpi-card__body">
            <span class="kpi-card__label">إجمالي المبيعات</span>
            <span class="kpi-card__value">124,500 ر.س</span>
            <span class="kpi-card__trend kpi-card__trend--up">
              <i class="pi pi-arrow-up"></i> 12% هذا الشهر
            </span>
          </div>
        </div>

        <div class="kpi-card kpi-card--cyan animate-slide-up" style="animation-delay:0.05s">
          <div class="kpi-card__icon">
            <i class="pi pi-users"></i>
          </div>
          <div class="kpi-card__body">
            <span class="kpi-card__label">العملاء النشطين</span>
            <span class="kpi-card__value">1,240</span>
            <span class="kpi-card__trend kpi-card__trend--up">
              <i class="pi pi-arrow-up"></i> 5% هذا الشهر
            </span>
          </div>
        </div>

        <div class="kpi-card kpi-card--amber animate-slide-up" style="animation-delay:0.1s">
          <div class="kpi-card__icon">
            <i class="pi pi-file"></i>
          </div>
          <div class="kpi-card__body">
            <span class="kpi-card__label">الفواتير المعلقة</span>
            <span class="kpi-card__value">14</span>
            <span class="kpi-card__trend kpi-card__trend--warn">
              <i class="pi pi-exclamation-triangle"></i> يتطلب إجراء
            </span>
          </div>
        </div>

        <div class="kpi-card kpi-card--green animate-slide-up" style="animation-delay:0.15s">
          <div class="kpi-card__icon">
            <i class="pi pi-box"></i>
          </div>
          <div class="kpi-card__body">
            <span class="kpi-card__label">أصناف المخزون</span>
            <span class="kpi-card__value">3,812</span>
            <span class="kpi-card__trend kpi-card__trend--up">
              <i class="pi pi-arrow-up"></i> 8% هذا الشهر
            </span>
          </div>
        </div>

      </div>

      <!-- Quick Links -->
      <div class="quick-links mt-6">
        <h3 class="quick-links__title">وصول سريع</h3>
        <div class="quick-links__grid">
          <a routerLink="/inventory/categories" class="quick-link-card">
            <i class="pi pi-tags"></i>
            <span>التصنيفات</span>
          </a>
          <a routerLink="/inventory/units" class="quick-link-card">
            <i class="pi pi-percentage"></i>
            <span>وحدات القياس</span>
          </a>
          <a routerLink="/sales" class="quick-link-card">
            <i class="pi pi-shopping-cart"></i>
            <span>المبيعات</span>
          </a>
          <a routerLink="/purchases" class="quick-link-card">
            <i class="pi pi-shopping-bag"></i>
            <span>المشتريات</span>
          </a>
          <a routerLink="/accounting" class="quick-link-card">
            <i class="pi pi-wallet"></i>
            <span>الحسابات</span>
          </a>
          <a routerLink="/reports" class="quick-link-card">
            <i class="pi pi-chart-bar"></i>
            <span>التقارير</span>
          </a>
        </div>
      </div>

    </div>
  `,
  styles: [`
    .kpi-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(230px, 1fr));
      gap: 1.25rem;
    }

    .kpi-card {
      display: flex;
      align-items: center;
      gap: 1rem;
      padding: 1.25rem 1.5rem;
      background: #fff;
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg);
      box-shadow: var(--shadow-sm);
      transition: box-shadow 0.2s, transform 0.2s;
    }
    .kpi-card:hover {
      box-shadow: var(--shadow-md);
      transform: translateY(-2px);
    }

    .kpi-card__icon {
      width: 52px; height: 52px;
      border-radius: var(--radius-md);
      display: flex; align-items: center; justify-content: center;
      flex-shrink: 0;
      font-size: 1.4rem;
    }
    .kpi-card__body {
      display: flex;
      flex-direction: column;
      gap: 0.2rem;
    }
    .kpi-card__label {
      font-size: 0.8rem;
      color: var(--text-muted);
      font-weight: 500;
    }
    .kpi-card__value {
      font-size: 1.5rem;
      font-weight: 800;
      color: var(--text-primary);
      line-height: 1.2;
    }
    .kpi-card__trend {
      font-size: 0.75rem;
      font-weight: 600;
      display: flex;
      align-items: center;
      gap: 3px;
    }
    .kpi-card__trend i { font-size: 0.7rem; }
    .kpi-card__trend--up   { color: var(--color-success); }
    .kpi-card__trend--warn { color: var(--color-warning); }
    .kpi-card__trend--down { color: var(--color-danger); }

    /* Color variants */
    .kpi-card--blue  .kpi-card__icon { background: #dbeafe; color: var(--color-primary); }
    .kpi-card--cyan  .kpi-card__icon { background: #e0f2fe; color: var(--color-accent); }
    .kpi-card--amber .kpi-card__icon { background: #fef3c7; color: var(--color-warning); }
    .kpi-card--green .kpi-card__icon { background: #d1fae5; color: var(--color-success); }

    /* Quick links */
    .quick-links {
      background: #fff;
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg);
      padding: 1.25rem 1.5rem;
      box-shadow: var(--shadow-sm);
    }
    .quick-links__title {
      font-size: 0.95rem;
      font-weight: 700;
      color: var(--text-secondary);
      margin-bottom: 1rem;
    }
    .quick-links__grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(130px, 1fr));
      gap: 0.75rem;
    }

    .quick-link-card {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 0.5rem;
      padding: 1rem 0.75rem;
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      background: var(--bg-surface-2);
      color: var(--text-secondary);
      text-decoration: none;
      font-size: 0.8rem;
      font-weight: 600;
      transition: all 0.2s;
    }
    .quick-link-card i {
      font-size: 1.4rem;
      color: var(--color-primary);
    }
    .quick-link-card:hover {
      background: var(--color-primary-alpha);
      border-color: var(--color-primary-light);
      color: var(--color-primary-dark);
      transform: translateY(-2px);
      box-shadow: var(--shadow-sm);
    }
  `]
})
export class DashboardComponent {}

