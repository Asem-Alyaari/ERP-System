using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Accounting.FiscalPeriods.Commands.Delete;

public record DeleteFiscalPeriodCommand(Guid Id) : IRequest<bool>;

public class DeleteFiscalPeriodCommandHandler : IRequestHandler<DeleteFiscalPeriodCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteFiscalPeriodCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteFiscalPeriodCommand request, CancellationToken cancellationToken)
    {
        var period = await _unitOfWork.Repository<FiscalPeriod>().GetByIdAsync(request.Id);
        if (period == null)
        {
            throw new Exception("الفترة المالية غير موجودة.");
        }

        if (period.IsClosed)
        {
            throw new Exception("لا يمكن حذف فترة مالية مغلقة بالفعل. يرجى فتحها أولاً إذا لزم الأمر.");
        }

        _unitOfWork.Repository<FiscalPeriod>().Delete(period);
        await _unitOfWork.Complete();

        return true;
    }
}
