using ERP.Domain.Entities;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using ERP.Application.Features.Sales.Customers.Specifications;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Application.Features.Sales.Customers.Commands.Update;

public record UpdateCustomerCommand : IRequest<MediatR.Unit>
{
    public Guid Id { get; init; }
    public string CustomerCode { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? TaxNumber { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
}

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, MediatR.Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCustomerCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(request.Id);

        if (customer == null)
        {
            throw new BusinessException($"العميل المطلوب غير موجود.");
        }

        var codeExists = await _unitOfWork.Repository<Customer>().CountAsync(new CustomerByCodeForUpdateSpecification(request.CustomerCode, request.Id));
        if (codeExists > 0)
        {
            throw new BusinessException($"كود العميل '{request.CustomerCode}' مستخدم بالفعل بواسطة عميل آخر.");
        }

        // تحديث تفاصيل الحساب المحاسبي المرتبط تلقائياً ليبقى متطابقاً مع اسم العميل الجديد
        var account = await _unitOfWork.Repository<Account>().GetByIdAsync(customer.AccountId);
        if (account != null)
        {
            account.UpdateDetails(
                $"حساب عميل - {request.NameAr}",
                $"Customer A/C - {request.NameEn}",
                true
            );
            _unitOfWork.Repository<Account>().Update(account);
        }

        customer.Update(
            request.CustomerCode,
            request.NameAr,
            request.NameEn,
            customer.AccountId, // الاحتفاظ بالحساب المرتبط تلقائياً
            request.TaxNumber,
            request.Phone,
            request.Email
        );

        _unitOfWork.Repository<Customer>().Update(customer);
        await _unitOfWork.Complete();

        return MediatR.Unit.Value;
    }
}
