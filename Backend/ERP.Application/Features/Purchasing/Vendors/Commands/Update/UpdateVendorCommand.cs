using ERP.Domain.Entities;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using ERP.Application.Features.Purchasing.Vendors.Specifications;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Application.Features.Purchasing.Vendors.Commands.Update;

public record UpdateVendorCommand : IRequest<MediatR.Unit>
{
    public Guid Id { get; init; }
    public string VendorCode { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? TaxNumber { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
}

public class UpdateVendorCommandHandler : IRequestHandler<UpdateVendorCommand, MediatR.Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateVendorCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(UpdateVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _unitOfWork.Repository<Vendor>().GetByIdAsync(request.Id);

        if (vendor == null)
        {
            throw new BusinessException($"المورد المطلوب غير موجود.");
        }

        var codeExists = await _unitOfWork.Repository<Vendor>().CountAsync(new VendorByCodeForUpdateSpecification(request.VendorCode, request.Id));
        if (codeExists > 0)
        {
            throw new BusinessException($"كود المورد '{request.VendorCode}' مستخدم بالفعل بواسطة مورد آخر.");
        }

        // تحديث تفاصيل الحساب المحاسبي المرتبط تلقائياً ليبقى متطابقاً مع اسم المورد الجديد
        var account = await _unitOfWork.Repository<Account>().GetByIdAsync(vendor.AccountId);
        if (account != null)
        {
            account.UpdateDetails(
                $"حساب مورد - {request.NameAr}",
                $"Vendor A/C - {request.NameEn}",
                true
            );
            _unitOfWork.Repository<Account>().Update(account);
        }

        vendor.Update(
            request.VendorCode,
            request.NameAr,
            request.NameEn,
            vendor.AccountId, // الاحتفاظ بالحساب المرتبط تلقائياً
            request.TaxNumber,
            request.Phone,
            request.Email
        );

        _unitOfWork.Repository<Vendor>().Update(vendor);
        await _unitOfWork.Complete();

        return MediatR.Unit.Value;
    }
}
