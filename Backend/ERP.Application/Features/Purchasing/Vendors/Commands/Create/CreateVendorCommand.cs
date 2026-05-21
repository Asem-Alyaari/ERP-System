using ERP.Domain.Entities;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using ERP.Application.Features.Purchasing.Vendors.Specifications;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Application.Features.Purchasing.Vendors.Commands.Create;

public record CreateVendorCommand : IRequest<Guid>
{
    public string VendorCode { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? TaxNumber { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
}

public class CreateVendorCommandHandler : IRequestHandler<CreateVendorCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateVendorCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateVendorCommand request, CancellationToken cancellationToken)
    {
        var codeExists = await _unitOfWork.Repository<Vendor>().CountAsync(new VendorByCodeSpecification(request.VendorCode));
        if (codeExists > 0)
        {
            throw new BusinessException($"كود المورد '{request.VendorCode}' مستخدم بالفعل.");
        }

        // 1. البحث عن الحساب الرئيسي للموردين (كوده 2101 - ذمم الموردين)
        var allAccounts = await _unitOfWork.Repository<Account>().ListAllAsync();
        var parentAccount = allAccounts.FirstOrDefault(a => a.AccountCode == "2101");
        
        if (parentAccount == null)
        {
            throw new BusinessException("الحساب الرئيسي للموردين (ذمم الموردين - 2101) غير موجود في شجرة الحسابات. يرجى تهيئة الشجرة أولاً.");
        }

        // 2. توليد كود الحساب الفرعي الجديد بشكل تلقائي ومتسلسل
        var subAccounts = allAccounts.Where(a => a.ParentAccountId == parentAccount.Id).ToList();
        string newAccountCode;
        if (subAccounts.Any())
        {
            var maxCodeStr = subAccounts.Select(a => a.AccountCode).Max();
            if (long.TryParse(maxCodeStr, out long maxCode))
            {
                newAccountCode = (maxCode + 1).ToString();
            }
            else
            {
                newAccountCode = parentAccount.AccountCode + (subAccounts.Count + 1).ToString("D2");
            }
        }
        else
        {
            newAccountCode = parentAccount.AccountCode + "01"; // أول حساب فرعي مثل 210101
        }

        // 3. إنشاء الحساب التفصيلي في الدليل المحاسبي للمورد الجديد
        var newAccount = new Account(
            Guid.NewGuid(),
            newAccountCode,
            $"حساب مورد - {request.NameAr}",
            $"Vendor A/C - {request.NameEn}",
            parentAccount.AccountType,
            true, // IsDetail = true (حساب تحليلي)
            parentAccount.CurrencyId,
            parentAccount.Id
        );

        _unitOfWork.Repository<Account>().Add(newAccount);

        // 4. إنشاء المورد وربطه تلقائياً بالحساب المنشأ
        var vendor = new Vendor(
            Guid.NewGuid(),
            request.VendorCode,
            request.NameAr,
            request.NameEn,
            newAccount.Id,
            request.TaxNumber,
            request.Phone,
            request.Email
        );

        _unitOfWork.Repository<Vendor>().Add(vendor);
        
        // إتمام الحفظ دفعة واحدة لضمان تماسك البيانات
        await _unitOfWork.Complete();

        return vendor.Id;
    }
}
