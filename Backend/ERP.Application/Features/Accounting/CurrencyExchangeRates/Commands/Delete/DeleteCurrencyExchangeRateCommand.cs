using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Accounting.CurrencyExchangeRates.Commands.Delete;

public record DeleteCurrencyExchangeRateCommand(Guid Id) : IRequest;

public class DeleteCurrencyExchangeRateCommandHandler : IRequestHandler<DeleteCurrencyExchangeRateCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCurrencyExchangeRateCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteCurrencyExchangeRateCommand request, CancellationToken cancellationToken)
    {
        var exchangeRate = await _unitOfWork.Repository<CurrencyExchangeRate>().GetByIdAsync(request.Id);
        if (exchangeRate == null)
        {
            throw new Exception("Currency exchange rate not found.");
        }

        _unitOfWork.Repository<CurrencyExchangeRate>().Delete(exchangeRate);
        await _unitOfWork.Complete();
    }
}
