using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Application.Features.Purchasing.Vendors.Specifications;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Application.Features.Purchasing.Vendors.Queries.GetAllVendors;

public record GetAllVendorsQuery : IRequest<List<VendorDto>>;

public class GetAllVendorsQueryHandler : IRequestHandler<GetAllVendorsQuery, List<VendorDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllVendorsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<VendorDto>> Handle(GetAllVendorsQuery request, CancellationToken cancellationToken)
    {
        var spec = new VendorsWithAccountSpecification();
        var vendors = await _unitOfWork.Repository<Vendor>().ListAsync(spec);
        
        return vendors.Select(v => new VendorDto
        {
            Id = v.Id,
            VendorCode = v.VendorCode,
            NameAr = v.NameAr,
            NameEn = v.NameEn,
            AccountId = v.AccountId,
            AccountNameAr = v.Account?.AccountNameAr,
            AccountNameEn = v.Account?.AccountNameEn,
            TaxNumber = v.TaxNumber,
            Phone = v.Phone,
            Email = v.Email
        }).ToList();
    }
}
