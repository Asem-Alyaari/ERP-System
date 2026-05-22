using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Accounting.FiscalPeriods.Commands.Create;

public record CreateFiscalPeriodCommand : IRequest<Guid>
{
    public string YearName { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}

public class CreateFiscalPeriodCommandHandler : IRequestHandler<CreateFiscalPeriodCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateFiscalPeriodCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateFiscalPeriodCommand request, CancellationToken cancellationToken)
    {
        // 1. Validation: Start date must be before End date
        if (request.StartDate >= request.EndDate)
        {
            throw new Exception("تاريخ البدء يجب أن يكون قبل تاريخ الانتهاء.");
        }

        if (string.IsNullOrWhiteSpace(request.YearName))
        {
            throw new Exception("يجب إدخال اسم السنة المالية.");
        }

        // 2. Validation: Overlap check
        var existingPeriods = await _unitOfWork.Repository<FiscalPeriod>().ListAllAsync();
        foreach (var p in existingPeriods)
        {
            // Overlap condition: StartA <= EndB && StartB <= EndA
            if (request.StartDate <= p.EndDate && p.StartDate <= request.EndDate)
            {
                throw new Exception($"هذه الفترة تتداخل مع فترة مالية قائمة بالفعل: {p.YearName} ({p.StartDate:yyyy-MM-dd} - {p.EndDate:yyyy-MM-dd}).");
            }
        }

        var period = new FiscalPeriod(
            Guid.NewGuid(),
            request.YearName,
            request.StartDate,
            request.EndDate
        );

        _unitOfWork.Repository<FiscalPeriod>().Add(period);
        await _unitOfWork.Complete();

        return period.Id;
    }
}
