using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Accounting.FiscalPeriods.Queries.GetFiscalPeriods;

public record GetFiscalPeriodsQuery : IRequest<List<FiscalPeriodDto>>;

public class GetFiscalPeriodsQueryHandler : IRequestHandler<GetFiscalPeriodsQuery, List<FiscalPeriodDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetFiscalPeriodsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<FiscalPeriodDto>> Handle(GetFiscalPeriodsQuery request, CancellationToken cancellationToken)
    {
        var periods = await _unitOfWork.Repository<FiscalPeriod>().ListAllAsync();

        return periods
            .OrderByDescending(p => p.StartDate)
            .Select(p => new FiscalPeriodDto
            {
                Id = p.Id,
                YearName = p.YearName,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsClosed = p.IsClosed
            })
            .ToList();
    }
}
