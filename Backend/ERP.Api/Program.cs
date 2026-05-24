using ERP.Application;
using ERP.Infrastructure;
using Microsoft.AspNetCore.Identity;
using ERP.Domain.Entities;
using ERP.Infrastructure.Persistence;
using ERP.Api.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/erp-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Register Clean Architecture Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // needed if you use cookies or specific auth tokens
    });
});

var app = builder.Build();

// Global Exception Handling
app.UseMiddleware<ExceptionMiddleware>();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await DbInitializer.SeedAdminUser(userManager, roleManager);

        var context = services.GetRequiredService<ApplicationDbContext>();
        await DbInitializer.SeedCurrenciesAndAccounts(context);

        // تصحيح شجرة الحسابات الحالية لدعم مجموعات الأصناف (Idempotent - آمن للتكرار)
        await DbInitializer.PatchChartOfAccountsForStockGroups(context);

        // تحديث حالة مركز التكلفة للحسابات المصروفية لتكون Required (Idempotent - آمن للتكرار)
        await DbInitializer.PatchExpenseAccountsCostCenterStatus(context);

        // زرع بيانات Lookup الأساسية
        await DbInitializer.SeedUnits(context);
        await DbInitializer.SeedCategories(context);
        await DbInitializer.SeedCostCenters(context);
        await DbInitializer.SeedStockGroups(context);
        
        // زرع الفترة المالية
        await DbInitializer.SeedFiscalPeriod(context);
        
        // زرع قيود يومية تجريبية
        await DbInitializer.SeedSampleJournalEntries(context);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred during seeding the database");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
