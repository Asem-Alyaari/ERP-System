using Microsoft.AspNetCore.Identity;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ERP.Infrastructure.Persistence;

public static class DbInitializer
{
    // ── Hardcoded GUID Constants for Stable References ────────────────────────
    
    // Units
    private static readonly Guid UnitPcsId = Guid.Parse("11111111-1111-1111-1111-111111111101");
    private static readonly Guid UnitBoxId = Guid.Parse("11111111-1111-1111-1111-111111111102");
    private static readonly Guid UnitPkgId = Guid.Parse("11111111-1111-1111-1111-111111111103");
    
    // Categories
    private static readonly Guid CategoryGeneralItemsId = Guid.Parse("22222222-2222-2222-2222-222222222201");
    private static readonly Guid CategoryFastMovingId = Guid.Parse("22222222-2222-2222-2222-222222222202");
    
    // Cost Centers
    private static readonly Guid CostCenterGeneralAdminId = Guid.Parse("33333333-3333-3333-3333-333333333301");
    private static readonly Guid CostCenterBranchesParentId = Guid.Parse("33333333-3333-3333-3333-333333333302");
    private static readonly Guid CostCenterSanaaBranchId = Guid.Parse("33333333-3333-3333-3333-333333333303");
    private static readonly Guid CostCenterAdenBranchId = Guid.Parse("33333333-3333-3333-3333-333333333304");
    
    // Stock Groups
    private static readonly Guid StockGroupMainInventoryId = Guid.Parse("44444444-4444-4444-4444-444444444401");
    
    // Fiscal Period
    private static readonly Guid FiscalPeriod2026Id = Guid.Parse("55555555-5555-5555-5555-555555555501");
    
    // Journal Entries
    private static readonly Guid JournalEntryPostedId = Guid.Parse("66666666-6666-6666-6666-666666666601");
    private static readonly Guid JournalEntryDraftId = Guid.Parse("66666666-6666-6666-6666-666666666602");
    
    // Journal Entry Lines
    private static readonly Guid JournalEntryLine1Id = Guid.Parse("77777777-7777-7777-7777-777777777701");
    private static readonly Guid JournalEntryLine2Id = Guid.Parse("77777777-7777-7777-7777-777777777702");
    private static readonly Guid JournalEntryLine3Id = Guid.Parse("77777777-7777-7777-7777-777777777703");
    private static readonly Guid JournalEntryLine4Id = Guid.Parse("77777777-7777-7777-7777-777777777704");
    
