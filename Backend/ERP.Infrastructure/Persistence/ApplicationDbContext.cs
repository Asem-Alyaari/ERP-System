using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ERP.Domain.Entities;

namespace ERP.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<FiscalPeriod> FiscalPeriods => Set<FiscalPeriod>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<CurrencyExchangeRate> CurrencyExchangeRates => Set<CurrencyExchangeRate>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<CostCenter> CostCenters => Set<CostCenter>();
    public DbSet<JournalEntryMaster> JournalEntryMasters => Set<JournalEntryMaster>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
    public DbSet<AccountBalance> AccountBalances => Set<AccountBalance>();
    public DbSet<StockGroup> StockGroups => Set<StockGroup>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<ItemUnit> ItemUnits => Set<ItemUnit>();
    public DbSet<ItemBatch> ItemBatches => Set<ItemBatch>();
    public DbSet<InventoryTransactionMaster> InventoryTransactionMasters => Set<InventoryTransactionMaster>();
    public DbSet<InventoryTransactionLine> InventoryTransactionLines => Set<InventoryTransactionLine>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<PurchaseInvoiceMaster> PurchaseInvoiceMasters => Set<PurchaseInvoiceMaster>();
    public DbSet<PurchaseInvoiceLine> PurchaseInvoiceLines => Set<PurchaseInvoiceLine>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<SalesInvoiceMaster> SalesInvoiceMasters => Set<SalesInvoiceMaster>();
    public DbSet<SalesInvoiceLine> SalesInvoiceLines => Set<SalesInvoiceLine>();
    public DbSet<ReceiptVoucher> ReceiptVouchers => Set<ReceiptVoucher>();
    public DbSet<PaymentVoucher> PaymentVouchers => Set<PaymentVoucher>();
    public DbSet<ExpenseBillMaster> ExpenseBillMasters => Set<ExpenseBillMaster>();
    public DbSet<ExpenseBillLine> ExpenseBillLines => Set<ExpenseBillLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
