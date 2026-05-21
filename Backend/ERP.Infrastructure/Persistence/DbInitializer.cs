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

            // المستوى 3: المجموعات الحسابية
            var cashAndEquivalents = new Account(Guid.NewGuid(), "1101", "النقدية وما يعادلها", "Cash & Cash Equivalents", AccountType.Asset, false, currencyId, currentAssets.Id);
            var tradeReceivables = new Account(Guid.NewGuid(), "1102", "ذمم العملاء", "Accounts Receivable", AccountType.Asset, false, currencyId, currentAssets.Id);
            var tradePayables = new Account(Guid.NewGuid(), "2101", "ذمم الموردين", "Accounts Payable", AccountType.Liability, false, currencyId, currentLiabilities.Id);

            context.Accounts.AddRange(cashAndEquivalents, tradeReceivables, tradePayables);
            await context.SaveChangesAsync();

            // المستوى 4: الحسابات التحليلية/التفصيلية (IsDetail = true)
            var mainCash = new Account(Guid.NewGuid(), "110101", "الصندوق الرئيسي", "Main Cash", AccountType.Asset, true, currencyId, cashAndEquivalents.Id);
            var bankRajhi = new Account(Guid.NewGuid(), "110102", "بنك الراجحي جاري", "Al Rajhi Bank Current", AccountType.Asset, true, currencyId, cashAndEquivalents.Id);
            
            var generalCustomer = new Account(Guid.NewGuid(), "110201", "العملاء العامون", "General Customers", AccountType.Asset, true, currencyId, tradeReceivables.Id);
            var generalSupplier = new Account(Guid.NewGuid(), "210101", "مورد عام", "General Supplier", AccountType.Liability, true, currencyId, tradePayables.Id);

            var capital = new Account(Guid.NewGuid(), "3101", "رأس المال", "Capital", AccountType.Equity, true, currencyId, equity.Id);
            
            var salesRev = new Account(Guid.NewGuid(), "4101", "إيرادات المبيعات", "Sales Revenues", AccountType.Revenue, true, currencyId, revenues.Id);
            
            var cogs = new Account(Guid.NewGuid(), "5101", "تكلفة البضاعة المباعة", "Cost of Goods Sold", AccountType.Expense, true, currencyId, expenses.Id);
            var adminExp = new Account(Guid.NewGuid(), "5102", "المصروفات العمومية والإدارية", "General & Administrative Expenses", AccountType.Expense, true, currencyId, expenses.Id);

            context.Accounts.AddRange(mainCash, bankRajhi, generalCustomer, generalSupplier, capital, salesRev, cogs, adminExp);
            await context.SaveChangesAsync();
        }
    }
}