    // Expense Accounts
    private static readonly Guid TelephoneExpenseId = Guid.Parse("88888888-8888-8888-8888-888888888801");
    private static readonly Guid ElectricityExpenseId = Guid.Parse("88888888-8888-8888-8888-888888888802");
    public static async Task SeedAdminUser(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // 1. إنشاء رتبة Admin إذا لم تكن موجودة
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // 2. التحقق من وجود مستخدم Admin
        var adminEmail = "admin@erp.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var user = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, "Admin@123");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }

    public static async Task SeedCurrenciesAndAccounts(ApplicationDbContext context)
    {
        // 1. زرع العملات الافتراضية
        var localCurrency = await context.Currencies.FirstOrDefaultAsync(c => c.IsLocal);
        if (localCurrency == null)
        {
            localCurrency = new Currency(Guid.NewGuid(), "SAR", "ريال سعودي", "ر.س", true);
            context.Currencies.Add(localCurrency);
            
            var usd = new Currency(Guid.NewGuid(), "USD", "دولار أمريكي", "$", false);
            context.Currencies.Add(usd);
            
            await context.SaveChangesAsync();
        }

        var currencyId = localCurrency.Id;

        // 2. زرع دليل الحسابات القياسي إذا لم يكن موجوداً
        if (!await context.Accounts.AnyAsync())
        {
            // المستوى 1: الحسابات الرئيسية (الآباء - ليست تفصيلية)
            var assets = new Account(Guid.NewGuid(), "1", "الأصول", "Assets", AccountType.Asset, false, currencyId);
            var liabilities = new Account(Guid.NewGuid(), "2", "الالتزامات", "Liabilities", AccountType.Liability, false, currencyId);
            var equity = new Account(Guid.NewGuid(), "3", "حقوق الملكية", "Equity", AccountType.Equity, false, currencyId);
            var revenues = new Account(Guid.NewGuid(), "4", "الإيرادات", "Revenues", AccountType.Revenue, false, currencyId);
            var expenses = new Account(Guid.NewGuid(), "5", "المصروفات", "Expenses", AccountType.Expense, false, currencyId);

            context.Accounts.AddRange(assets, liabilities, equity, revenues, expenses);
            await context.SaveChangesAsync();

            // المستوى 2: فروع الحسابات
            var currentAssets = new Account(Guid.NewGuid(), "11", "الأصول المتداولة", "Current Assets", AccountType.Asset, false, currencyId, assets.Id);
            var currentLiabilities = new Account(Guid.NewGuid(), "21", "الالتزامات المتداولة", "Current Liabilities", AccountType.Liability, false, currencyId, liabilities.Id);
            
            context.Accounts.AddRange(currentAssets, currentLiabilities);
            await context.SaveChangesAsync();

            // المستوى 3: المجموعات الحسابية (IsDetail = false — تستقبل حسابات فرعية)
            var cashAndEquivalents = new Account(Guid.NewGuid(), "1101", "النقدية وما يعادلها", "Cash & Cash Equivalents", AccountType.Asset, false, currencyId, currentAssets.Id);
            var tradeReceivables = new Account(Guid.NewGuid(), "1102", "ذمم العملاء", "Accounts Receivable", AccountType.Asset, false, currencyId, currentAssets.Id);
            // حساب المخزون الأب — الجذر الشرعي لحسابات مخازن مجموعات الأصناف المولّدة تلقائياً
            var inventoryParent = new Account(Guid.NewGuid(), "1103", "المخزون", "Inventory", AccountType.Asset, false, currencyId, currentAssets.Id);
            var tradePayables = new Account(Guid.NewGuid(), "2101", "ذمم الموردين", "Accounts Payable", AccountType.Liability, false, currencyId, currentLiabilities.Id);
            // حساب المبيعات الأب — IsDetail=false ليكون مجموعة تنبثق منها حسابات مبيعات الأصناف
            var salesRevGroup = new Account(Guid.NewGuid(), "4101", "إيرادات المبيعات", "Sales Revenues", AccountType.Revenue, false, currencyId, revenues.Id);
            // حساب التكلفة الأب — IsDetail=false ليكون مجموعة تنبثق منها حسابات تكلفة الأصناف
            var cogsGroup = new Account(Guid.NewGuid(), "5101", "تكلفة البضاعة المباعة", "Cost of Goods Sold", AccountType.Expense, false, currencyId, expenses.Id);

            context.Accounts.AddRange(cashAndEquivalents, tradeReceivables, inventoryParent, tradePayables, salesRevGroup, cogsGroup);
            await context.SaveChangesAsync();

            // المستوى 4: الحسابات التحليلية/التفصيلية (IsDetail = true)
            var mainCash     = new Account(Guid.NewGuid(), "110101", "الصندوق الرئيسي",            "Main Cash",                          AccountType.Asset,    true, currencyId, cashAndEquivalents.Id);
            var bankRajhi    = new Account(Guid.NewGuid(), "110102", "بنك الراجحي جاري",           "Al Rajhi Bank Current",              AccountType.Asset,    true, currencyId, cashAndEquivalents.Id);
            var generalCustomer = new Account(Guid.NewGuid(), "110201", "العملاء العامون",          "General Customers",                  AccountType.Asset,    true, currencyId, tradeReceivables.Id);
            var generalSupplier = new Account(Guid.NewGuid(), "210101", "مورد عام",                 "General Supplier",                   AccountType.Liability, true, currencyId, tradePayables.Id);
            var capital      = new Account(Guid.NewGuid(), "3101",   "رأس المال",                   "Capital",                            AccountType.Equity,   true, currencyId, equity.Id);
            // الحساب التفصيلي الافتراضي للمخزون العام تحت المجموعة الجذرية 1103
            var generalInventory = new Account(Guid.NewGuid(), "110301", "مخزون عام",               "General Inventory",                  AccountType.Asset,    true, currencyId, inventoryParent.Id);
            // الحسابات التفصيلية الافتراضية للمبيعات والتكلفة تحت مجموعتيهما
            var generalSalesRev  = new Account(Guid.NewGuid(), "410101", "إيرادات المبيعات العامة", "General Sales Revenues",             AccountType.Revenue,  true, currencyId, salesRevGroup.Id);
            var generalCogs      = new Account(Guid.NewGuid(), "510101", "تكلفة البضاعة المباعة العامة", "General Cost of Goods Sold",    AccountType.Expense,  true, currencyId, cogsGroup.Id);
            var adminExp         = new Account(Guid.NewGuid(), "5102",   "المصروفات العمومية والإدارية", "General & Administrative Expenses", AccountType.Expense, true, currencyId, expenses.Id);
            // Expense accounts with CostCenterStatus.Required for UI validation testing
            var telephoneExpense = new Account(TelephoneExpenseId, "51010101", "مصروف الهاتف", "Telephone Expense", AccountType.Expense, true, currencyId, adminExp.Id, CostCenterStatus.Required);
            var electricityExpense = new Account(ElectricityExpenseId, "51010102", "مصروف الكهرباء", "Electricity Expense", AccountType.Expense, true, currencyId, adminExp.Id, CostCenterStatus.Required);

            context.Accounts.AddRange(
                mainCash, bankRajhi, generalCustomer, generalSupplier, capital,
                generalInventory, generalSalesRev, generalCogs, adminExp, telephoneExpense, electricityExpense);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// يُصحِّح شجرة الحسابات في قاعدة البيانات القائمة لدعم سيناريو
    /// التوليد التلقائي لحسابات مجموعات الأصناف.
    /// الميثود آمنة للتكرار (Idempotent) وتُشغَّل عند كل Startup.
    /// </summary>
    public static async Task PatchChartOfAccountsForStockGroups(ApplicationDbContext context)
    {
        // ── 1. تحويل "4101" و"5101" إلى حسابات تجميعية (IsDetail = false) ─────────
        // يجب أن يتم أولاً قبل إدراج الأبناء لتجنب FK constraint violations
        await context.Database.ExecuteSqlRawAsync(@"
            UPDATE Accounts
            SET    IsDetail = 0
            WHERE  AccountCode IN ('4101', '5101')
              AND  IsDetail   = 1;
        ");

        // ── 2. إدراج حساب ""1103 - المخزون"" إن غاب ────────────────────────────────
        await context.Database.ExecuteSqlRawAsync(@"
            IF NOT EXISTS (SELECT 1 FROM Accounts WHERE AccountCode = '1103')
            BEGIN
                DECLARE @CurrentAssetsId UNIQUEIDENTIFIER = (
                    SELECT Id FROM Accounts WHERE AccountCode = '11'
                );
                DECLARE @CurrencyId UNIQUEIDENTIFIER = (
                    SELECT TOP 1 Id FROM Currencies WHERE IsLocal = 1
                );
                IF @CurrentAssetsId IS NOT NULL AND @CurrencyId IS NOT NULL
                BEGIN
                    INSERT INTO Accounts (Id, AccountCode, AccountNameAr, AccountNameEn,
                                         AccountType, IsDetail, CurrencyId, ParentAccountId)
                    VALUES (NEWID(), '1103', N'المخزون', 'Inventory',
                            1, 0, @CurrencyId, @CurrentAssetsId);
                END
            END
        ");

        // ── 3. إدراج الحسابات التفصيلية الافتراضية إن غابت ─────────────────────────

        // 110301 - مخزون عام (تحت 1103)
        await context.Database.ExecuteSqlRawAsync(@"
            IF NOT EXISTS (SELECT 1 FROM Accounts WHERE AccountCode = '110301')
            BEGIN
                DECLARE @InvParentId UNIQUEIDENTIFIER = (
                    SELECT Id FROM Accounts WHERE AccountCode = '1103'
                );
                DECLARE @Ccy1 UNIQUEIDENTIFIER = (
                    SELECT TOP 1 Id FROM Currencies WHERE IsLocal = 1
                );
                IF @InvParentId IS NOT NULL AND @Ccy1 IS NOT NULL
                    INSERT INTO Accounts (Id, AccountCode, AccountNameAr, AccountNameEn,
                                         AccountType, IsDetail, CurrencyId, ParentAccountId)
                    VALUES (NEWID(), '110301', N'مخزون عام', 'General Inventory',
                            1, 1, @Ccy1, @InvParentId);
            END
        ");

        // 410101 - إيرادات المبيعات العامة (تحت 4101)
        await context.Database.ExecuteSqlRawAsync(@"
            IF NOT EXISTS (SELECT 1 FROM Accounts WHERE AccountCode = '410101')
            BEGIN
                DECLARE @SalesParentId UNIQUEIDENTIFIER = (
                    SELECT Id FROM Accounts WHERE AccountCode = '4101'
                );
                DECLARE @Ccy2 UNIQUEIDENTIFIER = (
                    SELECT TOP 1 Id FROM Currencies WHERE IsLocal = 1
                );
                IF @SalesParentId IS NOT NULL AND @Ccy2 IS NOT NULL
                    INSERT INTO Accounts (Id, AccountCode, AccountNameAr, AccountNameEn,
                                         AccountType, IsDetail, CurrencyId, ParentAccountId)
                    VALUES (NEWID(), '410101', N'إيرادات المبيعات العامة', 'General Sales Revenues',
                            4, 1, @Ccy2, @SalesParentId);
            END
        ");

        // 510101 - تكلفة البضاعة المباعة العامة (تحت 5101)
        await context.Database.ExecuteSqlRawAsync(@"
            IF NOT EXISTS (SELECT 1 FROM Accounts WHERE AccountCode = '510101')
            BEGIN
                DECLARE @CogsParentId UNIQUEIDENTIFIER = (
                    SELECT Id FROM Accounts WHERE AccountCode = '5101'
                );
                DECLARE @Ccy3 UNIQUEIDENTIFIER = (
                    SELECT TOP 1 Id FROM Currencies WHERE IsLocal = 1
                );
                IF @CogsParentId IS NOT NULL AND @Ccy3 IS NOT NULL
                    INSERT INTO Accounts (Id, AccountCode, AccountNameAr, AccountNameEn,
                                         AccountType, IsDetail, CurrencyId, ParentAccountId)
                    VALUES (NEWID(), '510101', N'تكلفة البضاعة المباعة العامة', 'General Cost of Goods Sold',
                            5, 1, @Ccy3, @CogsParentId);
            END
        ");
    }

    /// <summary>
    /// زرع الوحدات الأساسية (حبة، كرتون، طرد)
    /// </summary>
    public static async Task SeedUnits(ApplicationDbContext context)
    {
        if (!await context.Units.AnyAsync())
        {
            var units = new[]
            {
                new Unit(UnitPcsId, "حبة", "Piece", "PCS"),
                new Unit(UnitBoxId, "كرتون", "Box", "BOX"),
                new Unit(UnitPkgId, "طرد", "Package", "PKG")
            };

            context.Units.AddRange(units);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// زرع التصنيفات الأساسية (أصناف عامة، أصناف سريعة الحركة)
    /// </summary>
    public static async Task SeedCategories(ApplicationDbContext context)
    {
        if (!await context.Categories.AnyAsync())
        {
            var categories = new[]
            {
                new Category(CategoryGeneralItemsId, "أصناف عامة", "General Items"),
                new Category(CategoryFastMovingId, "أصناف سريعة الحركة", "Fast Moving Items")
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// زرع شجرة مراكز التكلفة (الإدارة العامة، الفروع، فرع صنعاء، فرع عدن)
    /// </summary>
    public static async Task SeedCostCenters(ApplicationDbContext context)
    {
        if (!await context.CostCenters.AnyAsync())
        {
            // المستوى 1: الآباء (ليست تفصيلية)
            var generalAdmin = new CostCenter(
                CostCenterGeneralAdminId,
                "1000",
                "الإدارة العامة",
                "General Administration",
                isDetail: false);

            var branchesParent = new CostCenter(
                CostCenterBranchesParentId,
                "2000",
                "الفروع",
                "Branches",
                isDetail: false);

            context.CostCenters.AddRange(generalAdmin, branchesParent);
            await context.SaveChangesAsync();

            // المستوى 2: الفروع التفصيلية (تحت الفروع)
            var sanaaBranch = new CostCenter(
                CostCenterSanaaBranchId,
                "2001",
                "فرع صنعاء",
                "Sanaa Branch",
                isDetail: true,
                CostCenterBranchesParentId);

            var adenBranch = new CostCenter(
                CostCenterAdenBranchId,
                "2002",
                "فرع عدن",
                "Aden Branch",
                isDetail: true,
                CostCenterBranchesParentId);

            context.CostCenters.AddRange(sanaaBranch, adenBranch);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// زرع مجموعة المخازن الرئيسية مع ربطها بالحسابات المحاسبية الافتراضية
    /// </summary>
    public static async Task SeedStockGroups(ApplicationDbContext context)
    {
        if (!await context.StockGroups.AnyAsync())
        {
            // جلب الحسابات الجذرية للربط المحاسبي
            var inventoryAccount = await context.Accounts
                .FirstOrDefaultAsync(a => a.AccountCode == "1103"); // المخزون
            var salesAccount = await context.Accounts
                .FirstOrDefaultAsync(a => a.AccountCode == "4101"); // إيرادات المبيعات
            var cogsAccount = await context.Accounts
                .FirstOrDefaultAsync(a => a.AccountCode == "5101"); // تكلفة البضاعة المباعة

            var mainInventoryGroup = new StockGroup(
                StockGroupMainInventoryId,
                "SG001",
                "مجموعة المخازن الرئيسية",
                "Main Inventory Group",
                isDetail: false,
                parentGroupId: null,
                inventoryAccountId: inventoryAccount?.Id,
                salesAccountId: salesAccount?.Id,
                cogsAccountId: cogsAccount?.Id);

            context.StockGroups.Add(mainInventoryGroup);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// زرع الفترة المالية الافتراضية
    /// </summary>
    public static async Task SeedFiscalPeriod(ApplicationDbContext context)
    {
        if (!await context.FiscalPeriods.AnyAsync())
        {
            var fiscalPeriod = new FiscalPeriod(
                FiscalPeriod2026Id,
                "2026",
                new DateTime(2026, 1, 1),
                new DateTime(2026, 12, 31));

            context.FiscalPeriods.Add(fiscalPeriod);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// تحديث حالة مركز التكلفة للحسابات المصروفية لتكون Required
    /// هذه الميثود آمنة للتكرار وتُشغَّل بعد SeedCurrenciesAndAccounts
    /// </summary>
    public static async Task PatchExpenseAccountsCostCenterStatus(ApplicationDbContext context)
    {
        // تحديث جميع الحسابات التي تبدأ بـ 5101 (مصروفات) لتكون CostCenterStatus.Required
        await context.Database.ExecuteSqlRawAsync(@"
            UPDATE Accounts
            SET CostCenterStatus = 1
            WHERE  AccountCode LIKE '5101%'
              AND  IsDetail = 1
              AND  CostCenterStatus <> 1;
        ");
    }

    /// <summary>
    /// زرع قيود يومية تجريبية متزنة (Debit = Credit)
    /// </summary>
    public static async Task SeedSampleJournalEntries(ApplicationDbContext context)
    {
        // التحقق من عدم وجود القيود مسبقاً (idempotent safeguard)
        if (await context.JournalEntryMasters.AnyAsync(je => je.Id == JournalEntryPostedId || je.Id == JournalEntryDraftId))
        {
            return;
        }

        // جلب الحسابات والعملة اللازمة
        var mainCashAccount = await context.Accounts.FirstOrDefaultAsync(a => a.AccountCode == "110101");
        var electricityExpenseAccount = await context.Accounts.FirstOrDefaultAsync(a => a.AccountCode == "51010102");
        var telephoneExpenseAccount = await context.Accounts.FirstOrDefaultAsync(a => a.AccountCode == "51010101");
        var localCurrency = await context.Currencies.FirstOrDefaultAsync(c => c.IsLocal);
        var fiscalPeriod = await context.FiscalPeriods.FirstOrDefaultAsync();

        if (mainCashAccount == null || electricityExpenseAccount == null || telephoneExpenseAccount == null || 
            localCurrency == null || fiscalPeriod == null)
        {
            return; // Skip if required data is missing
        }

        // ── القيد الأول: مرحل (Posted) - دفع فاتورة كهرباء لفرع صنعاء ───────────────
        var postedEntry = new JournalEntryMaster(
            JournalEntryPostedId,
            "JV-2026-001",
            new DateTime(2026, 1, 15),
            "دفع فاتورة كهرباء - فرع صنعاء",
            fiscalPeriod.Id,
            "System");

        postedEntry.Post("System"); // Set status to Posted immediately

        var postedLine1 = new JournalEntryLine(
            JournalEntryLine1Id,
            JournalEntryPostedId,
            electricityExpenseAccount.Id,
            debit: 5000m,
            credit: 0m,
            localCurrency.Id,
            exchangeRate: 1m,
            costCenterId: CostCenterSanaaBranchId, // Assigned to Sanaa Branch (Code 2001)
            memo: "فاتورة كهرباء يناير 2026");

        var postedLine2 = new JournalEntryLine(
            JournalEntryLine2Id,
            JournalEntryPostedId,
            mainCashAccount.Id,
            debit: 0m,
            credit: 5000m,
            localCurrency.Id,
            exchangeRate: 1m,
            memo: "دفع نقداً");

        context.JournalEntryMasters.Add(postedEntry);
        context.JournalEntryLines.AddRange(postedLine1, postedLine2);
        await context.SaveChangesAsync();

        // ── القيد الثاني: مسودة (Draft) - فاتورة هاتف معلقة ───────────────────────────
        var draftEntry = new JournalEntryMaster(
            JournalEntryDraftId,
            "JV-2026-002",
            new DateTime(2026, 1, 20),
            "فاتورة هاتف - قيد مسودة للاختبار",
            fiscalPeriod.Id,
            "System");
        // Status remains Draft (default)

        var draftLine1 = new JournalEntryLine(
            JournalEntryLine3Id,
            JournalEntryDraftId,
            telephoneExpenseAccount.Id,
            debit: 1500m,
            credit: 0m,
            localCurrency.Id,
            exchangeRate: 1m,
            costCenterId: CostCenterSanaaBranchId,
            memo: "فاتورة هاتف فبراير 2026");

        var draftLine2 = new JournalEntryLine(
            JournalEntryLine4Id,
            JournalEntryDraftId,
            mainCashAccount.Id,
            debit: 0m,
            credit: 1500m,
            localCurrency.Id,
            exchangeRate: 1m,
            memo: "دفع نقداً");

        context.JournalEntryMasters.Add(draftEntry);
        context.JournalEntryLines.AddRange(draftLine1, draftLine2);
        await context.SaveChangesAsync();
    }
}
