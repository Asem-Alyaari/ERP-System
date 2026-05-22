using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Accounting.FiscalPeriods.Commands.Close;

public record CloseFiscalPeriodCommand(Guid Id) : IRequest<bool>;

public class CloseFiscalPeriodCommandHandler : IRequestHandler<CloseFiscalPeriodCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public CloseFiscalPeriodCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(CloseFiscalPeriodCommand request, CancellationToken cancellationToken)
    {
        var period = await _unitOfWork.Repository<FiscalPeriod>().GetByIdAsync(request.Id);
        if (period == null)
        {
            throw new Exception("الفترة المالية غير موجودة.");
        }

        period.ClosePeriod();
        _unitOfWork.Repository<FiscalPeriod>().Update(period);
        await _unitOfWork.Complete();

        return true;
    }
}
