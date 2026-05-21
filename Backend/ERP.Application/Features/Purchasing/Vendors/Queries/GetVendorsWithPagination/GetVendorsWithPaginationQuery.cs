using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Application.Features.Purchasing.Vendors.Specifications;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Application.Features.Purchasing.Vendors.Queries.GetVendorsWithPagination;

public record GetVendorsWithPaginationQuery : IRequest<VendorsPagedResponse>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
}

public class VendorsPagedResponse
{
    public List<VendorDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class GetVendorsWithPaginationQueryHandler : IRequestHandler<GetVendorsWithPaginationQuery, VendorsPagedResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetVendorsWithPaginationQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<VendorsPagedResponse> Handle(GetVendorsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.PageNumber - 1) * request.PageSize;
        
        var spec = new VendorFilterSpecification(request.SearchTerm, skip, request.PageSize);
        var countSpec = new VendorFilterCountSpecification(request.SearchTerm);

        var items = await _unitOfWork.Repository<Vendor>().ListAsync(spec);
        var totalCount = await _unitOfWork.Repository<Vendor>().CountAsync(countSpec);

        return new VendorsPagedResponse
        {
            Items = items.Select(v => new VendorDto
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
            }).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
