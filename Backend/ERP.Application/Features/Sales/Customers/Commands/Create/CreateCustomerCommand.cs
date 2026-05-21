using ERP.Domain.Entities;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using ERP.Application.Features.Sales.Customers.Specifications;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Application.Features.Sales.Customers.Commands.Create;

public record CreateCustomerCommand : IRequest<Guid>
{
    public string CustomerCode { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? TaxNumber { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
}

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var codeExists = await _unitOfWork.Repository<Customer>().CountAsync(new CustomerByCodeSpecification(request.CustomerCode));
        if (codeExists > 0)
        {
            throw new BusinessException($"كود العميل '{request.CustomerCode}' مستخدم بالفعل.");
        }

        // 1. البحث عن الحساب الرئيسي للعملاء (كوده 1102 - ذمم العملاء)
        var allAccounts = await _unitOfWork.Repository<Account>().ListAllAsync();
        var parentAccount = allAccounts.FirstOrDefault(a => a.AccountCode == "1102");
        
        if (parentAccount == null)
        {
            throw new BusinessException("الحساب الرئيسي للعملاء (ذمم العملاء - 1102) غير موجود في شجرة الحسابات. يرجى تهيئة الشجرة أولاً.");
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
            newAccountCode = parentAccount.AccountCode + "01"; // أول حساب فرعي مثل 110201
        }

        // 3. إنشاء الحساب التفصيلي في الدليل المحاسبي للعميل الجديد
        var newAccount = new Account(
            Guid.NewGuid(),
            newAccountCode,
            $"حساب عميل - {request.NameAr}",
            $"Customer A/C - {request.NameEn}",
            parentAccount.AccountType,
            true, // IsDetail = true (حساب تحليلي)
            parentAccount.CurrencyId,
            parentAccount.Id
        );

        _unitOfWork.Repository<Account>().Add(newAccount);

        // 4. إنشاء العميل وربطه تلقائياً بالحساب المنشأ
        var customer = new Customer(
            Guid.NewGuid(),
            request.CustomerCode,
            request.NameAr,
            request.NameEn,
            newAccount.Id,
            request.TaxNumber,
            request.Phone,
            request.Email
        );

        _unitOfWork.Repository<Customer>().Add(customer);
        
        // إتمام الحفظ دفعة واحدة لضمان تماسك البيانات
        await _unitOfWork.Complete();

        return customer.Id;
    }
}
