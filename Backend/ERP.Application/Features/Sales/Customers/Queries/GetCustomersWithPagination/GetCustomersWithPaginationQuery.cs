using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Application.Features.Sales.Customers.Specifications;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Application.Features.Sales.Customers.Queries.GetCustomersWithPagination;

public record GetCustomersWithPaginationQuery : IRequest<CustomersPagedResponse>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
}

public class CustomersPagedResponse
{
    public List<CustomerDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class GetCustomersWithPaginationQueryHandler : IRequestHandler<GetCustomersWithPaginationQuery, CustomersPagedResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCustomersWithPaginationQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomersPagedResponse> Handle(GetCustomersWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.PageNumber - 1) * request.PageSize;
        
        var spec = new CustomerFilterSpecification(request.SearchTerm, skip, request.PageSize);
        var countSpec = new CustomerFilterCountSpecification(request.SearchTerm);

        var items = await _unitOfWork.Repository<Customer>().ListAsync(spec);
        var totalCount = await _unitOfWork.Repository<Customer>().CountAsync(countSpec);

        return new CustomersPagedResponse
        {
            Items = items.Select(c => new CustomerDto
            {
                Id = c.Id,
                CustomerCode = c.CustomerCode,
                NameAr = c.NameAr,
                NameEn = c.NameEn,
                AccountId = c.AccountId,
                AccountNameAr = c.Account?.AccountNameAr,
                AccountNameEn = c.Account?.AccountNameEn,
                TaxNumber = c.TaxNumber,
                Phone = c.Phone,
                Email = c.Email
            }).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
