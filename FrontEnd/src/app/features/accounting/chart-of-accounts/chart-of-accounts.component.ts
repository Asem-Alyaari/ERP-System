import { Component, OnInit } from '@angular/core';
import { AccountService, AccountDto, AccountType } from '../../../core/services/account.service';
import { CurrencyService, Currency } from '../../../core/services/currency.service';
import { SHARED_IMPORTS } from '../../../shared/shared.imports';
import { MessageService, ConfirmationService } from 'primeng/api';

export interface AccountTreeNode {
  account: AccountDto;
  children: AccountTreeNode[];
  expanded: boolean;
  visible: boolean;
}

@Component({
  selector: 'app-chart-of-accounts',
  standalone: true,
  imports: [...SHARED_IMPORTS],
  providers: [MessageService, ConfirmationService],
  templateUrl: './chart-of-accounts.component.html',
  styleUrl: './chart-of-accounts.component.scss'
})
export class ChartOfAccountsComponent implements OnInit {
  accounts: AccountDto[] = [];
  currencies: Currency[] = [];
  treeNodes: AccountTreeNode[] = [];
  isLoading = false;
  searchTerm = '';

  // Active / Selected account for view details panel
  selectedAccount: AccountDto | null = null;

  // Dialog state
  dialogVisible = false;
  isEditMode = false;
  currentAccount: Partial<AccountDto> = {};
  
  // Types list for selector
  accountTypes = [
    { label: 'أصول (Asset)', value: AccountType.Asset },
    { label: 'خصوم / التزامات (Liability)', value: AccountType.Liability },
    { label: 'حقوق ملكية (Equity)', value: AccountType.Equity },
    { label: 'إيرادات (Revenue)', value: AccountType.Revenue },
    { label: 'مصروفات (Expense)', value: AccountType.Expense }
  ];

