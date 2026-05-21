using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Application.Features.Accounting.Currencies.Specifications;
using MediatR;

namespace ERP.Application.Features.Accounting.Currencies.Commands.Delete;

public record DeleteCurrencyCommand(Guid Id) : IRequest<MediatR.Unit>;

public class DeleteCurrencyCommandHandler : IRequestHandler<DeleteCurrencyCommand, MediatR.Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCurrencyCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(DeleteCurrencyCommand request, CancellationToken cancellationToken)
    {
        var currency = await _unitOfWork.Repository<Currency>().GetByIdAsync(request.Id);

        if (currency == null)
        {
            throw new Exception($"Currency with ID {request.Id} not found");
        }

        // 1. Cannot delete local currency
        if (currency.IsLocal)
        {
            throw new Exception("Cannot delete the local currency.");
        }

        // 2. Check for linked accounts
        var accountsCount = await _unitOfWork.Repository<Account>().CountAsync(
            new AccountsByCurrencyIdSpecification(request.Id)
        );
        if (accountsCount > 0)
        {
            throw new Exception("Cannot delete currency linked to chart of accounts.");
        }

        // 3. Check for journal entry lines
        var transactionLinesCount = await _unitOfWork.Repository<JournalEntryLine>().CountAsync(
            new JournalEntryLinesByCurrencyIdSpecification(request.Id)
        );
        if (transactionLinesCount > 0)
        {
            throw new Exception("Cannot delete currency associated with financial transaction history.");
        }

        _unitOfWork.Repository<Currency>().Delete(currency);
        await _unitOfWork.Complete();

        return MediatR.Unit.Value;
    }
}
