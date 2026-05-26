using MediatR;
using ERP.Domain.Entities;
using ERP.Domain.Repositories;

namespace ERP.Application.Features.Expenses.Bills.Queries.GetAll;

public record GetAllExpenseBillsQuery : IRequest<List<ExpenseBillMaster>>;

public class GetAllExpenseBillsQueryHandler : IRequestHandler<GetAllExpenseBillsQuery, List<ExpenseBillMaster>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllExpenseBillsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ExpenseBillMaster>> Handle(GetAllExpenseBillsQuery request, CancellationToken cancellationToken)
    {
        var bills = await _unitOfWork.Repository<ExpenseBillMaster>().ListAllAsync();
        return bills.ToList();
    }
}
