using MediatR;
using ERP.Domain.Entities;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using ERP.Application.Features.Expenses.Bills.Specifications;

namespace ERP.Application.Features.Expenses.Bills.Queries.GetById;

public record GetExpenseBillByIdQuery(Guid Id) : IRequest<ExpenseBillMaster>;

public class GetExpenseBillByIdQueryHandler : IRequestHandler<GetExpenseBillByIdQuery, ExpenseBillMaster>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetExpenseBillByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ExpenseBillMaster> Handle(GetExpenseBillByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new ExpenseBillWithDetailsSpecification(request.Id);
        var bill = await _unitOfWork.Repository<ExpenseBillMaster>().GetEntityWithSpec(spec);

        if (bill == null)
            throw new BusinessException("فاتورة المصروفات غير موجودة.");

        return bill;
    }
}
