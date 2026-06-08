import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SHARED_IMPORTS } from '../../../../shared/shared.imports';
import { FinancialReportService, IncomeStatement } from '../../../../core/services/financial-report.service';
import { FiscalPeriodService, FiscalPeriod } from '../../../../core/services/fiscal-period.service';
import { CostCenterService, CostCenterLookup } from '../../../../core/services/cost-center.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-income-statement',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ...SHARED_IMPORTS
  ],
  templateUrl: './income-statement.component.html',
  styleUrls: ['./income-statement.component.scss']
})
export class IncomeStatementComponent implements OnInit {
  fiscalPeriods$: Observable<FiscalPeriod[]> = new Observable();
  costCenters$: Observable<CostCenterLookup[]> = new Observable();
  selectedFiscalPeriodId: string | null = null;
  selectedCostCenterId: string | null = null;
  fromDate: Date | null = null;
  toDate: Date | null = null;

  incomeStatement: IncomeStatement | null = null;
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

    if (this.fromDate) {
      params.fromDate = this.fromDate.toISOString();
    }

    if (this.toDate) {
      params.toDate = this.toDate.toISOString();
    }

    this.financialReportService.getIncomeStatement(params).subscribe(data => {
      this.incomeStatement = data;
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

  getProfitLossClass(): string {
    if (!this.incomeStatement) return '';
    return this.incomeStatement.netProfitLoss >= 0 ? 'text-green-600' : 'text-red-600';
  }

  getProfitLossLabel(): string {
    if (!this.incomeStatement) return '';
    return this.incomeStatement.netProfitLoss >= 0 ? 'صافي الربح' : 'صافي الخسارة';
  }
}
