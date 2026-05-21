using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Application.Features.Sales.Customers.Specifications;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Application.Features.Sales.Customers.Queries.GetCustomerById;

public record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerDto?>;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCustomerByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new CustomersWithAccountSpecification(request.Id);
        var customers = await _unitOfWork.Repository<Customer>().ListAsync(spec);
        var customer = customers.FirstOrDefault();

        if (customer == null) return null;

        return new CustomerDto
        {
            Id = customer.Id,
            CustomerCode = customer.CustomerCode,
            NameAr = customer.NameAr,
            NameEn = customer.NameEn,
            AccountId = customer.AccountId,
            AccountNameAr = customer.Account?.AccountNameAr,
            AccountNameEn = customer.Account?.AccountNameEn,
            TaxNumber = customer.TaxNumber,
            Phone = customer.Phone,
            Email = customer.Email
        };
    }
}
