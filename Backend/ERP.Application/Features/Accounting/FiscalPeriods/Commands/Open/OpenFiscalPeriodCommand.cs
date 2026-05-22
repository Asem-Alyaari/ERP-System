using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Accounting.FiscalPeriods.Commands.Open;

public record OpenFiscalPeriodCommand(Guid Id) : IRequest<bool>;

public class OpenFiscalPeriodCommandHandler : IRequestHandler<OpenFiscalPeriodCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public OpenFiscalPeriodCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(OpenFiscalPeriodCommand request, CancellationToken cancellationToken)
    {
        var period = await _unitOfWork.Repository<FiscalPeriod>().GetByIdAsync(request.Id);
        if (period == null)
        {
            throw new Exception("الفترة المالية غير موجودة.");
        }

        period.OpenPeriod();
        _unitOfWork.Repository<FiscalPeriod>().Update(period);
        await _unitOfWork.Complete();

        return true;
    }
}
