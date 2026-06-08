import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SHARED_IMPORTS } from '../../../../shared/shared.imports';
import { FinancialReportService, BalanceSheet } from '../../../../core/services/financial-report.service';
import { FiscalPeriodService, FiscalPeriod } from '../../../../core/services/fiscal-period.service';
import { CostCenterService, CostCenterLookup } from '../../../../core/services/cost-center.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-balance-sheet',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ...SHARED_IMPORTS
  ],
  templateUrl: './balance-sheet.component.html',
  styleUrls: ['./balance-sheet.component.scss']
})
export class BalanceSheetComponent implements OnInit {
  fiscalPeriods$: Observable<FiscalPeriod[]> = new Observable();
  costCenters$: Observable<CostCenterLookup[]> = new Observable();
  selectedFiscalPeriodId: string | null = null;
  selectedCostCenterId: string | null = null;
  asOfDate: Date | null = null;

  balanceSheet: BalanceSheet | null = null;
  loading = false;

  constructor(
    private financialReportService: FinancialReportService,
    private fiscalPeriodService: FiscalPeriodService,
    private costCenterService: CostCenterService
  ) {}

  ngOnInit(): void {
    this.fiscalPeriods$ = this.fiscalPeriodService.getAll();
    this.costCenters$ = this.costCenterService.getAll();
  }

  generateReport(): void {
    if (!this.selectedFiscalPeriodId) {
      return;
    }

    this.loading = true;

    const params: any = {
      fiscalPeriodId: this.selectedFiscalPeriodId
    };

    if (this.selectedCostCenterId) {
      params.costCenterId = this.selectedCostCenterId;
    }

    if (this.asOfDate) {
      params.asOfDate = this.asOfDate.toISOString();
    }

    this.financialReportService.getBalanceSheet(params).subscribe(data => {
      this.balanceSheet = data;
      this.loading = false;
    });
  }

  printReport(): void {
    window.print();
  }

  formatNumber(value: number): string {
    return new Intl.NumberFormat('ar-SA', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(value);
  }

  getIndent(level: number): string {
    return `${(level - 1) * 20}px`;
  }

  getBalanceStatusClass(): string {
    if (!this.balanceSheet) return '';
    return this.balanceSheet.isBalanced ? 'text-green-600' : 'text-red-600';
  }

  getBalanceStatusLabel(): string {
    if (!this.balanceSheet) return '';
    return this.balanceSheet.isBalanced ? 'متوازن' : 'غير متوازن';
  }
}
