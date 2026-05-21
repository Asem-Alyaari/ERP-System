using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Application.Features.Purchasing.Vendors.Specifications;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Application.Features.Purchasing.Vendors.Queries.GetVendorById;

public record GetVendorByIdQuery(Guid Id) : IRequest<VendorDto?>;

public class GetVendorByIdQueryHandler : IRequestHandler<GetVendorByIdQuery, VendorDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetVendorByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<VendorDto?> Handle(GetVendorByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new VendorsWithAccountSpecification(request.Id);
        var vendors = await _unitOfWork.Repository<Vendor>().ListAsync(spec);
        var vendor = vendors.FirstOrDefault();

        if (vendor == null) return null;

        return new VendorDto
        {
            Id = vendor.Id,
            VendorCode = vendor.VendorCode,
            NameAr = vendor.NameAr,
            NameEn = vendor.NameEn,
            AccountId = vendor.AccountId,
            AccountNameAr = vendor.Account?.AccountNameAr,
            AccountNameEn = vendor.Account?.AccountNameEn,
            TaxNumber = vendor.TaxNumber,
            Phone = vendor.Phone,
            Email = vendor.Email
        };
    }
}
