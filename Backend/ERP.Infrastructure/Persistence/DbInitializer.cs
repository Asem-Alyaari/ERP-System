using Microsoft.AspNetCore.Identity;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ERP.Infrastructure.Persistence;

public static class DbInitializer
{
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

            context.Accounts.AddRange(
                mainCash, bankRajhi, generalCustomer, generalSupplier, capital,
                generalInventory, generalSalesRev, generalCogs, adminExp);
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
}
