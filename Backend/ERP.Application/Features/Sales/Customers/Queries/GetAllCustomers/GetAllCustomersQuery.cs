using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Application.Features.Sales.Customers.Specifications;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Application.Features.Sales.Customers.Queries.GetAllCustomers;

public record GetAllCustomersQuery : IRequest<List<CustomerDto>>;

public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, List<CustomerDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllCustomersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var spec = new CustomersWithAccountSpecification();
        var customers = await _unitOfWork.Repository<Customer>().ListAsync(spec);
        
        return customers.Select(c => new CustomerDto
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
        }).ToList();
    }
}