  constructor(
    private accountService: AccountService,
    private currencyService: CurrencyService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.isLoading = true;
    // Load currencies first, then accounts
    this.currencyService.getList().subscribe({
      next: (currencies) => {
        this.currencies = currencies;
        this.loadAccounts();
      },
      error: () => {
        this.isLoading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل تحميل قائمة العملات'
        });
      }
    });
  }

  loadAccounts() {
    this.accountService.getAll().subscribe({
      next: (res) => {
        this.accounts = res || [];
        this.buildTree();
        this.isLoading = false;
        
        // Restore selected account reference if it exists
        if (this.selectedAccount) {
          const updated = this.accounts.find(a => a.id === this.selectedAccount?.id);
          this.selectedAccount = updated || null;
        }
      },
      error: () => {
        this.isLoading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل تحميل دليل الحسابات'
        });
      }
    });
  }

  buildTree() {
    const nodeMap = new Map<string, AccountTreeNode>();
    const roots: AccountTreeNode[] = [];

    // Create a node for each account
    this.accounts.forEach(acc => {
      nodeMap.set(acc.id, {
        account: acc,
        children: [],
        expanded: false,
        visible: true
      });
    });

    // Populate children lists and roots
    this.accounts.forEach(acc => {
      const node = nodeMap.get(acc.id)!;
      if (acc.parentAccountId && nodeMap.has(acc.parentAccountId)) {
        nodeMap.get(acc.parentAccountId)!.children.push(node);
      } else {
        roots.push(node);
      }
    });

    // Sort by AccountCode
    nodeMap.forEach(node => {
      node.children.sort((a, b) => a.account.accountCode.localeCompare(b.account.accountCode));
    });
    roots.sort((a, b) => a.account.accountCode.localeCompare(b.account.accountCode));

    this.treeNodes = roots;
    
    // Apply search filter if active
    if (this.searchTerm) {
      this.filterTree();
    }
  }

  toggleNode(node: AccountTreeNode, event: MouseEvent) {
    event.stopPropagation();
    node.expanded = !node.expanded;
  }

  selectAccount(account: AccountDto) {
    this.selectedAccount = account;
  }

  onSearchChange() {
    this.filterTree();
  }

  filterTree() {
    if (!this.searchTerm.trim()) {
      this.resetVisibility(this.treeNodes);
      return;
    }
    this.applyFilter(this.treeNodes, this.searchTerm.trim().toLowerCase());
  }

  resetVisibility(nodes: AccountTreeNode[]) {
    nodes.forEach(node => {
      node.visible = true;
      this.resetVisibility(node.children);
    });
  }

  applyFilter(nodes: AccountTreeNode[], term: string): boolean {
    let anyVisible = false;

    nodes.forEach(node => {
      const matches = 
        node.account.accountCode.toLowerCase().includes(term) ||
        node.account.accountNameAr.toLowerCase().includes(term) ||
        node.account.accountNameEn.toLowerCase().includes(term);

      const childrenVisible = this.applyFilter(node.children, term);
      
      node.visible = matches || childrenVisible;
      
      // Auto expand matching parents
      if (childrenVisible) {
        node.expanded = true;
      }
      
      if (node.visible) {
        anyVisible = true;
      }
    });

    return anyVisible;
  }

  expandAll() {
    this.toggleAllNodes(this.treeNodes, true);
  }

  collapseAll() {
    this.toggleAllNodes(this.treeNodes, false);
  }

  toggleAllNodes(nodes: AccountTreeNode[], expand: boolean) {
    nodes.forEach(node => {
      node.expanded = expand;
      this.toggleAllNodes(node.children, expand);
    });
  }

  // Get type name in Arabic
  getTypeLabel(type: AccountType): string {
    switch (type) {
      case AccountType.Asset: return 'أصول';
      case AccountType.Liability: return 'خصوم / التزامات';
      case AccountType.Equity: return 'حقوق ملكية';
      case AccountType.Revenue: return 'إيرادات';
      case AccountType.Expense: return 'مصروفات';
      default: return 'غير معروف';
    }
  }

  get totalAccountsCount(): number {
    return this.accounts.length;
  }

  get detailAccountsCount(): number {
    return this.accounts.filter(a => a.isDetail).length;
  }

  getTypeBadgeClass(type: AccountType): 'info' | 'warn' | 'contrast' | 'success' | 'danger' | 'secondary' {
    switch (type) {
      case AccountType.Asset: return 'info';
      case AccountType.Liability: return 'warn';
      case AccountType.Equity: return 'contrast';
      case AccountType.Revenue: return 'success';
      case AccountType.Expense: return 'danger';
      default: return 'secondary';
    }
  }

  // Get parent account display name
  getParentName(parentId?: string): string {
    if (!parentId) return '—';
    const parent = this.accounts.find(a => a.id === parentId);
    return parent ? `${parent.accountCode} - ${parent.accountNameAr}` : '—';
  }

  openAddRootDialog() {
    this.isEditMode = false;
    
    // Default to local currency
    const localCurrency = this.currencies.find(c => c.isLocal) || this.currencies[0];
    
    this.currentAccount = {
      accountCode: '',
      accountNameAr: '',
      accountNameEn: '',
      accountType: AccountType.Asset,
      isDetail: true,
      currencyId: localCurrency?.id || '',
      parentAccountId: undefined
    };
    this.dialogVisible = true;
  }

  openAddSubDialog(parentAccount: AccountDto) {
    if (parentAccount.isDetail) {
      this.messageService.add({
        severity: 'warn',
        summary: 'تنبيه',
        detail: 'لا يمكن إضافة حسابات فرعية تحت حساب تحليلي تفصيلي.'
      });
      return;
    }

    this.isEditMode = false;
    const suggestedCode = this.suggestNextCode(parentAccount);
    
    this.currentAccount = {
      accountCode: suggestedCode,
      accountNameAr: '',
      accountNameEn: '',
      accountType: parentAccount.accountType,
      isDetail: true,
      currencyId: parentAccount.currencyId, // inherit currency
      parentAccountId: parentAccount.id
    };
    
    this.dialogVisible = true;
  }

  suggestNextCode(parent: AccountDto): string {
    // Find all immediate children of this parent
    const children = this.accounts.filter(a => a.parentAccountId === parent.id);
    
    if (children.length === 0) {
      // Suggest parent code + "01" (for double digits structure)
      return parent.accountCode + '01';
    }

    // Get all codes, filter numeric ones
    const codes = children
      .map(c => c.accountCode)
      .filter(code => code.startsWith(parent.accountCode));

    if (codes.length === 0) {
      return parent.accountCode + '01';
    }

    // Sort codes alphabetically/numerically and get the last one
    codes.sort();
    const lastCode = codes[codes.length - 1];

    // Find the suffix parts
    const parentLength = parent.accountCode.length;
    const suffix = lastCode.substring(parentLength);
    const num = parseInt(suffix, 10);

    if (isNaN(num)) {
      return parent.accountCode + '01';
    }

    const nextNum = num + 1;
    // Pad the next number to match the length of the suffix (usually 2 digits)
    const paddedSuffix = nextNum.toString().padStart(suffix.length, '0');
    return parent.accountCode + paddedSuffix;
  }

  openEditDialog(account: AccountDto) {
    this.isEditMode = true;
    this.currentAccount = { ...account };
    this.dialogVisible = true;
  }

  closeDialog() {
    this.dialogVisible = false;
  }

  saveAccount() {
    if (!this.currentAccount.accountCode || !this.currentAccount.accountNameAr || !this.currentAccount.accountNameEn || !this.currentAccount.currencyId) {
      this.messageService.add({
        severity: 'warn',
        summary: 'تنبيه',
        detail: 'يرجى تعبئة جميع الحقول الإلزامية'
      });
      return;
    }

    if (this.isEditMode && this.currentAccount.id) {
      this.accountService.update(this.currentAccount.id, this.currentAccount).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'تم',
            detail: 'تم تعديل الحساب بنجاح'
          });
          this.loadAccounts();
          this.closeDialog();
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: err.error?.message || 'فشل تعديل الحساب.'
          });
        }
      });
    } else {
      this.accountService.create(this.currentAccount).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'تم',
            detail: 'تم إضافة الحساب بنجاح'
          });
          this.loadAccounts();
          this.closeDialog();
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: err.error?.message || 'فشل إضافة الحساب.'
          });
        }
      });
    }
  }

  confirmDelete(account: AccountDto) {
    this.confirmationService.confirm({
      message: `هل أنت متأكد من حذف الحساب "${account.accountCode} - ${account.accountNameAr}"؟`,
      header: 'تأكيد الحذف',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'حذف الحساب',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.accountService.delete(account.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'تم',
              detail: 'تم حذف الحساب بنجاح'
            });
            if (this.selectedAccount?.id === account.id) {
              this.selectedAccount = null;
            }
            this.loadAccounts();
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: 'خطأ',
              detail: err.error?.message || 'فشل حذف الحساب. قد يكون الحساب مرتبطاً بحركات مالية أو يحتوي على حسابات فرعية.'
            });
          }
        });
      }
    });
  }
}
